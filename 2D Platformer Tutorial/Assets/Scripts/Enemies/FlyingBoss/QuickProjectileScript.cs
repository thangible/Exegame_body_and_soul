using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickProjectileScript : MonoBehaviour
{
    public static List<QuickProjectileScript> quickProjectiles = new List<QuickProjectileScript>();
    private static bool isBlockAvailable = true;


    public enum ProjectileType
    {
        FocusPlayer,
        Linear
    }

    [SerializeField] public float force = 15f;
    [SerializeField] public ProjectileType projectileType = ProjectileType.FocusPlayer;
    [SerializeField] public float angle = 90;
    [SerializeField] public bool isFacingLeft = true;
    //[SerializeField] float rotationSpeed = 5f;

    private GameObject player;
    private PlayerInput playerInput;
    private GameObject enemy;
    private FlyingBoss enemyScript;

    private Rigidbody2D rb;
    private Collider2D coll;
    private Vector3 direction;

    // allow blocking (same as attacking for the slow projectile)
    private float acceptedHitboxDistance = 2f;
    private float blockMoveTimeframe = 0.3f; // to make it easier to block with the right timing
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
            playerInput = player.GetComponent<PlayerInput>();
            enemy = GameObject.FindGameObjectWithTag("FlyingBoss");
            enemyScript = enemy.GetComponent<FlyingBoss>();

            if (projectileType == ProjectileType.FocusPlayer)
            {
                direction = player.transform.position - transform.position;
                rb.velocity = new Vector2(direction.x, direction.y).normalized * force;
            }
            else if (projectileType == ProjectileType.Linear)
            {
                Vector2 direction = isFacingLeft ? Vector2.left : Vector2.right;

                // Rotate the initial direction by the given angle
                direction = Quaternion.Euler(0, 0, angle) * direction;

                // Set the velocity to move in the determined direction
                rb.velocity = direction.normalized * force;
            }

            float rot = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            int rotationAngle = 90;
            transform.rotation = Quaternion.Euler(0, 0, rot + rotationAngle);
        }

        timer = 0f;

        quickProjectiles.Add(this);
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

        // replaced by OnPlayerAttackMove 
        /*
        if (Input.GetMouseButtonDown(0))
        {
            lastBlockMoveTime = Time.time;
        }*/

        if (player != null && player.activeSelf)
        {
            // projectile in range ?
            if ((Vector2.Distance(transform.position, player.transform.position) <= acceptedHitboxDistance))
            {
                // button pressed in the last time
                if (Time.time - lastBlockMoveTime <= blockMoveTimeframe && isBlockAvailable)
                {
                    gameObject.gameObject.SetActive(false);
                    //Destroy(gameObject);

                    isBlockAvailable = false;
                    MakeBlockAvailable();
                }
            }
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
            enemyScript.PlayerDefeated();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }


    public void OnPlayerAttackMove()
    {
        lastBlockMoveTime = Time.time;
    }


    private void OnDestroy()
    {
        quickProjectiles.Remove(this);
    }

    public static void DestroyAllProjectiles()
    {
        foreach (var projectile in quickProjectiles.ToArray())
        {
            if (projectile != null)
            {
                Destroy(projectile.gameObject);
            }
        }
        quickProjectiles.Clear();
    }

    private static IEnumerator MakeBlockAvailable()
    {
        yield return new WaitForSeconds(0.5f); // not relevant anymore, since the attack move from the player has its own cooldown
        isBlockAvailable = true;
    }

}
