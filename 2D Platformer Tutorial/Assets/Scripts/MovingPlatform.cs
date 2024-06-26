using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MovingPlatform;
using static UnityEngine.UI.ScrollRect;

// Tutorial: https://www.youtube.com/watch?v=SPmO_7Y5DLY
public class MovingPlatform : MonoBehaviour
{
    private Vector3 sourceLocation;
    private Vector3 targetLocation;
    private Vector3 nextLocation;
    [SerializeField] private SpriteRenderer platform;
    public enum PlatformType
    {
        Single,
        DoubleParallel,
        DoubleCollision
    }
    public PlatformType platformType = PlatformType.Single;
    public bool movePlatformOnce = false; // will only move once until the target has been reached
    public bool hasTrigger = false;

    [SerializeField] private Transform movingToLocation;
    private bool reverseMovement = false;

    public float minMovementSpeed = 0.5f;
    public float maxMovementSpeed = 1;
    public enum MovementType
    {
        Linear,
        Spring,
        Acceleration,
        TwoWayAcceleration
    }
    public MovementType movementType = MovementType.Linear;

    public float minDelay = 0f;
    public float maxDelay = 2.0f;


    private Vector3 counterpartSourceLocation;
    private Vector3 counterpartTargetLocation;
    private Vector3 counterpartNextLocation;
    [SerializeField] private SpriteRenderer counterpartPlatform;

    private BoxCollider2D platformCollider;
    private BoxCollider2D counterpartCollider;

    public float collisionCooldown = 0.5f;
    private bool collison = false;
    private bool collisionOnCooldown = false;
    private float collisionOnCooldownTimer;

    private float delayTimer = 0.0f;
    public bool isMoving = false;
    private float maxDistancePlatform;
    private float maxDistanceCounterpartPlatform;


    // Start is called before the first frame update
    void Start()
    {
        sourceLocation = platform.transform.localPosition;
        targetLocation = movingToLocation.localPosition;
        nextLocation = targetLocation;

        platformCollider = platform.GetComponent<BoxCollider2D>();
        maxDistancePlatform = Vector3.Distance(sourceLocation, targetLocation);


        if (counterpartPlatform != null)
        {   
            if (platformType == PlatformType.DoubleParallel)
            {
                Vector3 distance = platform.transform.localPosition - counterpartPlatform.transform.localPosition;

                counterpartSourceLocation = counterpartPlatform.transform.localPosition;
                counterpartTargetLocation = movingToLocation.localPosition - distance;
                counterpartNextLocation = counterpartTargetLocation;


            } else if (platformType == PlatformType.DoubleCollision)
            {
                Vector3 direction = nextLocation - sourceLocation;

                counterpartSourceLocation = counterpartPlatform.transform.localPosition;
                counterpartTargetLocation = counterpartSourceLocation - direction;
                counterpartNextLocation = counterpartTargetLocation;

            }

            counterpartCollider = counterpartPlatform.GetComponent<BoxCollider2D>();
            maxDistanceCounterpartPlatform = Vector3.Distance(counterpartSourceLocation, counterpartTargetLocation);
        }

        collisionOnCooldownTimer = collisionCooldown;
        delayTimer = Random.Range(minDelay, maxDelay);
    }

    // Update is called once per frame
    void Update()
    {   
        if (hasTrigger)
        {
            if (!isMoving)
            {
                return;
            } else
            {
                delayTimer -= Time.deltaTime;
                if (delayTimer <= 0)
                {
                    MovePlatform();
                }
            }

        } else
        {
            if (!isMoving)
            {
                delayTimer -= Time.deltaTime;
                if (delayTimer <= 0)
                {
                    isMoving = true;
                    MovePlatform();
                }
            } else
            {
                MovePlatform();
            }
        }


        UpdateCollisionCooldown();
    }

