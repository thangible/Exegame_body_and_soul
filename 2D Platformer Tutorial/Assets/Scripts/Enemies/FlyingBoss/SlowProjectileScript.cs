using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class SlowProjectileScript : MonoBehaviour
{
    public static List<SlowProjectileScript> slowProjectiles = new List<SlowProjectileScript>();


    [SerializeField] public float force = 1f;
    [SerializeField] int maxBounces = 6;
    public bool destroyOnNextBounce = false;
    //[SerializeField] float rotationSpeed = 5f;

    private GameObject player;
    private PlayerInput playerInput;
    private GameObject enemy;
    private FlyingBoss enemyScript;

    private Rigidbody2D rb;
    private Vector3 direction;
    private TrailRenderer trailRenderer;

    // allow attacking to bounce these projectiles back at the enemy
    private float acceptedHitboxDistance = 2f;
    private float attackMoveTimeframe = 0.3f; // to make it easier to hit the right timing
    private float lastAttackMoveTime;

    private bool changedDirectionToEnemy = false;
    private int currentBounces = 0;
    private bool canCollideWithEnemy = false;
    private float timer;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && player.activeSelf)
        {
            //playerInput = player.GetComponent<PlayerInput>();
            enemy = GameObject.FindGameObjectWithTag("FlyingBoss");
            enemyScript = enemy.GetComponent<FlyingBoss>();

            direction = player.transform.position - transform.position;
            rb.velocity = new Vector2(direction.x, direction.y).normalized * force;

            float rot = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            int rotationAngle = 90;
            transform.rotation = Quaternion.Euler(0, 0, rot + rotationAngle);
        }

        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }

        timer = 0f;

        slowProjectiles.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (!canCollideWithEnemy && timer >= 0.5f)
        {
            canCollideWithEnemy = true;
        }

        // replaced by OnPlayerAttackMove 
        /*
        if (Input.GetMouseButtonDown(0))
        {
            lastAttackMoveTime = Time.time;
        }
        */

        if (player != null && player.activeSelf)
        {
            // projectile on its way to the enemy && projectile in range ?
            if (!changedDirectionToEnemy && (Vector2.Distance(transform.position, player.transform.position) <= acceptedHitboxDistance))
            {
                // button pressed in the last time
                if (Time.time - lastAttackMoveTime <= attackMoveTimeframe)
                {
                    changedDirectionToEnemy = true;
                    //redirectProjectile();

                    trailRenderer.enabled = true;
                }
            }
        }

        // continuous rotation (buggy)
        //transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    private void redirectProjectile()
    {
        /* auto aim
        direction = enemy.transform.position - transform.position;
        rb.velocity = new Vector2(direction.x, direction.y).normalized * force;

        float rot = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
        int angle = 90;
        transform.rotation = Quaternion.Euler(0, 0, rot + angle);
        */

        direction = transform.position - player.transform.position;
        rb.velocity = new Vector2(direction.x, direction.y).normalized * force;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);

            collision.gameObject.SetActive(false);
            enemyScript.PlayerDefeated();
        } 
        else if (collision.gameObject.CompareTag("FlyingBoss") && changedDirectionToEnemy)
        {
            Destroy(gameObject);
            enemyScript.HandleHitpoints();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || (canCollideWithEnemy && collision.gameObject.CompareTag("FlyingBoss")))
        {
            if (changedDirectionToEnemy)
            {
                Destroy(gameObject);
            } 
            else
            {
                if (destroyOnNextBounce)
                {
                    Destroy(gameObject);
                }

                Vector2 normal = collision.ClosestPoint(transform.position) - (Vector2)transform.position;
                Vector2 reflection = Vector2.Reflect(rb.velocity, normal.normalized);
                rb.velocity = reflection;

                // also update rotation to align with the new direction
                float rot = Mathf.Atan2(-reflection.y, -reflection.x) * Mathf.Rad2Deg;
                int angle = 90;
                transform.rotation = Quaternion.Euler(0, 0, rot + angle);

                currentBounces++;
                if (currentBounces >= maxBounces)
                {
                    Destroy(gameObject);
                }
            }
        }
    }


    public void OnPlayerAttackMove()
    {
        lastAttackMoveTime = Time.time;
        redirectProjectile();
    }


    private void OnDestroy()
    {
        slowProjectiles.Remove(this);
    }

    public static void DestroyAllProjectiles()
    {
        foreach (var projectile in slowProjectiles.ToArray())
        {
            if (projectile != null)
            {
                Destroy(projectile.gameObject);
            }
        }
        slowProjectiles.Clear();
    }

}
