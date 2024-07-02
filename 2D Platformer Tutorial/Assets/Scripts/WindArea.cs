using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public enum WindDirection
{
    Up,
    Down
}

public class WindArea : MonoBehaviour
{
    public Vector2 windForce = new Vector2(0.0f, 1.0f);
    public float initialImpactTime = 1.0f;
    public WindDirection windDirection = WindDirection.Up;
    public float minYVelocity = 0.5f;

    public float minAdditionalForce = 1.0f;
    public float maxAdditionalForce = 2.0f;
    public float minAdditionalForceDuration = 2.1f;
    public float maxAdditionalForceDuration = 4f;
    public float additionalForceChance = 0.01f;
    public float additionalForceCooldown = 4.0f;
    public int maxTimesAdditionalForce = 5;
    private int timesAdditionalForceCounter = 0;

    public string collisionTag = "Player";

    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;

    private bool playerInZone = false;
    private float windEffectTimer = 0.0f;
    private bool minYVelocityReached = false;
    private bool additionalForceCooldownReady = false;
    float colliderYEnd = 0f;

    private Coroutine currentCoroutine;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerInZone)
            return;

        if (!string.IsNullOrEmpty(collisionTag) && !collision.CompareTag(collisionTag))
            return;

        playerInZone = true;
        colliderYEnd = GetColliderYEnd();

        onTriggerEnter?.Invoke();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!playerInZone)
            return;

        if (!string.IsNullOrEmpty(collisionTag) && !collision.CompareTag(collisionTag))
            return;


        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

        if (rb != null)
        {

            if (RespawnController.instance.HasRecentlyDied())
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;

                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                }
            }


            float targetVelocityY = (windDirection == WindDirection.Up) ? Mathf.Min(rb.velocity.y + windForce.y, minYVelocity) :
                                                                         Mathf.Max(rb.velocity.y - windForce.y, -minYVelocity);

            // apply wind force gradually over time
            if (windEffectTimer < initialImpactTime)
            {
                float interpolationFactor = windEffectTimer / initialImpactTime;

                float newVelocityY = Mathf.Lerp(rb.velocity.y, targetVelocityY, interpolationFactor);
                rb.velocity = new Vector2(rb.velocity.x, newVelocityY);

                windEffectTimer += Time.deltaTime;
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, targetVelocityY);
            }

            // start additional force cooldown
            if (!minYVelocityReached && rb.velocity.y == minYVelocity)
            {
                minYVelocityReached = true;
                additionalForceCooldownReady = true;
            }


            // add additional force to simulate wind
            if (additionalForceCooldownReady && Random.value < additionalForceChance && maxTimesAdditionalForce > timesAdditionalForceCounter)
            {
                if (Mathf.Abs(collision.transform.position.y - colliderYEnd) > 5f) // dont apply a force near the end of the wind zone
                {
                    ApplyAdditionalForce(rb);
                    StartCoroutine(AdditionalForceCooldown());
                    timesAdditionalForceCounter += 1;
                }
            }
        }
    }


    private float GetColliderYEnd()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.bounds.min.y;
        }
        return 0f;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!playerInZone)
            return;

        if (!string.IsNullOrEmpty(collisionTag) && !collision.CompareTag(collisionTag))
            return;

        playerInZone = false;

        onTriggerExit?.Invoke();
    }


    private void ApplyAdditionalForce(Rigidbody2D rb)
    {
        float additionalForceMagnitude = BiasRandom(minAdditionalForce, maxAdditionalForce, 1.5f);
        float duration = Random.Range(minAdditionalForceDuration, maxAdditionalForceDuration);

        Vector2 additionalForce = windForce * additionalForceMagnitude * 10;

        Debug.Log("Additional force applied: " + additionalForceMagnitude + "for duration of " + duration); // TODO remove ?


        currentCoroutine = StartCoroutine(AddForceOverTime(rb, additionalForce, duration));
    }

    private float BiasRandom(float min, float max, float biasPower)
    {
        float minPow = Mathf.Pow(min, biasPower);
        float maxPow = Mathf.Pow(max, biasPower);
        float randomPow = Random.Range(minPow, maxPow);
        return Mathf.Pow(randomPow, 1 / biasPower);
    }

    private IEnumerator AddForceOverTime(Rigidbody2D rb, Vector2 force, float duration)
    {
        float originalMass = rb.mass;
        float originalGravityScale = rb.gravityScale;

        float reducedMass = originalMass / 5;
        float reducedGravityScale = originalGravityScale / 5;

        float transitionDuration = 0.5f;
        float rampUpDuration = 1f;
        float rampDownDuration = 1f;
        float constantForceDuration = duration - rampUpDuration - rampDownDuration;


        // smoothly lower mass and gravity scale
        yield return StartCoroutine(SmoothTransition(rb, originalMass, reducedMass, originalGravityScale, reducedGravityScale, transitionDuration));

        float elapsedTime = 0.0f;

        // ramp-up phase
        while (elapsedTime < rampUpDuration)
        {
            if (RespawnController.instance.HasRecentlyDied()) break;

            float t = elapsedTime / rampUpDuration;
            Vector2 currentForce = Vector2.Lerp(Vector2.zero, force, t);
            rb.AddForce(currentForce * Time.fixedDeltaTime, ForceMode2D.Force);

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // constant force phase
        elapsedTime = 0.0f;
        while (elapsedTime < constantForceDuration)
        {
            if (RespawnController.instance.HasRecentlyDied()) break;

            rb.AddForce(force * Time.fixedDeltaTime, ForceMode2D.Force);

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // ramp-down phase
        elapsedTime = 0.0f;
        while (elapsedTime < rampDownDuration)
        {
            if (RespawnController.instance.HasRecentlyDied()) break;

            float t = elapsedTime / rampDownDuration;
            Vector2 currentForce = Vector2.Lerp(force, Vector2.zero, t);
            rb.AddForce(currentForce * Time.fixedDeltaTime, ForceMode2D.Force);

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // smoothly restore mass and gravity scale
        yield return StartCoroutine(SmoothTransition(rb, reducedMass, originalMass, reducedGravityScale, originalGravityScale, transitionDuration));
    }

    private IEnumerator SmoothTransition(Rigidbody2D rb, float startMass, float endMass, float startGravityScale, float endGravityScale, float transitionDuration)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < transitionDuration)
        {
            if (RespawnController.instance.HasRecentlyDied()) break;

            float t = elapsedTime / transitionDuration;
            rb.mass = Mathf.Lerp(startMass, endMass, t);
            rb.gravityScale = Mathf.Lerp(startGravityScale, endGravityScale, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rb.mass = endMass;
        rb.gravityScale = endGravityScale;
    }

    private IEnumerator AdditionalForceCooldown()
    {
        additionalForceCooldownReady = false;
        yield return new WaitForSeconds(additionalForceCooldown);
        additionalForceCooldownReady = true;
    }

}
