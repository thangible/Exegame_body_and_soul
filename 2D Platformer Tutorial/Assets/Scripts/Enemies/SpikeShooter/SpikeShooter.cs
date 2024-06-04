using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeShooter : MonoBehaviour
{
    public enum SpikeSectionType
    {
        Wind,
        Parkour
    }

    [SerializeField] GameObject spikeProjectile;
    [SerializeField] Transform spikeProjectilePos;
    [SerializeField] SpikeSectionType spikeSectionType = SpikeSectionType.Parkour;
    [SerializeField] GameObject player;

    [Header("Parkour only")]
    [SerializeField] public float force = 15f;

    [Header("Wind only")]
    [SerializeField] public float minForce = 10f;
    [SerializeField] public float maxForce = 15f;
    [SerializeField] public float minAngle = 80f;
    [SerializeField] public float maxAngle = 100f;

    [Header("Other")]
    [SerializeField] public float shootingInterval = 2f;
    [SerializeField] public bool useRandomTimer = true;
    [SerializeField] public float startingTime = 0f;
    [SerializeField] public bool isFacingLeft = true;

    private float timer;


    // Start is called before the first frame update
    void Start()
    {
        if (useRandomTimer)
        {
            timer = Random.Range(-1f, 1f);
        }
        else
        {
            timer = startingTime;
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        ShootSpikes();
    }

    void ShootSpikes()
    {
        if (player != null && player.activeSelf)
        {
            if (timer > shootingInterval)
            {
                timer = 0;

                FireSpikeProjectile();
            }
        }
    }

    void FireSpikeProjectile()
    {
        GameObject spikeProjectile;
        spikeProjectile = Instantiate(this.spikeProjectile, spikeProjectilePos.position, Quaternion.identity);

        if (spikeSectionType == SpikeSectionType.Wind)
        {
            SpikeWindSectionScript spikeScript = spikeProjectile.GetComponent<SpikeWindSectionScript>();
            if (spikeScript != null)
            {
                spikeScript.minForce = minForce;
                spikeScript.maxForce = maxForce;

                if (isFacingLeft)
                {
                    spikeScript.minAngle = 180f - minAngle;
                    spikeScript.maxAngle = 180f - maxAngle;
                } else
                {
                    spikeScript.minAngle = minAngle;
                    spikeScript.maxAngle = maxAngle;
                }
            }
        } 
        else // Parkour
        {
            SpikeParkourSectionScript spikeScript = spikeProjectile.GetComponent<SpikeParkourSectionScript>();
            if (spikeScript != null)
            {
                spikeScript.force = force;

                if (isFacingLeft)
                {
                    spikeScript.angle = 180f - spikeScript.angle;
                }
            }
        }
    }

}
