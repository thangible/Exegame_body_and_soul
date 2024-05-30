using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Create trigger zones (https://www.youtube.com/watch?v=1BdR5d1JTEI)
public class GeneralTriggerZone : MonoBehaviour
{
    public bool triggerOnce = false;
    private bool alreadyEntered = false;
    private bool alreadyExited = false;

    public string collisionTag = "Player";

    public UnityEvent onTriggerEnterFromLeft;
    public UnityEvent onTriggerEnterFromRight;
    public UnityEvent onTriggerExitToLeft;
    public UnityEvent onTriggerExitToRight;


    void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (alreadyEntered)
            return;

        if (!string.IsNullOrEmpty(collisionTag) && !collision.CompareTag(collisionTag))
            return;

        Vector2 playerPosition = collision.transform.position;
        Vector2 triggerPosition = transform.position;

        float horizontalDifference = playerPosition.x - triggerPosition.x;

        if (horizontalDifference > 0)
        {
            onTriggerEnterFromRight?.Invoke();
        }
        else
        {
            onTriggerEnterFromLeft?.Invoke();
        }

        if (triggerOnce)
            alreadyEntered = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (alreadyExited)
            return;

        if (!string.IsNullOrEmpty(collisionTag) && !collision.CompareTag(collisionTag))
            return;


        Vector2 playerPosition = collision.transform.position;
        Vector2 triggerPosition = transform.position;

        float horizontalDifference = playerPosition.x - triggerPosition.x;

        if (horizontalDifference > 0)
            onTriggerExitToRight?.Invoke();
        else
            onTriggerExitToLeft?.Invoke();

        if (triggerOnce)
            alreadyExited = true;
    }

}