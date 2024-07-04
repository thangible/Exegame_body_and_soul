using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePlatform : MonoBehaviour
{
    public GameObject platform1;
    public GameObject platform2;
    public List<GameObject> vanishWithPlatformOne;
    public List<GameObject> vanishWithPlatformTwo;

    public bool isPuzzlePlatformActivated = false;
    public float fadeDuration = 0.8f;
    private bool platform1Active = true;


    void Start()
    {
        ResetPlatforms();
    }

    public void ChangeActivated()
    {
        isPuzzlePlatformActivated = !isPuzzlePlatformActivated;
    }

    public void SwitchPlatforms()
    {
        if (isPuzzlePlatformActivated)
        {
            if (platform1Active)
            {
                StartCoroutine(Fade(platform1, fadeDuration, 0f, false));
                StartCoroutine(Fade(platform2, fadeDuration, 1f, true));

                foreach (GameObject gameObject in vanishWithPlatformOne)
                {
                    StartCoroutine(Fade(gameObject, fadeDuration, 0f, false));

                }
                foreach (GameObject gameObject in vanishWithPlatformTwo)
                {
                    StartCoroutine(Fade(gameObject, fadeDuration, 1f, true));

                }

                platform1Active = false;
            }
            else
            {
                StartCoroutine(Fade(platform2, fadeDuration, 0f, false));
                StartCoroutine(Fade(platform1, fadeDuration, 1f, true));

                foreach (GameObject gameObject in vanishWithPlatformOne)
                {
                    StartCoroutine(Fade(gameObject, fadeDuration, 1f, true));

                }
                foreach (GameObject gameObject in vanishWithPlatformTwo)
                {
                    StartCoroutine(Fade(gameObject, fadeDuration, 0f, false));

                }

                platform1Active = true;
            }
        }
    }

    private IEnumerator Fade(GameObject platform, float duration, float targetAlpha, bool activate)
    {
        SpriteRenderer spriteRenderer = platform.GetComponent<SpriteRenderer>();
        Collider2D collider = platform.GetComponent<Collider2D>();

        float startAlpha = spriteRenderer.color.a;
        float rate = 1.0f / duration;
        float progress = 0.0f;

        if (activate)
        {
            platform.SetActive(true);
            if (collider != null)
            {
                yield return new WaitForSeconds(0.2f);
                collider.enabled = true;
            }
        }

        while (progress < 1.0f)
        {
            Color tmpColor = spriteRenderer.color;
            spriteRenderer.color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, Mathf.Lerp(startAlpha, targetAlpha, progress));
            progress += rate * Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, targetAlpha);

        if (!activate)
        {
            platform.SetActive(false);
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }

    private void SetAlpha(GameObject platform, float alpha)
    {
        SpriteRenderer spriteRenderer = platform.GetComponent<SpriteRenderer>();
        Color tmpColor = spriteRenderer.color;
        spriteRenderer.color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, alpha);
    }

    public void ResetPlatforms()
    {
        platform1.SetActive(true);
        platform2.SetActive(false);
        SetAlpha(platform1, 1f);
        SetAlpha(platform2, 0f);
    }

}
