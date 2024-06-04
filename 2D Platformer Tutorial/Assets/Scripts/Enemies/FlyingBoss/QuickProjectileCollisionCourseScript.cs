using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickProjectileCollisionCourseScript : MonoBehaviour
{

    [SerializeField] public float force = 15f;
    //[SerializeField] float rotationSpeed = 5f;

    private GameObject player;
    private PlayerInput playerInput;
    private GameObject enemy;
    private FlyingBoss enemyScript;

    private Rigidbody2D rb;
    private Collider2D coll;
    private Vector3 direction;

    private float acceptedHitboxDistance = 2f;
    private float blockMoveTimeframe = 0.4f; // to make it easier to block with the right timing
    private float lastBlockMoveTime;

    private bool canCollideWithEnemy = false;
    private float timer;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        coll.enabled = false;
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && player.activeSelf)
        {
            //playerInput = player.GetComponent<PlayerInput>();
            enemy = GameObject.FindGameObjectWithTag("FlyingBoss");
            enemyScript = enemy.GetComponent<FlyingBoss>();

            rb.velocity = Vector2.zero;
            UpdateDirection();
        }

        timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (!canCollideWithEnemy && timer >= 0.5f)
        {
            canCollideWithEnemy = true;
            coll.enabled = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            lastBlockMoveTime = Time.time;
        }

        if (player != null && player.activeSelf)
        {
            UpdateDirection();

            // projectile in range ?
            if ((Vector2.Distance(transform.position, player.transform.position) <= acceptedHitboxDistance))
            {
                // button pressed in the last time
                if (Time.time - lastBlockMoveTime <= blockMoveTimeframe)
                {
                    Destroy(gameObject);
                }
            }
        }

        // continuous rotation (buggy)
        //transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    // Follow the player
    private void UpdateDirection()
    {
        if (player != null && player.activeSelf)
        {
            Vector3 direction = player.transform.position - transform.position;
            direction.Normalize();
            transform.position += direction * force * Time.deltaTime;

            float rot = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            int rotationAngle = 90;
            transform.rotation = Quaternion.Euler(0, 0, rot + rotationAngle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);

            collision.gameObject.SetActive(false);
            enemyScript.PlayerDefeated();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }

}
