using UnityEngine;

public class Collectable : MonoBehaviour
{
    public float idleMovementSpeed = 1f;
    public float distanceToTravel = 1f;

    private Vector3 startingPosition;
    private bool isGoingUp = true;


    void Start()
    {
        startingPosition = transform.position;
    }

    void Update()
    {
        IdleMovement();
    }

    void IdleMovement()
    {
        float targetPositionY = isGoingUp ? startingPosition.y + distanceToTravel : startingPosition.y;

        float movementDirection = isGoingUp ? 1f : -1f;
        float maxSpeed = idleMovementSpeed;
        float minSpeed = idleMovementSpeed / 3f;

        float newY = Mathf.Lerp(transform.position.y, targetPositionY, Time.deltaTime * idleMovementSpeed);
        float currentSpeed = Mathf.Max(Mathf.Abs(newY - transform.position.y) / Time.deltaTime, minSpeed);

        float movementSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed) * movementDirection;
        transform.position += Vector3.up * movementSpeed * Time.deltaTime;

        if ((isGoingUp && transform.position.y >= targetPositionY) ||
            (!isGoingUp && transform.position.y <= targetPositionY))
        {
            transform.position = new Vector3(transform.position.x, targetPositionY, transform.position.z);
            isGoingUp = !isGoingUp;
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameObject.SetActive(false);
            ProgressController.instance.SetHasPickedUpAttack();
            SoundManager.instance.PlaySound3D("Pickup", transform.position);
        }
    }
}