    private void MovePlatform()
    {
        float distanceToTarget = Vector3.Distance(platform.transform.localPosition, nextLocation);
        float adjustedSpeed = CalculateSpeed(distanceToTarget, maxDistancePlatform);

        // Move platform
        platform.transform.localPosition = Vector3.MoveTowards(platform.transform.localPosition, nextLocation, adjustedSpeed * Time.deltaTime);

        // Has a second platform and both collide with each other
        if (counterpartPlatform != null)
        {
            // Move second platform
            if (platformType == PlatformType.DoubleParallel)
            {
                counterpartPlatform.transform.localPosition = Vector3.MoveTowards(counterpartPlatform.transform.localPosition, counterpartNextLocation, adjustedSpeed * Time.deltaTime);

            } else if (platformType == PlatformType.DoubleCollision)
            {
                distanceToTarget = Vector3.Distance(counterpartPlatform.transform.localPosition, counterpartNextLocation);
                adjustedSpeed = CalculateSpeed(distanceToTarget, maxDistanceCounterpartPlatform);
                counterpartPlatform.transform.localPosition = Vector3.MoveTowards(counterpartPlatform.transform.localPosition, counterpartNextLocation, adjustedSpeed * Time.deltaTime);

                // Handle collision
                if (!collisionOnCooldown && platformCollider.IsTouching(counterpartCollider))
                {
                    collison = true;
                    collisionOnCooldown = true;
                    Physics2D.IgnoreCollision(platformCollider, counterpartCollider);
                }
            }
        }

        // movement after collision
        if (collison || (!collison && Vector3.Distance(platform.transform.localPosition, nextLocation) <= 0.01f)) // depending on the first platform (because they move in sync)
        {
            if (!movePlatformOnce)
            {
                nextLocation = nextLocation != sourceLocation ? sourceLocation : targetLocation;
                reverseMovement = !reverseMovement;

                if (counterpartPlatform != null)
                {
                    counterpartNextLocation = counterpartNextLocation != counterpartSourceLocation ? counterpartSourceLocation : counterpartTargetLocation;
                }

            } else
            {
                // in case platform should only move once
                isMoving = false;
            }
        }
        
        collison = false;
    }


    private void UpdateCollisionCooldown()
    {
        if (collisionOnCooldown)
        {
            collisionOnCooldownTimer -= Time.deltaTime;
            if (collisionOnCooldownTimer <= 0)
            {
                collisionOnCooldown = false;
                collisionOnCooldownTimer = collisionCooldown;
                Physics2D.IgnoreCollision(platformCollider, counterpartCollider, false);
            }
        }
    }


    private float CalculateSpeed(float distanceToTarget, float maxDistance)
    {
        float distanceFactor = (distanceToTarget / maxDistance);
        float newDistanceFactor = Mathf.Lerp(1f, 2f, 1 - distanceFactor);
        float adjustedSpeed = maxMovementSpeed;

        switch (movementType)
        {
            case MovementType.Acceleration:
                newDistanceFactor = Mathf.Lerp(1f, 2f, 1 - distanceFactor);

                if (!reverseMovement)
                {
                    if (distanceFactor > 0.2f)
                    {
                        adjustedSpeed *= Mathf.Pow(newDistanceFactor, 2);

                    }
                } else
                {
                    adjustedSpeed = minMovementSpeed * Mathf.Pow(newDistanceFactor, 2);
                }
                    
                return adjustedSpeed;

            case MovementType.TwoWayAcceleration:
                newDistanceFactor = Mathf.Lerp(1f, 2f, 1 - distanceFactor);

                if (distanceFactor > 0.2f)
                {
                    adjustedSpeed *= Mathf.Pow(newDistanceFactor, 2);
                } else
                {
                    adjustedSpeed = minMovementSpeed * Mathf.Pow(newDistanceFactor, 2);
                }

                return adjustedSpeed;

            case MovementType.Spring:
                newDistanceFactor = Mathf.Lerp(1f, 2f, distanceFactor);

                if (!reverseMovement)
                {
                    adjustedSpeed *= Mathf.Pow(newDistanceFactor, 2);
                }
                else
                {   
                    if (distanceFactor > 0.05f)
                    {
                        newDistanceFactor = Mathf.Lerp(1f, 2f, 1 - distanceFactor);
                        adjustedSpeed = minMovementSpeed * Mathf.Pow(newDistanceFactor, 2);
                    } else
                    {
                        adjustedSpeed = minMovementSpeed / 5;
                    }
                }

                return adjustedSpeed;

            case MovementType.Linear:
                return adjustedSpeed;

            default:
                return adjustedSpeed;
        }
    }



    // Some useful, isolated functionality

    public void TriggerMovement()
    {   
        if (!isMoving)
        {
            isMoving = true;
            delayTimer = Random.Range(minDelay, maxDelay);

            // Prevent movePlatformOnce = true, so isMoving does not get set back to false immediately in the MovePlatform method (since we probably call this method in the default position)
            StartCoroutine(TemporarilyDisableMovePlatformOnce());
        } else
        {
            ChangeDirection();
        }
    }

    private IEnumerator TemporarilyDisableMovePlatformOnce()
    {
        if (movePlatformOnce)
        {
            movePlatformOnce = false;
            yield return new WaitForSeconds(0.1f);
            movePlatformOnce = true;
        }
    }


    public void ChangeDirection()
    {
        nextLocation = nextLocation == targetLocation ? sourceLocation : targetLocation;
        if (counterpartPlatform != null)
        {
            counterpartNextLocation = counterpartNextLocation == counterpartTargetLocation ? counterpartSourceLocation : counterpartTargetLocation;
        }
    }

}
