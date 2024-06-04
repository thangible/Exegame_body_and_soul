using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


// Based on https://www.youtube.com/watch?v=_vh9mCskp_o
public class FlyingBoss : MonoBehaviour
{
    [Header("Sleeping")]
    [SerializeField] float playerProximityToAwaken = 10f; // on x axis
    //[SerializeField] float awakenKnockbackForce = 1f;
    private bool sleepingStateStarted = false;
    private bool sleepingStateFinished = false; // track sleeping state followed by awakeState

    // Awaken State
    private bool awakenStateStarted = false;
    private bool awakenStateFinished = false; // isInIdlePosition

    [Header("Idle")] // never "finished"
    [SerializeField] float idleMovementSpeed = 0.4f;
    [SerializeField] Vector2 idleMovementDirection = new Vector2(-1, 1);
    private bool hasReachedFirstIdle = false;

    [Header("QuickRangedAttack")] // Consists of the movement phase and the variation phase
    [SerializeField] GameObject quickProjectile;
    [SerializeField] GameObject quickProjectileCollisionCourse;
    [SerializeField] Transform quickProjectilePos;
    [SerializeField] Transform whileMovingProjectilePos;
    [SerializeField] float quickAttackVariationPhaseDuration = 10f;
    [SerializeField] float quickAttackProjectilesFiredRoutine1 = 24f;
    [SerializeField] float quickAttackProjectilesFiredRoutine2 = 4f;
    [SerializeField] float quickAttackProjectilesFiredRoutine3 = 44f;
    [SerializeField] float quickAttackProjectilesFiredRoutine4 = 12f;
    [SerializeField] float quickAttackMovementPhaseSpeed = 10f; // speed of side change --> basically same logic as phase 2 duration

    private bool routine1Active = false;
    private bool routine2Active = false;
    private bool routine3Active = false;
    private int[] routine3Pattern = new int[25];
    private bool routine4Active = false;
    private int[] routine4Pattern = new int[10];

    private int lastRoutine = 0;
    private bool isQuickAttackMovementPhaseActive = true;
    private float quickAttackElapsedTime = 0f;
    private bool quickAttackStateStarted = false;
    private bool quickAttackStateFinished = false;

    [Header("SlowRangedAttack")]
    [SerializeField] GameObject slowProjectile;
    [SerializeField] Transform slowProjectilePos;
    [SerializeField] float slowAttackPhaseDuration = 10f;
    private List<GameObject> slowProjectiles = new List<GameObject>();
    private float slowAttackElapsedTime = 0f;
    private bool slowAttackStateStarted = false;
    private bool slowAttackStateFinished = false;

    [Header("AttackPlayer")]
    [SerializeField] float attackPlayerSpeed = 50f;
    [SerializeField] float minAttackRange = 8;
    [SerializeField] float maxAttackRange = 12;
    [SerializeField] float attackFollowTime = 0.33f;
    private float attackRange = 0f;
    private Vector3 lastPosition;
    private bool isPlayerInRange = false;
    private bool isLeapingAtPlayer = false;
    private bool attackCompleted = false;
    private bool attackPlayerStateStarted = false;
    private bool attackPlayerStateFinished = false;

    [Header("HitPlayerInProximity")] // Hit player in proximity on x axis (when idle/ in default position)
    [SerializeField] float playerProximityToGetHit = 1f; // on x axis (choose a small value)
    [SerializeField] float proximityHitFollowTime = 0.1f;
    private bool hitPlayerInProximityStateStarted = false;
    private bool hitPlayerInProximityStateFinished = false;


    [Header("Other")]
    [SerializeField] GameObject player;
    [SerializeField] int bossHitpoints = 1;
    private int hits = 0;
    private float shootDownwardsAngle = 74f;

    [SerializeField] Transform bossArea;
    [SerializeField] float boundsDistanceHorizontal = 2f; // 1.5f
    [SerializeField] float boundsDistanceVertical = 0f; // 1.5f
    [SerializeField] float sleepingMovementUpperThresholdDistance = 3f; // a distance from lower bounds
    [SerializeField] float sleepingMovementLowerThresholdDistance = 2.5f; // a distance from lower bounds
    [SerializeField] float idleMovementUpperThresholdDistance = 3f; // a distance from upper bounds
    [SerializeField] float idleMovementLowerThresholdDistance = 4f; // a distance from upper bounds
    [SerializeField] float movementWallThresholdDistance = 2.5f;
    [SerializeField] List<GameObject> objectsToResetAfterDefeat;
    [SerializeField] List<GameObject> objectsToDisableAfterBossFight; // f.E. box colliders that confine the player inside of the boss arena
    [SerializeField] private CinemachineVirtualCamera vcamCurrent; // get current camera
    [SerializeField] private CinemachineVirtualCamera vcamOnReset; // get current camera

