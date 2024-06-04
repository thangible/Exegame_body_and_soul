using UnityEngine;

// Create new objects on player death
public class ObjectInstantiator : MonoBehaviour
{
    public GameObject objectToInstantiate; // prefab
    private GameObject currentObject;

    public bool isPrefab = true;
    public string objectTag = ""; // in case its not a prefab, destroy all other objects with this tag


    void Start()
    {
        if (!isPrefab)
        {
            DestroyOtherInstancesWithTag();
        }

        InstantiateNewObject();
    }

    void Update()
    {
        
        // if the player is not alive, destroy the current object and instantiate a new one
        if (!RespawnController.instance.IsPlayerAlive())
        {
            if (currentObject != null)
            {
                Destroy(currentObject);
            }

            // special case for FlyingBoss
            if (objectTag == "FlyingBoss" && ProgressController.instance.HasDefeatedFlyingEnemy())
            {
                // dont spawn a new flying enemy
            } 
            // special case for Falling Platforms
            else if (objectTag == "FallingPlatform" && ProgressController.instance.HasOvercomeFallingPlatforms())
            {
                // dont spawn new Falling Platforms
            }
            else
            {
                InstantiateNewObject();
            }
        }
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
