using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// create trigger zones (https://www.youtube.com/watch?v=1BdR5d1JTEI) 
public class TriggerZone : MonoBehaviour
{
    public bool triggerOnce = false;
    private bool alreadyEntered = false;
    private bool alreadyExited = false;

    public string collisionTag = "Player";

    public GameObject cameraBoundsWallCollider;
    public bool activateWallColliderFromLeft = true; // else FromRight
    public bool removeWallCollider = true;
    private BoxCollider2D wallCollider;

    public UnityEvent onTriggerEnterFromLeft;
    public UnityEvent onTriggerEnterFromRight;
    public UnityEvent onTriggerExitToLeft;
    public UnityEvent onTriggerExitToRight;

    void Start()
    {
        if (cameraBoundsWallCollider != null)
        {
            wallCollider = cameraBoundsWallCollider.GetComponent<BoxCollider2D>();
            if (wallCollider != null)
            {
                wallCollider.enabled = false;
            }
        }
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

            if (wallCollider != null && !activateWallColliderFromLeft)
            {
                if (!removeWallCollider)
                {
                    StartCoroutine(EnableWallColliderWithDelay());
                }
                else
                {
                    wallCollider.enabled = false;
                }
            }
        }
        else
        {
            onTriggerEnterFromLeft?.Invoke();

            if (wallCollider != null && activateWallColliderFromLeft)
            {   
                if (!removeWallCollider)
                {
                    StartCoroutine(EnableWallColliderWithDelay());
                } else
                {
                    wallCollider.enabled = false;
                }
            }
        }

        if (triggerOnce)
            alreadyEntered = true;
    }

    private IEnumerator EnableWallColliderWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (wallCollider != null)
        {
            wallCollider.enabled = true;
        }
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