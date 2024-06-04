using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpikeWindSectionScript : MonoBehaviour
{
    [SerializeField] public float minForce = 10f;
    [SerializeField] public float maxForce = 15f;
    [SerializeField] public float minAngle = 80f;
    [SerializeField] public float maxAngle = 100f;

    [SerializeField] public float notEnoughMovementThreshold = 5f;
    [SerializeField] public float allowedStationaryTime = 2f;

    [SerializeField] public float fallDistanceThreshold = 5f;
    [SerializeField] public float allowedFallTime = 1f;

    private GameObject player;

    private Rigidbody2D rb;
    private Collider2D coll;
    private Vector3 direction;
    private TrailRenderer trailRenderer;

    private float force;
    private float angle;

    private bool canCollideWithProjectile = false;
    private float timer;
    private Vector2 lastPosition;

    private float stationaryTimer = 0f;
    private float fallTimer = 0f;
    private float fallDistance = 0f;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();

        player = GameObject.FindGameObjectWithTag("Player");

        force = Random.Range(minForce, maxForce);
        angle = Random.Range(minAngle, maxAngle);

        if (player != null && player.activeSelf)
        {
            direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            rb.AddForce(direction * force, ForceMode2D.Impulse);

            float rot = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            int rotationAngle = 90;
            transform.rotation = Quaternion.Euler(0, 0, rot + rotationAngle);
        }

        timer = 0f;
        lastPosition = rb.position;
        stationaryTimer = 0f;
        fallTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (!canCollideWithProjectile && timer >= 1.5f)
        {
            canCollideWithProjectile = true;
        }


        // check for stationary movement --> destroy
        if (Vector2.Distance(lastPosition, rb.position) < notEnoughMovementThreshold)
        {
            stationaryTimer += Time.deltaTime;
        }
        else
        {
            stationaryTimer = 0f;
            lastPosition = rb.position;
        }

        if (stationaryTimer >= allowedStationaryTime)
        {
            Destroy(gameObject);
        }


        // check for y downwards movement --> destroy
        if (rb.velocity.y < 0)
        {
            fallTimer += Time.deltaTime;
            fallDistance += Mathf.Abs(rb.velocity.y) * Time.deltaTime;
        }
        else
        {
            fallTimer = 0f;
            fallDistance = 0f;
        }

        if (fallTimer >= allowedFallTime && fallDistance >= fallDistanceThreshold)
        {
            Destroy(gameObject);
        }

        // continuous rotation (buggy)
        //transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);

            collision.gameObject.SetActive(false);
            RespawnController.instance.AnnouncePlayerDeath();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || (collision.gameObject.CompareTag("Projectile") && canCollideWithProjectile))
        {
            Vector2 normal = collision.ClosestPoint(transform.position) - (Vector2)transform.position;
            Vector2 reflection = Vector2.Reflect(rb.velocity, normal.normalized);
            float dampingFactor = 0.6f;
            rb.velocity = reflection * dampingFactor;

            // also update rotation to align with the new direction
            float rot = Mathf.Atan2(-reflection.y, -reflection.x) * Mathf.Rad2Deg;
            int angle = 90;
            transform.rotation = Quaternion.Euler(0, 0, rot + angle);
        }
        else if (collision.gameObject.CompareTag("DespawnCollider"))
        {
            Destroy(gameObject);
        }
    }
}