    private bool isTouchingUp;
    private bool isTouchingDown;
    private bool isTouchingWall;

    private Vector2 playerPosition;
    private Vector3 initialPosition;

    private bool isFacingLeft = true;
    private bool isGoingUp = true;
    private bool isMovingOnX = false;
    private bool hasMovedSides = false;

    private float timer = 0f;
    private float projectileTimer = 0f;
    private int projectilesFired = 0;
    private float distanceToPlayer;

    private bool pickedPhaseOne = false;
    private bool pickedPhaseTwo = false;

    private Rigidbody2D enemyRB;
    private Collider2D enemyCollider;
    private Animator enemyAnimator;



    void Start()
    {
        if (idleMovementDirection != Vector2.zero)
        {
            idleMovementDirection.Normalize();
        }
        enemyRB = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        enemyAnimator = GetComponent<Animator>();

        initialPosition = transform.position;
        isGoingUp = idleMovementDirection.y > 0;
        isFacingLeft = idleMovementDirection.x < 0;

        for (int i = 0; i < routine3Pattern.Length; i++)
        {
            routine3Pattern[i] = i * 360 / routine3Pattern.Length;
        }

        for (int i = 0; i < routine4Pattern.Length; i++)
        {
            if (i <= 5)
            {
                routine4Pattern[i] = Random.Range(10, 50);
            } else
            {
                routine4Pattern[i] = Random.Range(51, 111);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        timer += Time.deltaTime;
        projectileTimer += Time.deltaTime;
        distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        CheckBoundaries();
    }

    void FixedUpdate()
    {
        // Has reached idle State
        if (sleepingStateFinished && awakenStateFinished)
        {
            // Always play idle movement
            // (not equal to the idle state/animation --> TODO ideally we would resolve this in the animator, to always play the idle even while other animations are active)
            IdleMovement();

            // Phase One
            if ((!quickAttackStateFinished && !attackPlayerStateFinished) && !pickedPhaseOne)
            {
                RandomStatePicker(AnimationStrings.flyingBoss_quickRangedAttack, AnimationStrings.flyingBoss_attackPlayer);
                pickedPhaseOne = true;

                // enable this attack
                hitPlayerInProximityStateFinished = false;
            }

            // Phase Two
            if ((quickAttackStateFinished || attackPlayerStateFinished) && pickedPhaseOne)
            {
                if (!slowAttackStateFinished && !pickedPhaseTwo)
                {
                    enemyAnimator.SetBool(AnimationStrings.flyingBoss_slowRangedAttack, true);
                    pickedPhaseTwo = true;
                }

                if (slowAttackStateFinished && pickedPhaseTwo)
                {
                    // start the cycle again
                    pickedPhaseOne = false;
                    pickedPhaseTwo = false;
                    timer = 0f;
                    projectileTimer = 0f;
                    projectilesFired = 0;

                    attackPlayerStateFinished = false;
                    quickAttackStateFinished = false;
                    slowAttackStateFinished = false;

                    // also disable this attack for now
                    hitPlayerInProximityStateFinished = true;
                }
            }

            // Always check for HitPlayerInProximityState() --> should not stand directly under the enemy
            if (!isMovingOnX && bossArea != null)
            {
                if (!hitPlayerInProximityStateFinished)
                {
                    if (Mathf.Abs(player.transform.position.x - transform.position.x) < playerProximityToGetHit && !isPlayerInRange)
                    {
                        isPlayerInRange = true;
                        enemyAnimator.SetBool(AnimationStrings.flyingBoss_hitPlayerInProximity, true);
                    }
                }
            }
        }
        //SleepingState();
        //AwakenState();
        //IdleState();

        //SlowRangedAttackState();
        //QuickRangedAttackState();
        //AttackPlayerState();

        //HitPlayerInProximityState();
    }

    void RandomStatePicker(string triggerStr1, string triggerStr2)
    {
        int randomState = Random.Range(0, 2);
        if (randomState == 0)
        {
            enemyAnimator.SetBool(triggerStr1, true);
        }
        else if (randomState == 1)
        {
            enemyAnimator.SetBool(triggerStr2, true);
        }
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // TODO remove any effects on the enemy instance (f.E. rotation after hitting the player)
            //ResetEnemy();

            collision.gameObject.SetActive(false);
            PlayerDefeated();
        }
    }


    void CheckBoundaries()
    {
        if (bossArea != null)
        {
            Bounds bounds = new Bounds(bossArea.position, bossArea.localScale);

            isTouchingUp = transform.position.y >= bounds.max.y - boundsDistanceHorizontal;
            isTouchingDown = transform.position.y <= bounds.min.y + boundsDistanceHorizontal;
            isTouchingWall = transform.position.x <= bounds.min.x + boundsDistanceVertical || transform.position.x >= bounds.max.x - boundsDistanceVertical;
        }

    }


    public void SleepingState()
    {
        if (bossArea != null && !sleepingStateFinished)
        {
            if (!sleepingStateStarted)
            {
                sleepingStateStarted = true;
            }

            Bounds bounds = new Bounds(bossArea.position, bossArea.localScale);

            float upperLimit = bounds.min.y + sleepingMovementUpperThresholdDistance;
            float lowerLimit = bounds.min.y + sleepingMovementLowerThresholdDistance;
            float distanceToUpperLimit = Mathf.Abs(upperLimit - transform.position.y);
            float distanceToLowerLimit = Mathf.Abs(lowerLimit - transform.position.y);
            float maxDistance = Mathf.Abs(upperLimit - lowerLimit);

            float sleepingMovementSpeed = idleMovementSpeed / 3;
            float minSpeed = sleepingMovementSpeed / 1.7f;
            float transitionSmoothness = maxDistance / 2;
            float adjustedSpeed;

            if (isGoingUp)
            {
                adjustedSpeed = Mathf.Lerp(minSpeed, sleepingMovementSpeed, distanceToUpperLimit / transitionSmoothness);
                if (distanceToLowerLimit < transitionSmoothness)
                {
                    adjustedSpeed = Mathf.Lerp(minSpeed, sleepingMovementSpeed, Mathf.Min(distanceToUpperLimit, distanceToLowerLimit) / transitionSmoothness);
                }

                transform.Translate(Vector3.up * adjustedSpeed * Time.deltaTime);
                if (transform.position.y >= upperLimit)
                {
                    isGoingUp = false;
                }
            }
            else
            {
                adjustedSpeed = Mathf.Lerp(minSpeed, sleepingMovementSpeed, distanceToLowerLimit / transitionSmoothness);
                if (distanceToUpperLimit < transitionSmoothness)
                {
                    adjustedSpeed = Mathf.Lerp(minSpeed, sleepingMovementSpeed, Mathf.Min(distanceToUpperLimit, distanceToLowerLimit) / transitionSmoothness);
                }

                transform.Translate(Vector3.down * adjustedSpeed * Time.deltaTime);
                if (transform.position.y <= lowerLimit)
                {
                    isGoingUp = true;
                }
            }

            // check player proximity to awaken
            if (Mathf.Abs(player.transform.position.x - transform.position.x) < playerProximityToAwaken)
            {
                Rigidbody2D playerRB = player.GetComponent<Rigidbody2D>();
                if (playerRB != null)
                {
                    // TODO apply knockback here & camera shake https://www.youtube.com/watch?v=ZoZQ-cerkbg&ab_channel=Raycastly
                }

                // finish state
                sleepingStateFinished = true;
                sleepingStateStarted = false;

                enemyAnimator.SetTrigger(AnimationStrings.flyingBoss_awaken);
            }


            // Also correct x direction if necessary (more of a design choice in case we do not hit the optimal location when placing this enemy)
            float leftLimit = bounds.min.x + movementWallThresholdDistance;
            float rightLimit = bounds.max.x - movementWallThresholdDistance;

            if (isFacingLeft && transform.position.x >= rightLimit)
            {
                transform.Translate(Vector3.left * idleMovementSpeed * Time.deltaTime);
            }
            else if (!isFacingLeft && transform.position.x <= leftLimit)
            {
                transform.Translate(Vector3.right * idleMovementSpeed * Time.deltaTime);
            }
        }

    }


    public void AwakenState()
    {
        if (bossArea != null && !awakenStateFinished)
        {
            if (!awakenStateStarted)
            {
                awakenStateStarted = true;
            }

            Bounds bounds = new Bounds(bossArea.position, bossArea.localScale);

            float upperLimit = bounds.max.y - idleMovementUpperThresholdDistance;

            float awakenMovementSpeed = idleMovementSpeed * 10;
            transform.Translate(Vector3.up * awakenMovementSpeed * Time.deltaTime);

            if (transform.position.y >= upperLimit)
            {
                // finish state
                awakenStateFinished = true;
                awakenStateStarted = false;

                enemyAnimator.SetTrigger(AnimationStrings.flyingBoss_hasAwakened);
            }
        }
    }


    public void IdleState() // condition to start: awaken done ; condition to stop (temporarily): any movement on the x axis
    {
        if (!hasReachedFirstIdle)
        {
            initialPosition = transform.position;
            hasReachedFirstIdle = true;
        }

        IdleMovement();
    }

    public void IdleMovement()
    {
        if (!isMovingOnX && bossArea != null)
        {
            Bounds bounds = new Bounds(bossArea.position, bossArea.localScale);

            float upperLimit = bounds.max.y - idleMovementUpperThresholdDistance;
            float lowerLimit = bounds.max.y - idleMovementLowerThresholdDistance;
            float distanceToUpperLimit = Mathf.Abs(upperLimit - transform.position.y);
            float distanceToLowerLimit = Mathf.Abs(lowerLimit - transform.position.y);
            float maxDistance = Mathf.Abs(upperLimit - lowerLimit);

            float minSpeed = idleMovementSpeed / 1.7f;
            float transitionSmoothness = maxDistance / 2;
            float adjustedSpeed;

            if (isGoingUp)
            {
                adjustedSpeed = Mathf.Lerp(minSpeed, idleMovementSpeed, distanceToUpperLimit / transitionSmoothness);
                if (distanceToLowerLimit < transitionSmoothness)
                {
                    adjustedSpeed = Mathf.Lerp(minSpeed, idleMovementSpeed, Mathf.Min(distanceToUpperLimit, distanceToLowerLimit) / transitionSmoothness);
                }

                transform.Translate(Vector3.up * adjustedSpeed * Time.deltaTime);
                if (transform.position.y >= upperLimit)
                {
                    isGoingUp = false;
                }
            }
            else
            {
                adjustedSpeed = Mathf.Lerp(minSpeed, idleMovementSpeed, distanceToLowerLimit / transitionSmoothness);
                if (distanceToUpperLimit < transitionSmoothness)
                {
                    adjustedSpeed = Mathf.Lerp(minSpeed, idleMovementSpeed, Mathf.Min(distanceToUpperLimit, distanceToLowerLimit) / transitionSmoothness);
                }

                transform.Translate(Vector3.down * adjustedSpeed * Time.deltaTime);
                if (transform.position.y <= lowerLimit)
                {
                    isGoingUp = true;
                }
            }
        }
    }



    public void SlowRangedAttackState()
    {
        if (player != null && player.activeSelf)
        {
            if (!slowAttackStateFinished)
            {
                if (!slowAttackStateStarted)
                {
                    projectileTimer = 0;
                    slowAttackStateStarted = true;
                }

                slowAttackElapsedTime += Time.deltaTime;

                if (slowAttackElapsedTime < slowAttackPhaseDuration)
                {

                    if (projectileTimer > 1.5)
                    {
                        projectileTimer = 0;

                        FireSlowProjectile(20f);
                    }
                }
                else
                {
                    // finish state
                    slowAttackElapsedTime = 0f;
                    slowAttackStateFinished = true;
                    slowAttackStateStarted = false;

                    // destroy all remaining projectiles
                    foreach (GameObject projectile in slowProjectiles)
                    {
                        if (projectile != null)
                        {
                            SlowProjectileScript projectileScript = projectile.GetComponent<SlowProjectileScript>();
                            if (projectileScript != null)
                            {
                                projectileScript.destroyOnNextBounce = true;
                            }
                        }
                    }
                    slowProjectiles.Clear();

                    enemyAnimator.SetBool(AnimationStrings.flyingBoss_slowRangedAttack, false);
                }
            }
        }
    }

    public void FireSlowProjectile(float force = 20f)
    {
        GameObject projectile;
        projectile = Instantiate(slowProjectile, slowProjectilePos.position, Quaternion.identity);
        slowProjectiles.Add(projectile);

        SlowProjectileScript projectileScript = projectile.GetComponent<SlowProjectileScript>();
        projectileScript.force = force;
    }



    public void QuickRangedAttackState()
    {
        if (player != null && player.activeSelf)
        {
            if (!quickAttackStateFinished)
            {
                if (!quickAttackStateStarted)
                {
                    projectilesFired = 0;
                    timer = 0f;
                    projectileTimer = 0;
                    quickAttackStateStarted = true;
                }

                // First execute the movement phase
                if (isQuickAttackMovementPhaseActive && timer >= 0.5f) // some delay
                {
                    if (!hasMovedSides)
                    {
                        MoveToOtherSide();

                        if (isFacingLeft)
                        {
                            float angle = shootDownwardsAngle;
                            ShootWhileMoving(angle);
                        }
                        else
                        {
                            enemyAnimator.SetTrigger(AnimationStrings.flyingBoss_quickRangedAttackReturnMovement);
                            float angle = -shootDownwardsAngle;
                            ShootWhileMoving(angle);
                        }
                    }
                    else
                    {
                        isQuickAttackMovementPhaseActive = false;
                        isMovingOnX = false;
                    }

                } 
                else if (!isQuickAttackMovementPhaseActive) // Then switch to the variation phase with its routines
                {
                    enemyAnimator.SetTrigger(AnimationStrings.flyingBoss_quickRangedAttackIdleMovement);

                    quickAttackElapsedTime += Time.deltaTime;

                    if (quickAttackElapsedTime < quickAttackVariationPhaseDuration)
                    {
                        if (!routine1Active && !routine2Active && !routine3Active && !routine4Active) // pick a routine/variation
                        {
                            bool routineFound = false;
                            while (!routineFound)
                            {
                                float randomNumber = Random.value;

                                if (randomNumber < 0.25f)
                                {
                                    // routine 1
                                    if (lastRoutine != 1)
                                    {
                                        routineFound = true;
                                        routine1Active = true;
                                    }
                                }
                                else if (randomNumber <= 0.5f)
                                {
                                    // routine 2
                                    if (lastRoutine != 2)
                                    {
                                        routineFound = true;
                                        routine2Active = true;
                                    }
                                }
                                else if (randomNumber <= 0.75f)
                                {
                                    // routine 3
                                    if (lastRoutine != 3)
                                    {
                                        routineFound = true;
                                        routine3Active = true;
                                    }
                                }
                                else
                                {
                                    // routine 4
                                    if (lastRoutine != 4)
                                    {
                                        routineFound = true;
                                        routine4Active = true;
                                    }
                                }
                            }

                            projectilesFired = 0;
                            timer = 0f;
                            projectileTimer = -1.75f; // additional time for a break
                            if (routine4Active)
                            {
                                projectileTimer = -0.75f;
                            }
                        }

                        ExecuteQuickProjectileRoutines(); // execute routine/variation

                    }
                    else if (quickAttackElapsedTime >= quickAttackVariationPhaseDuration && (routine1Active || routine2Active || routine3Active || routine4Active)) // finish ongoing routine
                    {
                        ExecuteQuickProjectileRoutines();
                    }
                    else
                    {
                        // finish state
                        routine1Active = false;
                        routine2Active = false;
                        routine3Active = false;
                        routine4Active = false;
                        lastRoutine = 0;
                        isQuickAttackMovementPhaseActive = true;

                        timer = 0f;
                        projectileTimer = 0f;
                        projectilesFired = 0;

                        hasMovedSides = false;
                        isMovingOnX = false;
                        quickAttackElapsedTime = 0f;
                        quickAttackStateFinished = true;
                        quickAttackStateStarted = false;

                        enemyAnimator.SetBool(AnimationStrings.flyingBoss_quickRangedAttack, false);
                    }
                }

                // Will move on to the movement, therefore reset values
                /*
                if (!isMovingOnX && !hasMovedToOtherSide && quickAttackElapsedTime >= quickAttackPhaseOneDuration && (!routine1Active && !routine2Active && !routine3Active && !routine4Active))
                {
                    projectilesFired = 0;
                    projectileTimer = 1.9f;
                }
                */

            }
        }
    }

    public void ExecuteQuickProjectileRoutines()
    {
        if (routine1Active)
        {
            // routine 1
            if (projectileTimer > 0.1f)
            {
                projectileTimer = 0;

                FireQuickProjectile(QuickProjectileScript.ProjectileType.FocusPlayer, 0f, 22f);
                projectilesFired++;

                if (projectilesFired % 12 == 0)
                {
                    projectileTimer = 0 - 0.3f;
                }
                if (projectilesFired >= quickAttackProjectilesFiredRoutine1)
                {
                    projectilesFired = 0;
                    routine1Active = false;
                    routine2Active = false;
                    routine3Active = false;
                    routine4Active = false;
                    lastRoutine = 1;
                }
            }
        }
        else if (routine2Active)
        {
            // routine 2
            if (projectileTimer > 1f)
            {
                projectileTimer = 0f;

                Instantiate(quickProjectileCollisionCourse, quickProjectilePos.position, Quaternion.identity);
                projectilesFired++;

                if (projectilesFired >= quickAttackProjectilesFiredRoutine2)
                {
                    projectilesFired = 0;
                    routine1Active = false;
                    routine2Active = false;
                    routine3Active = false;
                    routine4Active = false;
                    lastRoutine = 2;
                }
            }
        }
        else if (routine3Active)
        {
            // routine 3
            if (projectileTimer > 0.15f)
            {
                projectileTimer = 0;

                for (int i = 0; i < routine3Pattern.Length; i++)
                {
                    //if (routine3Pattern[i] >= 0 && (routine3Pattern[i] <= 120 || routine3Pattern[i] >= 320)) { // TODO crash?
                    FireQuickProjectile(QuickProjectileScript.ProjectileType.Linear, routine3Pattern[i]);
                }
                projectilesFired++;

                if (projectilesFired % 3 == 0)
                {
                    projectileTimer = 0 - 0.1f;
                } else if (projectilesFired % 8 == 0)
                {
                    projectileTimer = 0 - 0.1f;
                }

                if (projectilesFired % 6 == 0)
                {
                    projectileTimer = 0 - 0.2f;

                    float randomNumber = Random.value;
                    if (randomNumber < 0.50f)
                    {
                        for (int i = 0; i < routine3Pattern.Length; i++)
                        {
                            routine3Pattern[i] += 20;
                        }
                    } else
                    {
                        for (int i = 0; i < routine3Pattern.Length; i++)
                        {
                            routine3Pattern[i] -= 20;
                        }
                    }
                }
                if (projectilesFired >= quickAttackProjectilesFiredRoutine3)
                {
                    projectilesFired = 0;
                    routine1Active = false;
                    routine2Active = false;
                    routine3Active = false;
                    routine4Active = false;
                    lastRoutine = 3;
                }
            }
        }
        else if (routine4Active)
        {
            // routine 4
            if (projectileTimer > 0.1f)
            {
                projectileTimer = 0;

                for (int i = 0; i < routine4Pattern.Length; i++)
                {
                    FireQuickProjectile(QuickProjectileScript.ProjectileType.Linear, routine4Pattern[i], 15f);
                }
                projectilesFired++;

                if (projectilesFired % 2 == 0)
                {
                    projectileTimer = 0 - 1.1f;

                    for (int i = 0; i < routine4Pattern.Length; i++)
                    {
                        if (i <= 5)
                        {
                            routine4Pattern[i] = Random.Range(10, 50);
                        }
                        else
                        {
                            routine4Pattern[i] = Random.Range(51, 111);
                        }
                    }
                }
                if (projectilesFired >= quickAttackProjectilesFiredRoutine4)
                {
                    projectilesFired = 0;
                    routine1Active = false;
                    routine2Active = false;
                    routine3Active = false;
                    routine4Active = false;
                    lastRoutine = 4;
                }
            }

        }
    }



    public void FireQuickProjectile(QuickProjectileScript.ProjectileType type, float angle = 0f, float force = 15f, bool isMoving = false)
    {
        GameObject projectile;
        if (isMoving)
        {
            projectile = Instantiate(quickProjectile, whileMovingProjectilePos.position, Quaternion.identity);
        } else
        {
            projectile = Instantiate(quickProjectile, quickProjectilePos.position, Quaternion.identity);

        }

        QuickProjectileScript projectileScript = projectile.GetComponent<QuickProjectileScript>();
        projectileScript.projectileType = type;
        projectileScript.force = force;
        projectileScript.angle = angle;
        projectileScript.isFacingLeft = isFacingLeft;
    }



    void MoveToOtherSide()
    {
        if (bossArea != null)
        {
            Bounds bounds = new Bounds(bossArea.position, bossArea.localScale);

            float leftLimit = bounds.min.x + movementWallThresholdDistance;
            float rightLimit = bounds.max.x - movementWallThresholdDistance;
            float distanceToLeftLimit = Mathf.Abs(leftLimit - transform.position.x);
            float distanceToRightLimit = Mathf.Abs(rightLimit - transform.position.x);
            float maxDistance = Mathf.Abs(rightLimit - leftLimit);

            float minSpeed = quickAttackMovementPhaseSpeed / 2f;
            float transitionSmoothness = maxDistance / 2;
            float adjustedSpeed;


            if (isFacingLeft)
            {
                adjustedSpeed = Mathf.Lerp(minSpeed, quickAttackMovementPhaseSpeed, distanceToLeftLimit / transitionSmoothness);
                if (distanceToRightLimit < transitionSmoothness)
                {
                    adjustedSpeed = Mathf.Lerp(minSpeed, quickAttackMovementPhaseSpeed, Mathf.Min(distanceToLeftLimit, distanceToRightLimit) / transitionSmoothness);
                }

                transform.Translate(Vector3.left * adjustedSpeed * Time.deltaTime);
                if (transform.position.x <= leftLimit)
                {
                    isFacingLeft = false;
                }
            }
            else
            {
                adjustedSpeed = Mathf.Lerp(minSpeed, quickAttackMovementPhaseSpeed, distanceToRightLimit / transitionSmoothness);
                if (distanceToLeftLimit < transitionSmoothness)
                {
                    adjustedSpeed = Mathf.Lerp(minSpeed, quickAttackMovementPhaseSpeed, Mathf.Min(distanceToLeftLimit, distanceToRightLimit) / transitionSmoothness);
                }

                transform.Translate(Vector3.right * adjustedSpeed * Time.deltaTime);
                if (transform.position.x >= rightLimit)
                {
                    isFacingLeft = true;
                    hasMovedSides = true;
                }
            }

            isMovingOnX = true;
        }
    }

    void ShootWhileMoving(float angle)
    {
        if (isMovingOnX)
        {
            if (projectileTimer > 0.09f)
            {
                projectileTimer = 0;

                FireQuickProjectile(QuickProjectileScript.ProjectileType.Linear, angle, 20f, true);
                projectilesFired++;

                if (projectilesFired % 15 == 0)
                {
                    projectileTimer = 0 - 0.3f;
                }
            }
        }
    }



    public void AttackPlayerState()
    {
        if (!attackPlayerStateFinished && bossArea != null && player.activeSelf)
        {
            if (!attackPlayerStateStarted)
            {
                timer = 0f;
                projectileTimer = 0;
                lastPosition = transform.position;
                attackRange = Random.Range(minAttackRange, maxAttackRange);
                attackPlayerStateStarted = true;
            }

            float distanceToPlayerX = Mathf.Abs(player.transform.position.x - transform.position.x);

            if (distanceToPlayerX <= attackRange && !attackCompleted)
            {
                enemyAnimator.SetTrigger(AnimationStrings.flyingBoss_attackPlayerDashMovement);

                if (!isLeapingAtPlayer)
                {
                    FlipTowardsPlayer();

                    timer = 0f;
                    isLeapingAtPlayer = true;
                }
                if (isLeapingAtPlayer)
                {
                    if (timer < attackFollowTime)
                    {
                        playerPosition = player.transform.position - transform.position;
                        playerPosition.Normalize();
                    }

                    enemyRB.velocity = attackPlayerSpeed * playerPosition;
                }

                if (isTouchingWall || isTouchingDown)
                {
                    enemyRB.velocity = Vector2.zero;
                    isLeapingAtPlayer = false;
                    attackCompleted = true;
                }
            }
            else if (distanceToPlayerX > attackRange && !attackCompleted)
            {
                if (timer >= 0.5f) // some delay
                {
                    MoveToOtherSide();

                    if (isFacingLeft)
                    {
                        float angle = shootDownwardsAngle;
                        ShootWhileMoving(angle);
                    }
                    else
                    {
                        float angle = -shootDownwardsAngle;
                        ShootWhileMoving(angle);
                    }
                }
            }

            if (attackCompleted)
            {
                enemyAnimator.SetTrigger(AnimationStrings.flyingBoss_attackPlayerReturnMovement);

                Vector3 directionToInitialPosition = (lastPosition - transform.position).normalized;

                Bounds bounds = new Bounds(bossArea.position, bossArea.localScale);

                float rightLimit = bounds.max.x - movementWallThresholdDistance;
                float upperLimit = bounds.max.y - idleMovementUpperThresholdDistance; // upper limit for the idle state

                bool backAtLastPosition = false;
                if (transform.position.y >= upperLimit)
                {
                    directionToInitialPosition.y = 0f;
                    directionToInitialPosition.x = 0f;

                    transform.Translate(Vector3.right * (attackPlayerSpeed / 15) * Time.deltaTime);

                    if (transform.position.x >= rightLimit)
                    {
                        backAtLastPosition = true;
                    }
                } else if (transform.position.x >= rightLimit)
                {
                    directionToInitialPosition.x = 0f;
                    directionToInitialPosition.y = 0f;

                    transform.Translate(Vector3.up * (attackPlayerSpeed / 15) * Time.deltaTime);

                    if (transform.position.y >= upperLimit)
                    {
                        backAtLastPosition = true;
                    }
                }


                if (Vector3.Distance(transform.position, lastPosition) < 0.1f || backAtLastPosition)
                {
                    // finish state
                    enemyRB.velocity = Vector2.zero;
                    isPlayerInRange = false;
                    isLeapingAtPlayer = false;
                    attackCompleted = false;
                    attackRange = 0f;

                    timer = 0f;
                    projectileTimer = 0f;
                    projectilesFired = 0;

                    hasMovedSides = false;
                    isMovingOnX = false;
                    attackPlayerStateFinished = true;
                    attackPlayerStateStarted = false;

                    enemyAnimator.SetBool(AnimationStrings.flyingBoss_attackPlayer, false);
                }
                else
                {
                    enemyRB.velocity = (attackPlayerSpeed / 5) * directionToInitialPosition;
                }

            }
        }
    }

    public void HitPlayerInProximityState()
    {
        if (!hitPlayerInProximityStateFinished && bossArea != null && player.activeSelf)
        {
            if (!hitPlayerInProximityStateStarted)
            {
                lastPosition = transform.position;
                hitPlayerInProximityStateStarted = true;
            }

            if (isPlayerInRange && !attackCompleted) // should not stand directly under the enemy
            {
                if (!isLeapingAtPlayer)
                {
                    FlipTowardsPlayer();

                    timer = 0f;
                    isLeapingAtPlayer = true;
                }

                if (isLeapingAtPlayer)
                {
                    if (timer < proximityHitFollowTime)
                    {
                        playerPosition = player.transform.position - transform.position;
                        playerPosition.Normalize();
                    }

                    enemyRB.velocity = (attackPlayerSpeed * 2f) * playerPosition;
                }


                if (isTouchingWall || isTouchingDown)
                {
                    enemyRB.velocity = Vector2.zero;
                    isLeapingAtPlayer = false;
                    attackCompleted = true;
                }
            }

            if (attackCompleted)
            {
                enemyAnimator.SetTrigger(AnimationStrings.flyingBoss_hitPlayerInProximityReturnMovement);

                Vector3 directionToInitialPosition = (lastPosition - transform.position).normalized;

                Bounds bounds = new Bounds(bossArea.position, bossArea.localScale);

                float rightLimit = bounds.max.x - movementWallThresholdDistance;
                float upperLimit = bounds.max.y - idleMovementUpperThresholdDistance; // upper limit for the idle state

                bool backAtLastPosition = false;
                if (transform.position.y >= upperLimit)
                {
                    directionToInitialPosition.y = 0f;
                    directionToInitialPosition.x = 0f;

                    transform.Translate(Vector3.right * (attackPlayerSpeed / 15) * Time.deltaTime);

                    if (transform.position.x >= rightLimit)
                    {
                        backAtLastPosition = true;
                    }
                }
                else if (transform.position.x >= rightLimit)
                {
                    directionToInitialPosition.x = 0f;
                    directionToInitialPosition.y = 0f;

                    transform.Translate(Vector3.up * (attackPlayerSpeed / 15) * Time.deltaTime);

                    if (transform.position.y >= upperLimit)
                    {
                        backAtLastPosition = true;
                    }
                }


                if (Vector3.Distance(transform.position, lastPosition) < 0.1f || backAtLastPosition)
                {
                    // finish state
                    enemyRB.velocity = Vector2.zero;
                    isPlayerInRange = false;
                    isLeapingAtPlayer = false;
                    attackCompleted = false;
                    attackRange = 0f;

                    isMovingOnX = false;
                    hitPlayerInProximityStateFinished = false; // TODO remove?
                    hitPlayerInProximityStateStarted = false;

                    enemyAnimator.SetBool(AnimationStrings.flyingBoss_hitPlayerInProximity, false);
                }
                else
                {
                    enemyRB.velocity = (attackPlayerSpeed / 5) * directionToInitialPosition;
                }

            }
        }
    }

    void FlipTowardsPlayer()
    {
        float playerDirection = player.transform.position.x - transform.position.x;

        if (playerDirection > 0 && isFacingLeft)
        {
            Flip();
        }
        else if (playerDirection < 0 && !isFacingLeft)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingLeft = !isFacingLeft;
        idleMovementDirection.x *= -1;
        //attackMovementDirection.x *= -1;
        transform.Rotate(0, 180, 0);
    }



    internal void HandleHitpoints()
    {
        hits += 1;
        if (hits >= bossHitpoints)
        {
            ProgressController.instance.SetFlyingEnemyDefeated();
            gameObject.SetActive(false);

            foreach (var obj in objectsToDisableAfterBossFight)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            if (vcamCurrent != null)
            {
                CinemachineFramingTransposer transposer = vcamCurrent.GetCinemachineComponent<CinemachineFramingTransposer>();

                if (transposer != null)
                {
                    transposer.m_LookaheadTime = 0.5f;
                    transposer.m_LookaheadSmoothing = 6f;

                    transposer.m_DeadZoneWidth = 0.1f;
                    transposer.m_DeadZoneHeight = 0.06f;

                    transposer.m_SoftZoneWidth = 0.5f;
                    transposer.m_SoftZoneHeight = 0.5f;
                }
            }
        }
    }

    public void PlayerDefeated()
    {
        if (vcamOnReset != null)
        {
            CameraManager.SwitchCamera(vcamOnReset); // TODO instantly switch to new camera

            foreach (var obj in objectsToResetAfterDefeat)
            {
                if (obj != null)
                {
                    Collider2D colliderComponent = obj.GetComponent<Collider2D>();
                    if (colliderComponent != null)
                    {
                        colliderComponent.enabled = false;
                    }
                }
            }
        }

        RespawnController.instance.AnnouncePlayerDeath();
    }

    /*
    public void ResetEnemy()
    {
        Destroy(gameObject);

        if (vcamOnReset != null)
        {
            CameraManager.SwitchCamera(vcamOnReset);

            foreach (var obj in objectsToDisableAfterBossFight)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
    */
}