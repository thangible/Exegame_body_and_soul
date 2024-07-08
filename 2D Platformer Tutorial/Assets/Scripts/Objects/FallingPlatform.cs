using System.Collections;
using UnityEngine;

// Falling platform based on https://www.youtube.com/watch?v=k70z88Xivzs&ab_channel=Raycastly
public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float fallDelay = 1f;
    [SerializeField] private float destroyDelay = 2f;

    private bool falling = false;

    [SerializeField] private Rigidbody2D rb_fallingPlatform;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Avoid calling the coroutine multiple times if it's already been called (falling)
        if (falling)
            return;

        // If the player landed on the platform, start falling
        if (collision.transform.tag == "Player")
        {
            StartCoroutine(StartFall());
        }
    }

    private IEnumerator StartFall()
    {
        falling = true;

        // Wait for a few seconds before dropping
        yield return new WaitForSeconds(fallDelay);

        // Enable rigidbody and destroy after a few seconds
        rb_fallingPlatform.bodyType = RigidbodyType2D.Dynamic;
        Destroy(gameObject, destroyDelay);
    }
}