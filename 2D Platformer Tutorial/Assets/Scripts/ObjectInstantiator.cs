using System.Collections;
using UnityEngine;

// Create new objects on player death
public class ObjectInstantiator : MonoBehaviour
{
    public GameObject objectToInstantiate; // prefab
    private GameObject currentObject;

    public bool isPrefab = true;
    public bool instantiateByDefault = true;
    private float initDelay = 2f;
    private bool nonDefaultInstantiationComplete = false;

    public string objectTag = ""; // in case its not a prefab, destroy all other objects with this tag


    void Start()
    {
        StartCoroutine(DelayedInitialization());
    }

    IEnumerator DelayedInitialization()
    {
        yield return new WaitForEndOfFrame();

        if (!isPrefab)
        {
            DestroyOtherInstancesWithTag();
        }

        yield return new WaitForSeconds(initDelay);

        if (instantiateByDefault)
        {
            if (ShouldObjectRespawn()) // NEWLY ADDED TODO KEEP?
            {
                InstantiateNewObject();
            } 
        }
    }


    void Update()
    {
        // handle objects that have not been instantiated by default
        if (!nonDefaultInstantiationComplete)
        {
            // puzzle finished barrier
            if (objectTag == "PuzzleFinishedBarrier" && ProgressController.instance.HasSolvedPuzzle())
            {
                InstantiateNewObject();
                nonDefaultInstantiationComplete = true;
            }
        }


        if (RespawnController.instance.HasRecentlyDied())
        {
            HandlePlayerDeath();

            // on player death, destroy remaining projectiles
            if (objectTag == "FlyingBoss")
            {
                SlowProjectileScript.DestroyAllProjectiles();
                QuickProjectileScript.DestroyAllProjectiles();
                QuickProjectileCollisionCourseScript.DestroyAllProjectiles();
            }
        }

        // in special cases, destroy the object forever
        if (objectTag == "FlyingBossBarrier" && ProgressController.instance.HasDefeatedFlyingEnemy())
        {
            if (currentObject != null)
            {
                Destroy(currentObject);
            }
        }
    }

    void HandlePlayerDeath()
    {
        // if the player is not alive, destroy the current object and instantiate a new one
        if (currentObject != null)
        {
            Destroy(currentObject);
        }

        if (!ShouldObjectRespawn())
        {
            return;
        }

        InstantiateNewObject();
    }

    bool ShouldObjectRespawn()
    {
        if (objectTag == "FlyingBoss" && ProgressController.instance.HasDefeatedFlyingEnemy())
        {
            // dont spawn a new flying enemy
            return false;
        }
        else if (objectTag == "FlyingBossBarrier" && ProgressController.instance.HasDefeatedFlyingEnemy())
        {
            // dont spawn new Flying Boss Barrier (barrier behind boss)
            return false;
        }
        else if (objectTag == "FirstFallingPlatforms" && ProgressController.instance.HasOvercomeFirstFallingPlatforms())
        {
            // dont spawn new Falling Platforms
            return false;
        }
        else if (objectTag == "LastFallingPlatforms" && ProgressController.instance.HasOvercomeLastFallingPlatforms())
        {
            // dont spawn new Falling Platforms
            return false;
        }
        else if (objectTag == "PuzzleFinishedBarrier" && !ProgressController.instance.HasSolvedPuzzle())
        {
            // dont spawn new puzzle finished barrier
            return false;
        }
        else if (objectTag == "AttackCollectable" && ProgressController.instance.HasPickedUpAttack())
        {
            // dont spawn new attack collectable
            return false;
        }

        return true;
    }

    void InstantiateNewObject()
    {
        currentObject = Instantiate(objectToInstantiate, transform.position, Quaternion.identity);


        // special case for FlyingBoss
        if (currentObject.GetComponent<FlyingBoss>() != null)
        {
            Collider2D colliderComponent = currentObject.GetComponent<Collider2D>();
            if (colliderComponent != null)
            {
                colliderComponent.enabled = true;
            }

            FlyingBoss scriptComponent = currentObject.GetComponent<FlyingBoss>();
            if (scriptComponent != null)
            {
                scriptComponent.enabled = true;
            }

            Animator animatorComponent = currentObject.GetComponent<Animator>();
            if (animatorComponent != null)
            {
                animatorComponent.enabled = true;
            }
        }

        currentObject.SetActive(true);
    }

    void DestroyOtherInstancesWithTag()
    {
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(objectTag);

        foreach (GameObject obj in objectsWithTag)
        {
            if (obj != currentObject)
            {
                obj.SetActive(false);
            }
        }
    }

}
