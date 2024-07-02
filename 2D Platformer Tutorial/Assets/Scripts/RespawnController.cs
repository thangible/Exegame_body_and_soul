using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Respawn player script --> place on main camera
public class RespawnController : MonoBehaviour
{
    public static RespawnController instance;

    [SerializeField] private GameObject player;
    [SerializeField] public Transform respawnPoint;

    public bool isPlayerAlive = true;
    private bool hasRecentlyDied; // otherwise some objects do not get instantiated (player respawns too quickly for that)
    private float recentlyDiedDuration = 1f;

    // introduce lock to isPlayerAlive
    private readonly object playerAliveLock = new object(); // TODO keep or move?


    private void Awake()
    {
        isPlayerAlive = true;
        hasRecentlyDied = false;
        instance = this;
    }

    void Update()
    {
        // handle player death
        if (!isPlayerAlive)
        {
            RespawnPlayer();
        }
    }

    public void RespawnPlayer()
    {
        if (player != null && respawnPoint != null)
        {
            player.transform.position = respawnPoint.position;

            // remove all forces
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            // also (re)activate player
            player.SetActive(true);

            // mark player as alive
            SetPlayerAlive(true);
        }
    }

    public void ChangeRespawnPoint(Transform newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
    }

    public static void DestroyInstance()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
    }

    public void AnnouncePlayerDeath(bool isPlayerDead = true)
    {
        isPlayerAlive = !isPlayerDead;

        if (isPlayerDead)
        {
            StartCoroutine(SetHasRecentlyDied());
        }
    }

    private IEnumerator SetHasRecentlyDied()
    {
        lock (playerAliveLock)
        {
            hasRecentlyDied = true;
        }
        yield return new WaitForSeconds(recentlyDiedDuration);
        lock (playerAliveLock)
        {
            hasRecentlyDied = false;
        }
    }

    public bool HasRecentlyDied()
    {
        lock (playerAliveLock)
        {
            return hasRecentlyDied;
        }
    }

    // preferably use hasRecentlyDied to check this
    public bool IsPlayerAlive()
    {
        lock (playerAliveLock)
        {
            return isPlayerAlive;
        }
    }

    private void SetPlayerAlive(bool alive)
    {
        lock (playerAliveLock)
        {
            isPlayerAlive = alive;
        }
    }

    /* old without lock
    public bool IsPlayerAlive()
    {
        return isPlayerAlive;
    } */

}
