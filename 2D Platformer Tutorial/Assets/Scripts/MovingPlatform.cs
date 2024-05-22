using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tutorial: https://www.youtube.com/watch?v=SPmO_7Y5DLY
public class MovingPlatform : MonoBehaviour
{
    private Vector3 sourceLocation;
    private Vector3 targetLocation;
    private Vector3 nextLocation;

    [SerializeField] private Transform platform;

    [SerializeField] private Transform movingToLocation;

    public float platformSpeed = 1;


    // Start is called before the first frame update
    void Start()
    {
        sourceLocation = platform.localPosition;
        targetLocation = movingToLocation.localPosition;
        nextLocation = targetLocation;
    }

    // Update is called once per frame
    void Update()
    {
        MovePlatform();
    }

    private void MovePlatform()
    {
        platform.localPosition = Vector3.MoveTowards(platform.localPosition, nextLocation, platformSpeed * Time.deltaTime);

        if (Vector3.Distance(platform.localPosition, nextLocation) <= 0.01)
        {
            nextLocation = nextLocation != sourceLocation ? sourceLocation : targetLocation;
        }
    }
}
