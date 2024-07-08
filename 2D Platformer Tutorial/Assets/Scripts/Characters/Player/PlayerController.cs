using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Windows.Kinect; 

// Based on https://www.youtube.com/watch?v=oxiPWg8cdRM&ab_channel=Chris%27Tutorials
[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerController : MonoBehaviour
{
    [Header("Walk")]
    [Range(1f, 100f)] public float maxWalkSpeed = 12.5f;
    [Range(0f, 50f)] public float groundAcceleration = 5f;
    [Range(0f, 50f)] public float groundDeceleration = 20f;
    [Range(1f, 100f)] public float maxAirWalkSpeed = 7f;
    [Range(0f, 50f)] public float airAcceleration = 5f;
    [Range(0f, 50f)] public float airDeceleration = 5f;
    [Range(0f, 50f)] public float frictionAmount = 1f;
    private Vector2 _moveVelocity;
    private Vector2 moveInput; // movement input
    private float stopMovementTolerance = 0.05f;
    private float stopMovementToleranceTimer = 0f;
    public float baseDownForceMagnitude = 30f;
    public float maxDownForceMagnitude = 60f;


    [Header("Run")]
    [Range(1f, 100f)] public float maxRunSpeed = 20f;

    
    [Header("Jump")]
    public float jumpImpulse = 8f; // could be named jumpHeight
    [Range(1f, 100f)] public float maxAirJumpSpeed = 7f;
    public float maxIdleForceIncrease = 1.1f;
    public float jumpGravityScaleFactor = 1.5f;
    private float jumpGravityCompensationFactor = 1.05f;
    public float maxFallSpeed = 20f;
    public float jumpApexThreshold = 5f;
    public float jumpHangTimeGravityFactor = 2f;
    // no jump cut because of kinect controls
    public float jumpFallTimeGravityFactor = 2f;
    public float doubleJumpCooldown = 1f;
    public float maxDoubleJumpForceReduction = 1.3f;
    public float disableDoubleJumpThreshold = 15f;
    //public float disableDashAfterJumpThreshold = 15f;
    public bool IsJumping { get; private set; }
    private bool _isJumpFalling;
    //private bool canWallJump = true;
    private bool canDoubleJump = true;

    [Range(0f, 1f)] public float jumpBufferTime = 0.1f;
    [Range(0f, 1f)] public float jumpCoyoteTime = 0.1f;
    private float jumpBufferTimer = 10f;
    private float jumpCoyoteTimer = 0f;
    private float timeSinceLastJump = 0f;
    private int jumpCounter = 0;
    private float timeSinceLastJumpCompleted = 0f;

    // Fall damage
    public float fallDamageThreshold = 15f; // falling speed threshhold; not distance
    public float fallDamageAfterJumpingThreshold = 20f;
    public float fallingGravityFactor = 1.2f;
    private float timeSinceLastFallCompleted = 0f;
    private bool playerDiesFromFallDamage = false;
    private bool landedOnNoFallDamageObject = false;
    private bool canTakeFallDamage = true;
    private string noFallDamageObjectTag = "NoFallDamageObject";

    [Header("Dash")]
    public float dashingImpulse = 24f;
    public float dashingDistance = 12f;
    public float dashingCooldown = 1f;
    private float dashingTimer = 0f;
    private bool canDash = true;
    private bool disabledDoubleJump = false;

    [Header("Attack")]
    public float attackRange = 2; // 0.6f;
    public int attackDamage = 1;
    public float attackCooldown = 0.6f;
    private bool canAttack = true;
    private bool attackDisabled = true;
    private string destroyableObjectTag = "DestroyableObject";
    private string hostileObjectTag = "Hostile";


    TouchingDirections touchingDirections;

    private PuzzlePlatform[] puzzlePlatforms;

    private float puzzle_airWalkSpeed;
    private float puzzle_airJumpSpeed;
    private float puzzle_jumpImpulse;
    private float original_airWalkSpeed;
    private float original_airJumpSpeed;
    private float original_jumpImpulse;

    private float original_gravityScale;

    // Kinect variables
    //public BodySourceManager bodySourceManager;
    public GameObject bodySourceManager = null;
    private Body[] bodies;


    //private GameObject currentOneWayPlatform;
    //private float secondsToFallThroughPlatform = 0.25f;



    public float CurrentMoveSpeed { get
        {
            float lerpAmount = 1f;
            float accelRate = 1f;

            if (CanMove) // can restrict movement here with: CanMove
            {
                float targetSpeed = moveInput.x;

                if (IsMoving) // !touchingDirections.IsOnWall && !touchingDirections.IsOnCeiling // IsMoving is moveInput != Vector2.zero
                {
                    // SetFacingDirection(moveInput); // could call this here
                    Vector2 targetVelocity = Vector2.zero;

                    if (touchingDirections.IsGrounded)
                    {
                        // Ground move
                        accelRate = groundAcceleration;

                        if (IsRunning)
                        {
                            targetVelocity = new Vector2(moveInput.x, 0f) * maxRunSpeed;

                            targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed * maxRunSpeed, lerpAmount);
                        }
                        else
                        {
                            targetVelocity = new Vector2(moveInput.x, 0f) * maxWalkSpeed;

                            targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed * maxWalkSpeed, lerpAmount);
                        }
                    } 
                    else
                    {
                        // Air move
                        accelRate = airAcceleration;
                        targetVelocity = new Vector2(moveInput.x, 0f) * maxAirWalkSpeed;

                        if (IsJumping || _isJumpFalling)
                        {
                            targetVelocity = new Vector2(moveInput.x, 0f) * maxAirJumpSpeed;
                            targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed * maxAirJumpSpeed, lerpAmount);

                            // boost acc when reaching peak of jump
                            if (Mathf.Abs(rb.velocity.y) < jumpApexThreshold)
                            {
                                accelRate *= 1.5f;
                                targetSpeed *= 1.1f;
                            }
                        } 
                        else
                        {
                            targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed * maxAirWalkSpeed, lerpAmount);
                        }
                    }

                    _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, accelRate * Time.fixedDeltaTime);
                }
                else
                {
                    accelRate = groundDeceleration;
                    if (!touchingDirections.IsGrounded)
                    {
                        // Air move
                        accelRate = airDeceleration;
                    } 

                    _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, accelRate * Time.fixedDeltaTime);

                    // apply counter force when decelerating
                    float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
                    amount *= Mathf.Sign(rb.velocity.x);

                    rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
                }


                float speedDif = targetSpeed - rb.velocity.x;
                float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, 0.9f) * Math.Sign(speedDif);
                return movement;
            }
            else
            {
                return 0f; // movement locked (when attacking)
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
        //rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // newly added because of the dash, was discrete before
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();

        puzzlePlatforms = FindObjectsOfType<PuzzlePlatform>();

        puzzle_airWalkSpeed = maxAirWalkSpeed / 2;
        puzzle_airJumpSpeed = maxAirJumpSpeed / 1.1f;
        puzzle_jumpImpulse = jumpImpulse / 1.1f;
        original_airWalkSpeed = maxAirWalkSpeed;
        original_airJumpSpeed = maxAirJumpSpeed;
        original_jumpImpulse = jumpImpulse;

        original_gravityScale = rb.gravityScale;
    }


    private void OnValidate()
    {
    }

    private void OnEnable()
    {
    }

    // Update is called once per frame
    void Update()
    {
        stopMovementToleranceTimer += Time.deltaTime;

        if (!touchingDirections.IsGrounded)
        {
            jumpCoyoteTimer += Time.deltaTime;
        }
        jumpBufferTimer += Time.deltaTime;
        timeSinceLastJump += Time.deltaTime;

        timeSinceLastJumpCompleted += Time.deltaTime;
        timeSinceLastFallCompleted += Time.deltaTime;

        dashingTimer += Time.deltaTime;


        // Disable/Enable Attack
        if (!ProgressController.instance.HasPickedUpAttack())
        {
            attackDisabled = true;
        } else
        {
            attackDisabled = false;
        }


        // Set movement boolean with tolerance (to stop quick switch to idle mode when changing direction)
        if (moveInput != Vector2.zero)
        {
            IsMoving = true;
            stopMovementToleranceTimer = 0f;
        }
        else
        {
            if (stopMovementToleranceTimer > stopMovementTolerance)
            {
                IsMoving = false;
            }
        }

        // Kinect input handling
        if (bodySourceManager != null)
        {
            //bodies = bodySourceManager.GetData(); // TODO UNCOMMENT
            if (bodies != null)
            {
                foreach (var body in bodies)
                {
                    if (body != null && body.IsTracked)
                    {
                        HandleKinectGestures(body);
                    }
                }
            }
        }

    }


    private void FixedUpdate()
    {
        // Movement on x
        //float movement = CurrentMoveSpeed;
        //rb.velocity = new Vector2(_moveVelocity.x, rb.velocity.y);
        //rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y); // * Time.fixedDeltaTime already handled by RigitBody
        rb.AddForce(CurrentMoveSpeed * Vector2.right, ForceMode2D.Force);

        if (touchingDirections.IsGrounded || touchingDirections.IsApproachingGrounded)
        {
            float currentXSpeed = Mathf.Abs(rb.velocity.x);
            float downForceMagnitude = baseDownForceMagnitude + (currentXSpeed / maxRunSpeed) * (maxDownForceMagnitude - baseDownForceMagnitude);
            //print(downForceMagnitude);

            if (!IsJumping && !_isJumpFalling)
            {
                rb.AddForce(Vector2.down * downForceMagnitude, ForceMode2D.Force);
            }
        }


        // disable double jump
        if (IsFalling && Mathf.Abs(rb.velocity.y) > disableDoubleJumpThreshold)
        {
            canDoubleJump = false; // disable double jump
        }

        // Jumping
        if (jumpBufferTimer < jumpBufferTime && (!IsJumping && !_isJumpFalling) && (touchingDirections.IsGrounded || jumpCoyoteTimer < jumpCoyoteTime))
        {
            InitializeJump(false);
        }
        // Double Jump
        else if (jumpBufferTimer < jumpBufferTime && (IsJumping || _isJumpFalling) && (jumpCounter < 2 && timeSinceLastJump > doubleJumpCooldown && canDoubleJump))
        {
            InitializeJump(true);
        }

        HandleJumpState();


        // Dashing
        dashingTimer += Time.deltaTime;
        if (IsDashing)
        {
            // Prevent movement while dashing
            return;
        }


        // track falling state
        //animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
        if (!touchingDirections.IsApproachingGrounded && !touchingDirections.IsApproachingGrounded)
        {
            animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
        } 
        else
        {
            if (IsJumping || _isJumpFalling)
            {
                animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
            } 
            else
            {
                animator.SetFloat(AnimationStrings.yVelocity, 0);
            }
        }
        

        // apply fall damage
        HandleFall();

        //FallThroughPlatform();
    }


    /*
    private void Slide()
    {
        //We remove the remaining upwards Impulse to prevent upwards sliding
        if (rb.velocity.y > 0)
        {
            rb.AddForce(-rb.velocity.y * Vector2.up, ForceMode2D.Impulse);
        }

        //Works the same as the Run but only in the y-axis
        //THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
        float speedDif = Data.slideSpeed - RB.velocity.y;
        float movement = speedDif * Data.slideAccel;
        //So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
        //The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        rb.AddForce(movement * Vector2.up);
    }
    */

    private void HandleFall()
    {
        int layerMask = LayerMask.GetMask("Default", "Ground");

        bool lastIsFalling = IsFalling;
        IsFalling = rb.velocity.y < 0 && !touchingDirections.IsGrounded;

        /*
        if (lastIsFalling && !(IsFalling))
        {
            if (touchingDirections.IsGrounded)
            {
                animator.SetTrigger(AnimationStrings.impactAfterFalling);
            }
        }
        */

        if (IsFalling && (!IsJumping && !_isJumpFalling))
        {
            rb.gravityScale = original_gravityScale * fallingGravityFactor;
            animator.ResetTrigger(AnimationStrings.impactAfterFalling);
        }
        //else if (lastIsFalling && !IsFalling && timeSinceLastJumpCompleted > 0.05f && timeSinceLastFallCompleted > 0.05f)
        else if (lastIsFalling && touchingDirections.IsGrounded && timeSinceLastJumpCompleted > 0.05f && timeSinceLastFallCompleted > 0.05f)
        {
            //if (!touchingDirections.IsOnWall && !touchingDirections.IsOnCeiling)
            //print("hit the ground after falling");
            timeSinceLastFallCompleted = 0f;
            animator.SetTrigger(AnimationStrings.impactAfterFalling);

            rb.gravityScale = original_gravityScale;
        }

        if (IsFalling && canTakeFallDamage)
        {
            // (almost) seal fate of death when reaching a certain y velocity
            if (_isJumpFalling)
            {
                if (Mathf.Abs(rb.velocity.y) > fallDamageAfterJumpingThreshold && !playerDiesFromFallDamage)
                {
                    canDash = false; // disable dash
                    canDoubleJump = false; // disable double jump
                    playerDiesFromFallDamage = true;

                }
            } 
            else
            {
                if (Mathf.Abs(rb.velocity.y) > fallDamageThreshold && !playerDiesFromFallDamage)
                {
                    canDash = false; // disable dash
                    canDoubleJump = false; // disable double jump
                    playerDiesFromFallDamage = true;

                }
            }
        }

        if (touchingDirections.IsGrounded && playerDiesFromFallDamage)
        {
            // check for collisions below the player
            float distanceToGround = (playerCollider.size.y) + 1f;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distanceToGround, touchingDirections.groundLayerMask);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag(noFallDamageObjectTag))
                {
                    landedOnNoFallDamageObject = true;
                }
            }

            //print("PLAYER WILL DIE!");
            if (!landedOnNoFallDamageObject)
            {
                RespawnController.instance.AnnouncePlayerDeath();
            }

            landedOnNoFallDamageObject = false;
            playerDiesFromFallDamage = false;
            canDash = true;
            canDoubleJump = true;
            disabledDoubleJump = false;
        }
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        //IsMoving = moveInput != Vector2.zero;
        SetFacingDirection(moveInput);
    }
    
    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !IsFacingRight)
        {
            // Face the right
            IsFacingRight = true;
            //transform.Rotate(0f, 100, 0f);
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            // Face the left
            IsFacingRight = false;
            //transform.Rotate(0f, -100, 0f);
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
            //IsJumping = true;

            // Count jumps
            if (context.started)
            {
                jumpBufferTimer = 0f;
                //jumpCounter += 1;

                // switch "puzzle" platforms on jump for all found PuzzlePlatform instances
                foreach (PuzzlePlatform puzzlePlatform in puzzlePlatforms)
                {
                    if (touchingDirections.IsGrounded)
                    {
                        puzzlePlatform.SwitchPlatforms();
                    }
                }

            }

            if (context.performed)
            {
                // Update the jump buffer timer if the key is still held down
                jumpBufferTimer = 0f;
            }

            // Normal jump
            /*
            if (context.started && touchingDirections.IsGrounded) // without coyote time: context.started && touchingDirections.IsGrounded
            {
                animator.SetTrigger(AnimationStrings.jump);

                LastPressedJumpTime = 0;
                LastOnGroundTime = 0;

                float force = jumpImpulse;
                if (rb.velocity.y < 0)
                    force -= rb.velocity.y;
                //rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + ((force + (0.5f * Time.fixedDeltaTime * -jumpGravityStrengthFactor)) / rb.mass));
                //rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
            }
            else if (context.started && touchingDirections.IsOnWall)
            {
                if (canWallJump)
                {
                    //PerformWallJump();
                }
            }
            */

            // Tapping for little jumps
            /*
            if (context.canceled && rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
            */
        }
    }

    private void InitializeJump(bool doubleJump)
    {
        IsJumping = true;
        animator.SetTrigger(AnimationStrings.jump);
        animator.ResetTrigger(AnimationStrings.impactAfterFalling);
        animator.ResetTrigger(AnimationStrings.impactAfterJumpFalling);

        timeSinceLastJump = 0f;
        jumpCounter += 1;


        float currentVelocity = rb.velocity.magnitude;
        float velocityForMaxScaling = maxAirJumpSpeed;
        currentVelocity = Mathf.Clamp(currentVelocity, 0f, velocityForMaxScaling);

        float velocityFactor = currentVelocity / velocityForMaxScaling;
        float forceIncreaseFactor = Mathf.Lerp(maxIdleForceIncrease, 1f, velocityFactor);
        float force = jumpImpulse * forceIncreaseFactor;

        if (doubleJump)
        {
            _isJumpFalling = false;

            currentVelocity = Mathf.Abs(rb.velocity.y);
            velocityForMaxScaling = maxAirJumpSpeed;
            currentVelocity = Mathf.Clamp(currentVelocity, 0f, velocityForMaxScaling);
            float forceReductionFactor = Mathf.Lerp(1f, maxDoubleJumpForceReduction, currentVelocity / velocityForMaxScaling);
            print(forceReductionFactor);
            force = jumpImpulse / forceReductionFactor;
        }
        if (rb.velocity.y < 0)
            force -= rb.velocity.y;

        // do the actual jump
        rb.gravityScale = original_gravityScale * jumpGravityScaleFactor;

        //rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + ((force + (0.5f * Time.fixedDeltaTime * -jumpGravityCompensationFactor)) / rb.mass));
        SoundManager.instance.PlaySound3D("Jump", transform.position);
        //rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
    }

    private void HandleJumpState()
    {
        // set jumping states
        if (IsJumping && rb.velocity.y < 0)
        {
            IsJumping = false;
            _isJumpFalling = true;
        }

        if (_isJumpFalling && touchingDirections.IsGrounded) // after jump like: !IsJumping && !_isJumpFalling && touchingDirections.IsGrounded
        {
            IsJumping = false;
            _isJumpFalling = false;
            timeSinceLastJumpCompleted = 0f;

            //if (!touchingDirections.IsOnWall && !touchingDirections.IsOnCeiling)
            //print("hit the ground after JUMP falling");
            animator.SetTrigger(AnimationStrings.impactAfterJumpFalling);

            if (!disabledDoubleJump)
            {
                canDoubleJump = true;

            }

            jumpCounter = 0;
            jumpCoyoteTimer = 0f;

            rb.gravityScale = original_gravityScale;
        }


        // apex of jump
        if ((IsJumping || _isJumpFalling) && Mathf.Abs(rb.velocity.y) < jumpApexThreshold)
        {
            // change gravity on apex
            rb.gravityScale = original_gravityScale * jumpHangTimeGravityFactor;
        }
        // rest of the fall
        else if (_isJumpFalling && rb.velocity.y < 0)
        {
            // gigher gravity if falling
            rb.gravityScale = original_gravityScale * jumpFallTimeGravityFactor;
            // caps maximum fall speed after jumping
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));

            // disable double jump when too close to ground
            if (touchingDirections.IsApproachingGrounded && !touchingDirections.IsGrounded) // fixed double jump disabled right before hitting ground
            {
                canDoubleJump = false;
            }
        }
    }

    // Wall jump
    private void PerformWallJump()
    {
        float originalGravity = rb.gravityScale;

        //if (jumpCounter <= 1)
        {
            //animator.SetTrigger(AnimationStrings.jump);
            rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);

            // increase gravity at peak of jump curve
            if (rb.velocity.y < 0)
            {
                rb.gravityScale = originalGravity * 1.5f;
            }
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash)
        {
            StartCoroutine(Dash());
        }
    }


    // Implement a dash
    private IEnumerator Dash()
    {
        if (dashingTimer > dashingCooldown)
        {
            bool wasInterrupted = false; // interrupted by an external influence

            canDash = false;
            IsDashing = true;

            animator.ResetTrigger(AnimationStrings.impactAfterFalling);
            animator.ResetTrigger(AnimationStrings.impactAfterJumpFalling);

            float originalGravity = rb.gravityScale;
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            //rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // do not allow dashing through any obstacles
            int layerMask = LayerMask.GetMask("Default", "Ground"); // "Default", could be added to detect f.E. triggers

            //Vector2 dashDirection = new Vector2(transform.localScale.x * dashingImpulse, 0);
            Vector2 dashDirection = new Vector2(transform.localScale.x, 0).normalized; // more constant version
            Vector2 targetPosition = rb.position + dashDirection * dashingDistance;

            RaycastHit2D[] hits = new RaycastHit2D[10];
            trailRenderer.emitting = true;

            float burstImpulse = dashingImpulse * 1.5f;
            float currentImpulse = burstImpulse;
            bool burstActive = true;

            SoundManager.instance.PlaySound3D("Dash", transform.position);

            Vector2 previousPosition = rb.position;
            while (Vector2.Distance(rb.position, targetPosition) > 0.1f)
            {
                // raycast to detect any colliders in the path of the dash
                Vector2 boxSize = new Vector2(dashingDistance / (dashingDistance / 1.5f), playerCollider.bounds.size.y);

                int numHits = Physics2D.BoxCastNonAlloc(rb.position, boxSize, 0f, dashDirection, hits, dashingDistance / (dashingDistance / 1.5f), layerMask);

                //int numHits = Physics2D.RaycastNonAlloc(rb.position, dashDirection.normalized, hits, raycastDistance, layerMask);
                bool stopDash = false; // stop when hitting ground layer

                /* show box
                Vector2 boxOrigin = rb.position + dashDirection.normalized * boxSize.x / 2;
                Debug.DrawLine(boxOrigin + new Vector2(-boxSize.x / 2, -boxSize.y / 2), boxOrigin + new Vector2(boxSize.x / 2, -boxSize.y / 2), Color.red);
                Debug.DrawLine(boxOrigin + new Vector2(boxSize.x / 2, -boxSize.y / 2), boxOrigin + new Vector2(boxSize.x / 2, boxSize.y / 2), Color.red);
                Debug.DrawLine(boxOrigin + new Vector2(boxSize.x / 2, boxSize.y / 2), boxOrigin + new Vector2(-boxSize.x / 2, boxSize.y / 2), Color.red);
                Debug.DrawLine(boxOrigin + new Vector2(-boxSize.x / 2, boxSize.y / 2), boxOrigin + new Vector2(-boxSize.x / 2, -boxSize.y / 2), Color.red);
                */

                if (numHits > 0)
                {
                    for (int i = 0; i < numHits; i++)
                    {
                        Collider2D collider = hits[i].collider;
                        if (collider == null)
                        {
                            continue;
                        }

                        if (collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                        {
                            stopDash = true;
                            break;
                        }

                        // handle other collisions with f.E. triggers
                        //if (collider.isTrigger)
                    }
                }

                // hit a ground layer object
                if (stopDash)
                {
                    rb.velocity = Vector2.zero;
                    //Vector2 stoppingForce = -rb.velocity * 10f;
                    //rb.AddForce(stoppingForce, ForceMode2D.Impulse);
                    break;
                }


                float stepDistance = currentImpulse * Time.fixedDeltaTime;
                rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, stepDistance));

                if (burstActive)
                {
                    currentImpulse = Mathf.MoveTowards(currentImpulse, dashingImpulse, (burstImpulse - dashingImpulse) * Time.fixedDeltaTime * 0.1f);
                    if (Mathf.Approximately(currentImpulse, dashingImpulse))
                    {
                        burstActive = false;
                    }
                }

                yield return new WaitForFixedUpdate();

                // free player when stuck
                if (Vector2.Distance(rb.position, previousPosition) < 0.01f)
                {
                    break;
                }
                previousPosition = rb.position;

                // check if dash was interrupted
                if (!IsDashing)
                {
                    wasInterrupted = true;
                    break;
                }
            }


            trailRenderer.emitting = false;
            rb.gravityScale = originalGravity;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete; // set back

            IsDashing = false;
            dashingTimer = 0f;
            //yield return new WaitForSeconds(dashingCooldown);

            if (wasInterrupted)
            {
                canDash = false;
            }
            else
            {
                canDash = true;
            }
        }
    }



    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && canAttack && !attackDisabled)
        {
            animator.SetTrigger(AnimationStrings.attack);
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        canAttack = false;

        SoundManager.instance.PlaySound3D("Attack", transform.position);

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
            if (hit.CompareTag(destroyableObjectTag))
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
        Collider2D platformCollider = currentOneWayPlatform.GetComponent<Collider2D>();

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
        if (collision.gameObject.CompareTag(hostileObjectTag) || collision.gameObject.layer == LayerMask.NameToLayer(hostileObjectTag))
        {
            gameObject.SetActive(false);
            RespawnController.instance.AnnouncePlayerDeath();
        }

        // handle being crushed between 2 objects
        bool collidedWithCrushObstacle1 = false;
        bool collidedWithCrushObstacle2 = true;
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

        //canWallJump = false;
        canDoubleJump = false;
        disabledDoubleJump = true;
        canDash = false;
        canTakeFallDamage = false;

        maxAirWalkSpeed = puzzle_airWalkSpeed;
        maxAirJumpSpeed = puzzle_airJumpSpeed;
        jumpImpulse = puzzle_jumpImpulse;
    }

    public void ResetAfterPuzzleSection()
    {
        //canWallJump = true;
        canDoubleJump = true;
        disabledDoubleJump = false;
        canDash = true;
        canTakeFallDamage = true;

        maxAirWalkSpeed = original_airWalkSpeed;
        maxAirJumpSpeed = original_airJumpSpeed;
        jumpImpulse = original_jumpImpulse;
    }


    public void PrepareForCaveSection()
    {
        // interrupt dash
        IsDashing = false;

        //canWallJump = false;
        canDash = true;
        canTakeFallDamage = false;
    }

    public void ResetAfterCaveSection()
    {
        //canWallJump = true;
        canDash = true;
        canTakeFallDamage = true;
    }












