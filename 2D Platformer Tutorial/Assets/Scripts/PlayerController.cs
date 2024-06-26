using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Based on https://www.youtube.com/watch?v=oxiPWg8cdRM&ab_channel=Chris%27Tutorials
[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float airWalkSpeed = 7f;

    [Header("Jump")]
    public float jumpImpulse = 8f;
    public int jumpCounter = 0;
    private bool canWallJump = true;

    public float fallDamageThreshold = 15f; // falling speed threshhold; not distance
    private bool playerDiesFromFallDamage = false;
    private bool canTakeFallDamage = true;

    [Header("Dash")]
    private bool canDash = true;
    public float dashingImpulse = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    [Header("Attack")]
    public float attackRange = 2; // 0.6f;
    public int attackDamage = 1;
    public float attackCooldown = 0.6f;
    public string destroyableTag = "DestroyableObject";
    private bool canAttack = true;


    Vector2 moveInput;
    TouchingDirections touchingDirections;

    private PuzzlePlatform[] puzzlePlatforms;

    private float puzzle_airWalkSpeed;
    private float puzzle_jumpImpulse;
    private float original_airWalkSpeed;
    private float original_jumpImpulse;

    //private GameObject currentOneWayPlatform;
    //private float secondsToFallThroughPlatform = 0.25f;


    public float CurrentMoveSpeed { get
        {
            if(CanMove) // can restrict movement here with: CanMove
            {
                if (IsMoving && !touchingDirections.IsOnWall && !touchingDirections.IsOnCeiling) // already added isOnCeiling
                {
                    if (touchingDirections.IsGrounded)
                    {
                        if (IsRunning)
                        {
                            return runSpeed;
                        }
                        else
                        {
                            return walkSpeed;
                        }
                    } else
                    {
                        // Air move
                        return airWalkSpeed;
                    }
                }
                else
                {
                    return 0; // idle
                }

            }
            else
            {
                return 0; // movement locked (when attacking)
            }

        }
    }

    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationStrings.canMove);
        }
    }

    [SerializeField]
    private bool _isMoving = false;
    public bool IsMoving { get 
        {
            return _isMoving;
        } 
        private set
        {
            _isMoving = value;
            animator.SetBool(AnimationStrings.isMoving, value); // link to animator parameter
        } 
    }

    [SerializeField]
    private bool _isRunning = false;
    public bool IsRunning
    {
        get
        {
            return _isRunning;
        }
        private set
        {
            _isRunning = value;
            animator.SetBool(AnimationStrings.isRunning, value); // link to animator parameter
        }
    }

    [SerializeField]
    private bool _isFacingRight = true;
    public bool IsFacingRight { get
        {
            return _isFacingRight;
        }
        private set
        {
            if (_isFacingRight != value)
            {
                // Flip the local scale to make the player face the opposite direction
                transform.localScale *= new Vector2(-1, 1);
            }

            _isFacingRight = value;
        }
    }

    [SerializeField]
    private bool _isDashing = false;
    public bool IsDashing
    {
        get
        {
            return _isDashing;
        }
        private set
        {
            _isDashing = value;
            animator.SetBool(AnimationStrings.isDashing, value); // could also be SetTrigger
        }
    }

    [SerializeField]
    private bool _isFalling = false;
    public bool IsFalling
    {
        get
        {
            return _isFalling;
        }
        private set
        {
            _isFalling = value;
            //animator.SetBool(AnimationStrings.yVelocity, value); // handled differently
        }
    }


    Rigidbody2D rb;
    [SerializeField] private CapsuleCollider2D playerCollider;
    Animator animator;
    [SerializeField] private TrailRenderer trailRenderer;

    public string oneWayPlatformLayerName = "OneWayPlatform";
    public string playerLayerName = "Player";


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // newly added because of the dash, was discrete before
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();

        puzzlePlatforms = FindObjectsOfType<PuzzlePlatform>();

        puzzle_airWalkSpeed = airWalkSpeed / 2;
        puzzle_jumpImpulse = jumpImpulse / 1.4f;
        original_airWalkSpeed = airWalkSpeed;
        original_jumpImpulse = jumpImpulse;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDashing)
        {
            // Prevent movement while dashing
            return;
        }

        // Count jumps until hitting the ground or wall
        if (touchingDirections.IsGrounded)
        {
            jumpCounter = 0;
        }

        //FallThroughPlatform(); // TODO re-activate ?
    }

    private void FixedUpdate()
    {
        if (IsDashing)
        {   
            // Prevent movement while dashing
            return;
        }


        // track falling state
        rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y); // * Time.fixedDeltaTime already handled by RigitBody
        animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);

        // apply fall damage
        IsFalling = rb.velocity.y < 0 && !touchingDirections.IsGrounded;
        if (IsFalling)
        {
            // check the speed threshold
            if (Mathf.Abs(rb.velocity.y) > fallDamageThreshold && canTakeFallDamage)
            {
                playerDiesFromFallDamage = true;
                canDash = false; // disable dash
            }
        }

        if (touchingDirections.IsGrounded && playerDiesFromFallDamage)
        {
            RespawnController.instance.AnnouncePlayerDeath();
            canDash = true;
            playerDiesFromFallDamage = false;
        }

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        IsMoving = moveInput != Vector2.zero;

        SetFacingDirection(moveInput);
    }
    
    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !IsFacingRight)
        {
            // Face the right
            IsFacingRight = true;
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            // Face the left
            IsFacingRight = false;
        }
        else if (moveInput.x == 0)
        {
            // Keep the current direction when not moving horizontally
            // This prevents unnecessary flipping while idle
            return;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsRunning = true;

        } else if (context.canceled) 
        {
            IsRunning = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (true) // can restrict movement here with: CanMove
        {
            // Count jumps
            if (context.started)
            {
                jumpCounter += 1;

                // switch "puzzle" platforms on jump for all found PuzzlePlatform instances
                foreach (PuzzlePlatform puzzlePlatform in puzzlePlatforms)
                {
                    if (touchingDirections.IsGrounded)
                    {
                        puzzlePlatform.SwitchPlatforms();
                    }
                }

            }

            // Normal jump
            if (context.started && touchingDirections.IsGrounded)
            {
                animator.SetTrigger(AnimationStrings.jump);
                rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);

                // Double jump when on wall
            }
            else if (context.started && touchingDirections.IsOnWall)
            {
                if (canWallJump)
                {
                    //PerformWallJump(); // TODO re-activate Wall jump ?
                }
            }


            // Tapping for little jumps
            if (context.canceled && rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }
    }

    // Wall jump
    private void PerformWallJump()
    {
        float originalGravity = rb.gravityScale;

        //if (jumpCounter <= 1)
        {
            animator.SetTrigger(AnimationStrings.jump);
            rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);

            // increase gravity at peak of jump curve
            if (rb.velocity.y < 0)
            {
                rb.gravityScale = originalGravity * 1.5f;
            }
        }
    }

    // Implement a dash
    private IEnumerator Dash()
    {
        bool wasInterrupted = false;

        canDash = false;
        IsDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // do not allow dashing through any obstacles

        Vector2 dashVelocity = new Vector2(transform.localScale.x * dashingImpulse, 0); // y velocity is 0
        //Vector2 dashDirection = new Vector2(transform.localScale.x, 0).normalized;
        //float dashDistance = dashingImpulse * dashingTime;
        int layerMask = LayerMask.GetMask("Ground"); // "Default",  removed

        trailRenderer.emitting = true;

        float dashTimeRemaining = dashingTime;
        while (dashTimeRemaining > 0)
        {
            rb.velocity = dashVelocity;

            RaycastHit2D hit = Physics2D.Raycast(rb.position, dashVelocity.normalized, dashVelocity.magnitude * Time.fixedDeltaTime, layerMask);

            if (hit.collider != null)
            {
                // could do more, but not necessary
                //rb.velocity = Vector2.zero;
                //rb.position = hit.point;
                break;
            }

            yield return new WaitForFixedUpdate();
            dashTimeRemaining -= Time.fixedDeltaTime;

            // check if dash is interrupted
            if (!IsDashing)
            {
                wasInterrupted = true;
                break;
            }
        }
        //rb.velocity = new Vector2(transform.localScale.x * dashingImpulse, 0); // y velocity is 0
        //yield return new WaitForSeconds(dashingTime);

        trailRenderer.emitting = false;
        rb.gravityScale = originalGravity;
        //rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete; // set back
        IsDashing = false;
        yield return new WaitForSeconds(dashingCooldown);

        if (wasInterrupted)
        {
            canDash = false;
        } else
        {
            canDash = true;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && canAttack)
        {
            animator.SetTrigger(AnimationStrings.attack);
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        canAttack = false;

        Vector2 attackDirection = IsFacingRight ? Vector2.right : Vector2.left;
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * attackRange;
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPosition, attackRange);

        // Damage or destroy them if they have the destroyableTag
        foreach (Collider2D hit in hitObjects)
        {
            // fetch object to remove hp

            DestroyableObject destroyableObject = hit.GetComponent<DestroyableObject>();
            if (destroyableObject != null)
            {
                // check if the object is even close --> only look at x (because the attack range is by default very large for the projectiles; in this case only x matters)
                float distance = Mathf.Abs(transform.position.x - destroyableObject.transform.position.x);
                if (distance <= attackRange)
                {
                    destroyableObject.TakeDamage(attackDamage);
                }
            }

            // handle projectiles
            AttackBossProjectiles(hit);

            // alternatively just destroy in one hit
            if (hit.CompareTag(destroyableTag))
            {
                Destroy(hit.gameObject);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void AttackBossProjectiles(Collider2D hit)
    {
        QuickProjectileScript quickProjectile = hit.GetComponent<QuickProjectileScript>();
        if (quickProjectile != null)
        {
            quickProjectile.OnPlayerAttackMove();
        }

        QuickProjectileCollisionCourseScript quickProjectileCollisionCourseScript = hit.GetComponent<QuickProjectileCollisionCourseScript>();
        if (quickProjectileCollisionCourseScript != null)
        {
            quickProjectileCollisionCourseScript.OnPlayerAttackMove();
        }

        SlowProjectileScript slowProjectileScript = hit.GetComponent<SlowProjectileScript>();
        if (slowProjectileScript != null)
        {
            slowProjectileScript.OnPlayerAttackMove();
        }
    }


    // Implement One Way Platform interactions
    private void FallThroughPlatform() // old: IEnumerator
    {
        /* old
        BoxCollider2D platformCollider = currentOneWayPlatform.GetComponent<BoxCollider2D>();

        animator.SetTrigger(AnimationStrings.fallThroughPlatform);
        Physics2D.IgnoreCollision(playerCollider, platformCollider);
        yield return new WaitForSeconds(secondsToFallThroughPlatform);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false); */

        if (Input.GetAxis("Vertical") < 0)
        {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayerName), LayerMask.NameToLayer(oneWayPlatformLayerName), true);
            //yield return new WaitForSeconds(secondsToFallThroughPlatform);
        }
        else
        {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayerName), LayerMask.NameToLayer(oneWayPlatformLayerName), false);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // handle collision with hostile object
        if (collision.gameObject.CompareTag("Hostile") || collision.gameObject.layer == LayerMask.NameToLayer("Hostile"))
        {
            gameObject.SetActive(false);
            RespawnController.instance.AnnouncePlayerDeath();
        }

        // handle being crushed between 2 objects
        bool collidedWithCrushObstacle1 = false;
        bool collidedWithCrushObstacle2 = true; // TODO fix this with = false
        // Check for collision with Obstacle1
        if (collision.gameObject.CompareTag("PlayerCrushOne"))
        {
            collidedWithCrushObstacle1 = true;
        }
        // Check for collision with Obstacle2
        if (collision.gameObject.CompareTag("PlayerCrushTwo"))
        {
            collidedWithCrushObstacle2 = true;
        }

        // If collided with both obstacles, set the player inactive
        if (collidedWithCrushObstacle1 && collidedWithCrushObstacle2)
        {
            gameObject.SetActive(false);
            RespawnController.instance.AnnouncePlayerDeath();
        }
    }



    public void PrepareForPuzzleSection()
    {
        // interrupt dash
        IsDashing = false;

        canWallJump = false;
        canDash = false;
        canTakeFallDamage = false;

        airWalkSpeed = puzzle_airWalkSpeed;
        jumpImpulse = puzzle_jumpImpulse;
    }

    public void ResetAfterPuzzleSection()
    {
        canWallJump = true;
        canDash = true;
        canTakeFallDamage = true;

        airWalkSpeed = original_airWalkSpeed;
        jumpImpulse = original_jumpImpulse;
    }

}
