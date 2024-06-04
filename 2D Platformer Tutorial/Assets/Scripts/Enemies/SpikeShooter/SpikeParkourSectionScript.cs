using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SpikeParkourSectionScript : MonoBehaviour
{
    [SerializeField] public float force = 10f;
    [SerializeField] public float angle = 0;

    private GameObject player;

    private Rigidbody2D rb;
    private Collider2D coll;
    private Vector3 direction;
    private TrailRenderer trailRenderer;

    private bool canCollideWithGround = false;
    private float timer;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && player.activeSelf)
        {
            direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            rb.AddForce(direction * force, ForceMode2D.Impulse);

            float rot = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            int rotationAngle = 90;
            transform.rotation = Quaternion.Euler(0, 0, rot + rotationAngle);
        }

        timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (!canCollideWithGround && timer >= 0.5f)
        {
            canCollideWithGround = true;
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
        else if (canCollideWithGround && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}