// Handle attack directly
    public void PerformAttack() 
{
    if (canAttack && !attackDisabled)
    {
        animator.SetTrigger(AnimationStrings.attack);
        StartCoroutine(Attack());
    }
}



// Handle jump directly
public void PerformJump()
{
    jumpBufferTimer = 0f; // Simulate jump input delay

    // Double Jump logic
    if (jumpBufferTimer < jumpBufferTime && (IsJumping || _isJumpFalling) && (jumpCounter < 2 && timeSinceLastJump > doubleJumpCooldown && canDoubleJump))
    {
        InitializeJump(true);
    }

    // Normal Jump logic
    if (jumpBufferTimer < jumpBufferTime && (!IsJumping && !_isJumpFalling) && (touchingDirections.IsGrounded || jumpCoyoteTimer < jumpCoyoteTime))
    {
        InitializeJump(false);
    }
}

// Handle dash directly
public void PerformDash()
{
    if (canDash)
    {
        StartCoroutine(Dash());
    }
}

// Kinect gesture handling
private void HandleKinectGestures(Body body)
{
    if (IsFistGesture(body, JointType.HandLeft))
    {
        // Move left
        moveInput = Vector2.left;
        SetFacingDirection(moveInput);
    }
    else if (IsFistGesture(body, JointType.HandRight))
    {
        // Move right
        moveInput = Vector2.right;
        SetFacingDirection(moveInput);
    }
    else
    {
        // Stop movement
        moveInput = Vector2.zero;
    }

    // Jump gesture (head moving up significantly)
    if (IsJumpGesture(body))
    {
        PerformJump(); // Trigger jump function
    }

    // Dash gesture (quick hand movement to other shoulder)
    if (IsDashGesture(body))
    {
        PerformDash();
    }

    // Attack gesture (punch)
    if (isAttackGesture(body))
    {
        PerformAttack();
    }
}

