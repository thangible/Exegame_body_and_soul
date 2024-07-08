using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// create trigger zones (based on https://www.youtube.com/watch?v=1BdR5d1JTEI) 
public class CameraTriggerZone : MonoBehaviour
{
    public bool triggerOnce = false;
    private bool alreadyEntered = false;
    private bool alreadyExited = false;

    public string collisionTag = "Player";

    public GameObject cameraBoundsWallCollider;
    public bool activateWallColliderFromLeft = true; // else FromRight
    public bool removeWallCollider = true;
    private Collider2D wallCollider;

    public UnityEvent onTriggerEnterFromLeft;
    public UnityEvent onTriggerEnterFromRight;
    public UnityEvent onTriggerExitToLeft;
    public UnityEvent onTriggerExitToRight;
    public UnityEvent onTriggerStay;

    private int isTriggerStayCounter = 0;


    void Start()
    {
        if (cameraBoundsWallCollider != null)
        {
            wallCollider = cameraBoundsWallCollider.GetComponent<Collider2D>();
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
        yield return new WaitForSeconds(0.01f); // could add some delay
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


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!string.IsNullOrEmpty(collisionTag) && !collision.CompareTag(collisionTag))
            return;

        isTriggerStayCounter++;

        if (isTriggerStayCounter >= 500)
        {
            onTriggerStay?.Invoke();
            isTriggerStayCounter = 0;

            if (wallCollider != null)
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
    }

}