using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Respawn player script --> place on main camera
public class RespawnController : MonoBehaviour
{
    public static RespawnController instance;

    [SerializeField] private GameObject player;
    [SerializeField] public Transform respawnPoint;

    public bool isPlayerAlive;


    private void Awake()
    {
        isPlayerAlive = true;
        instance = this;
    }

    void Update()
    {
        // handle player death
        if (!RespawnController.instance.isPlayerAlive)
        {
            RespawnController.instance.RespawnPlayer();
            RespawnController.instance.isPlayerAlive = true;
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
        }
    }

    public void ChangeRespawnPoint(Transform newRespawnPoint)
    {
        RespawnController.instance.respawnPoint = newRespawnPoint;
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
    }

    public bool IsPlayerAlive()
    {
        return isPlayerAlive;
    }

}