private bool IsFistGesture(Body body, JointType hand)
{
    var handState = hand == JointType.HandLeft ? body.HandLeftState : body.HandRightState;
    return handState == HandState.Closed;
}

private float initialHeadY;
private float initialHeadTime;
private float jumpDetectionThreshold = 0.1f;
private float jumpDetectionTimeWindow = 0.2f;

private void ResetJumpDetection()
{
    initialHeadY = 0f;
    initialHeadTime = 0f;
}

private bool IsJumpGesture(Body body)
{
    var head = body.Joints[JointType.Head];

    if (head.TrackingState != TrackingState.Tracked) {
        ResetJumpDetection();
        return false;
    }

    // Capture initial head position and time
    if (initialHeadTime == 0f)
    {
        initialHeadY = head.Position.Y;
        initialHeadTime = Time.time;
        return false;
    }

    // Calculate the difference in head height
    float verticalMovement = head.Position.Y - initialHeadY;

    // Reset detection if the time window is exceeded
    if (Time.time - initialHeadTime > jumpDetectionTimeWindow)
    {
        initialHeadY = head.Position.Y;
        initialHeadTime = Time.time;
        return false;
    }

    // Threshold adjusted to detect significant upward movement
    if (verticalMovement > jumpDetectionThreshold)
    {
        ResetJumpDetection(); // Reset for next detection
        return true;
    }

    return false;
}
private bool IsDashGesture(Body body)
{
     if (IsLeftArmTappingRightShoulder(body))
    {
        SetFacingDirection(Vector2.right);
        return true;
    }
    else if (IsRightArmTappingLeftShoulder(body))
    {
        SetFacingDirection(Vector2.left);
        return true;
    }
    return false;
}


    private float punchSpeedThreshold = 0.3f; 
    private float punchDistanceThreshold = 0.08f; // Minimum distance the hand must travel towards the camera

    private Vector3 previousRightHandPosition;
    private Vector3 previousLeftHandPosition;

