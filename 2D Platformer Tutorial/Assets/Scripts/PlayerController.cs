using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float airWalkSpeed = 7f;

    public float jumpImpulse = 8f;
    public int jumpCounter = 0;

    private bool canDash = true;
    public float dashingImpulse = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    Vector2 moveInput;
    TouchingDirections touchingDirections;

    //private static bool isPlayerAlive = true;

    //private GameObject currentOneWayPlatform;
    //private float secondsToFallThroughPlatform = 0.25f;


    public float CurrentMoveSpeed { get
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


    Rigidbody2D rb;
    [SerializeField] private CapsuleCollider2D playerCollider;
    Animator animator;
    [SerializeField] private TrailRenderer trailRenderer;

    public string oneWayPlatformLayerName = "OneWayPlatform";
    public string playerLayerName = "Player";


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
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

        FallThroughPlatform();
    }

    private void FixedUpdate()
    {
        if (IsDashing)
        {   
            // Prevent movement while dashing
            return;
        }

        rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y); // * Time.fixedDeltaTime already handled by RigitBody
        
        animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
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
        // TODO check if alive as well

        // Count jumps
        if (context.started)
        {
            jumpCounter += 1;
        }

        // Normal jump
        if (context.started && touchingDirections.IsGrounded)
        {
            animator.SetTrigger(AnimationStrings.jump);
            rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);

        // Double jump when on wall
        } else if (context.started && touchingDirections.IsOnWall)
        {
            float originalGravity = rb.gravityScale;

            //if (jumpCounter <= 1)
            {
                animator.SetTrigger(AnimationStrings.jump);
                rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);

               // increase gravity at peak of jump curve
               if(rb.velocity.y < 0)
                {
                    rb.gravityScale = originalGravity * 1.5f;
                }
            }
        }


        // Tapping for little jumps
        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

    }


    // Implement a dash
    private IEnumerator Dash()
    {
        canDash = false;
        IsDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingImpulse, 0);
        trailRenderer.emitting = true;
        yield return new WaitForSeconds(dashingTime);

        trailRenderer.emitting = false;
        rb.gravityScale = originalGravity;
        IsDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash)
        {
            StartCoroutine(Dash());
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

    /* old
    public void OnFallThroughPlatform(InputAction.CallbackContext context, Collision2D collision)
    {
        currentOneWayPlatform = collision.gameObject;

        if (context.started)
        {
            if(currentOneWayPlatform != null)
            {
                //StartCoroutine(FallThroughPlatform());
            }
        }

        currentOneWayPlatform = null;
    } */


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

    /*
    public static void AnnouncePlayerDeath(bool isPlayerDead = true)
    {
        isPlayerAlive = !isPlayerDead;
    }

    public static bool IsPlayerAlive()
    {
        return isPlayerAlive;
    } 
    */

}