private bool isAttackGesture(Body body)
    {
        var rightHand = body.Joints[JointType.HandRight];
        var leftHand = body.Joints[JointType.HandLeft];

        // Check if hands are tracked
        if (rightHand.TrackingState == TrackingState.Tracked && leftHand.TrackingState == TrackingState.Tracked)
        {
            Vector3 rightHandPosition = GetVector3FromJoint(rightHand);
            Vector3 leftHandPosition = GetVector3FromJoint(leftHand);

            // Calculate hand speeds towards the camera (positive z-axis direction)
            float rightHandSpeed = CalculateHandSpeed(rightHandPosition, previousRightHandPosition);
            float leftHandSpeed = CalculateHandSpeed(leftHandPosition, previousLeftHandPosition);

            // Calculate distance moved towards the camera
            float rightHandDistance = rightHandPosition.z - previousRightHandPosition.z;
            float leftHandDistance = leftHandPosition.z - previousLeftHandPosition.z;

            // Update previous hand positions for next frame
            previousRightHandPosition = rightHandPosition;
            previousLeftHandPosition = leftHandPosition;

            // Check if either hand is moving fast towards the camera and meets distance threshold
            if ((rightHandSpeed > punchSpeedThreshold && rightHandDistance > punchDistanceThreshold) ||
                (leftHandSpeed > punchSpeedThreshold && leftHandDistance > punchDistanceThreshold))
            {
                return true;
            }
        }

        return false;
    }

    float CalculateHandSpeed(Vector3 currentPos, Vector3 previousPos)
    {
        return Mathf.Abs((currentPos - previousPos).z) / Time.deltaTime;
    }

    Vector3 GetVector3FromJoint(Windows.Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
    }

private bool IsLeftArmTappingRightShoulder(Body body)
{
    var leftHand = body.Joints[JointType.HandLeft];
    var rightShoulder = body.Joints[JointType.ShoulderRight];

    return leftHand.TrackingState == TrackingState.Tracked &&
           rightShoulder.TrackingState == TrackingState.Tracked &&
           Vector3.Distance(new Vector3(leftHand.Position.X, leftHand.Position.Y, leftHand.Position.Z),
                            new Vector3(rightShoulder.Position.X, rightShoulder.Position.Y, rightShoulder.Position.Z)) < 0.15f;
}

private bool IsRightArmTappingLeftShoulder(Body body)
{
    var rightHand = body.Joints[JointType.HandRight];
    var leftShoulder = body.Joints[JointType.ShoulderLeft];

    return rightHand.TrackingState == TrackingState.Tracked &&
           leftShoulder.TrackingState == TrackingState.Tracked &&
           Vector3.Distance(new Vector3(rightHand.Position.X, rightHand.Position.Y, rightHand.Position.Z),
                            new Vector3(leftShoulder.Position.X, leftShoulder.Position.Y, leftShoulder.Position.Z)) < 0.15f;
}

}
