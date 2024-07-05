using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Script for shooting lasers based on https://www.youtube.com/watch?v=vdci2oxVaoA&ab_channel=1MinuteUnity
public class LaserShooter : MonoBehaviour
{
    public Transform laserPosition;
    public GameObject laser;

    public GameObject hitEffect;
    public GameObject intersectionEffect;

    public LayerMask groundLayer;

    public float rotationStartAngle = 0f;
    public float rotationEndAngle = 45f;
    private float maxLaserDistance = 150f;

    public float rotationDuration = 5.0f;
    public float rotationStartDelay = 0.0f;
    public bool rotateClockwise = true;

    private LineRenderer lineRenderer;
    private BoxCollider2D laserCollider;
    private float colliderWidth = 0.3f;
    private Vector3 colliderOffset;

    private bool intersectionFound = false;
    private Vector2 intersectionPoint = Vector2.zero;

    private float timer = 0f;


    public void Awake()
    {
        lineRenderer = laser.GetComponent<LineRenderer>();
        CreateLaserCollider();
        SetLaserGradient();
    }

    private void Update()
    {
        if (timer < rotationStartDelay)
        {
            timer += Time.deltaTime;
            return;
        }

        intersectionFound = false;
        intersectionPoint = Vector2.zero;

        RotateObject();
        ShootLaser();
    }


    void RotateObject()
    {
        float timeSinceStart = Mathf.Max(Time.time - rotationStartDelay, 0f);
        float t = Mathf.PingPong(timeSinceStart / rotationDuration, 1.0f);
        float targetAngle;

        if (rotateClockwise)
        {
            targetAngle = Mathf.Lerp(rotationStartAngle, rotationEndAngle, t);
        }
        else
        {
            targetAngle = Mathf.Lerp(rotationEndAngle, rotationStartAngle, t);
        }

        Vector3 newRotation = transform.eulerAngles;
        newRotation.z = targetAngle;
        transform.eulerAngles = newRotation;

        if (t == 1.0f)
        {
            rotateClockwise = !rotateClockwise;
        }
    }

    void ShootLaser()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(laserPosition.position, laserPosition.up, maxLaserDistance);
        Vector2 endPosition = (Vector2)laserPosition.position + (Vector2)(laserPosition.up * maxLaserDistance);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject != gameObject && hit.collider.gameObject != laser)
            {
                // hit anything
                endPosition = hit.point;


                if ((groundLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
                {
                    // hit ground layer     
                    //Debug.Log("Hit groundLayer: " + hit.collider.gameObject.name);

                    break; // only break --> find a valid hit if its an object from the ground layer
                }

                if (hit.collider.CompareTag("Player"))
                {
                    // hit player
                    RespawnController.instance.AnnouncePlayerDeath();
                }

                if (hit.collider.CompareTag("Laser"))
                {
                    // hit another laser, calculate intersection point
                    LineRenderer otherLaserLineRenderer = hit.collider.GetComponent<LineRenderer>();
                    if (otherLaserLineRenderer != null)
                    {
                        Vector2 intersection = FindIntersection(
                            laserPosition.position, laserPosition.position + laserPosition.up * maxLaserDistance,
                            otherLaserLineRenderer.GetPosition(0), otherLaserLineRenderer.GetPosition(1)
                        );

                        if (intersection != Vector2.zero)
                        {
                            //hitPosition = intersection;
                            intersectionPoint = intersection;
                            intersectionFound = true;

                            // break;
                        }
                    }

                    // skip the hit point and continue to the next one (because it was just another laser and we do not want to stop here)
                    continue;
                }

                //break;
                continue;
            }
        }

        Draw2DRay(laserPosition.position, endPosition);
        UpdateLaserCollider();
    }

    void Draw2DRay(Vector2 startPosition, Vector2 endPosition)
    {
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);

        hitEffect.transform.position = endPosition;

        if (intersectionFound)
        {
            intersectionEffect.transform.position = intersectionPoint;
            intersectionEffect.SetActive(true);
        } 
        else
        {
            intersectionEffect.SetActive(false);
        }
    }

    
    void SetLaserGradient()
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[3];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];

        // color from 0% to 1% (doesnt work, takes up way more space which still looks good)
        colorKeys[0].color = new Color32(179, 110, 255, 255); // B36EFF
        colorKeys[0].time = 0.01f;

        // color from 1% to 90%
        colorKeys[1].color = new Color32(255, 210, 0, 255); // FFD200
        colorKeys[1].time = 0.90f;

        // color from 90% to 100%
        colorKeys[2].color = new Color32(255, 215, 108, 255); // FFD76C
        colorKeys[2].time = 1.0f;

        // alpha
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.01f;
        alphaKeys[1].alpha = 1.0f;
        alphaKeys[1].time = 0.90f;
        alphaKeys[2].alpha = 1.0f;
        alphaKeys[2].time = 1.0f;

        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }

    void CreateLaserCollider()
    {
        if (laserCollider == null)
        {
            // create collider (also adjust laser transform in unity)
            laserCollider = laser.AddComponent<BoxCollider2D>();
            laserCollider.isTrigger = true;

            Vector2 laserEndPosition = (Vector2)laserPosition.position + (Vector2)(laserPosition.up * maxLaserDistance / 1f);
            float colliderLength = Vector2.Distance(laserPosition.position, laserEndPosition);
            laserCollider.size = new Vector2(colliderLength, colliderWidth);

            // correct position
            colliderOffset = new Vector3(transform.position.x, transform.position.y, transform.position.z) / 7.75f;
            laserCollider.offset = colliderOffset;

            // correct rotation
            Vector2 direction = laserEndPosition - (Vector2)laserPosition.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 0f;
            laserCollider.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void UpdateLaserCollider()
    {
        if (laserCollider != null && lineRenderer != null)
        {
            Vector3 laserStart = lineRenderer.GetPosition(0);
            Vector3 laserEnd = lineRenderer.GetPosition(1);

            float colliderLength = Vector3.Distance(laserStart, laserEnd);
            laserCollider.size = new Vector2(colliderLength, colliderWidth);
            laserCollider.offset = new Vector3(colliderLength / 2, colliderOffset.y, colliderOffset.z);
        }
    }





    Vector2 FindIntersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        float a1 = p2.y - p1.y;
        float b1 = p1.x - p2.x;
        float c1 = a1 * p1.x + b1 * p1.y;

        float a2 = q2.y - q1.y;
        float b2 = q1.x - q2.x;
        float c2 = a2 * q1.x + b2 * q1.y;

        float delta = a1 * b2 - a2 * b1;

        if (Mathf.Abs(delta) < Mathf.Epsilon)
        {
            // Lines are parallel, no intersection
            return Vector2.zero;
        }

        float x = (b2 * c1 - b1 * c2) / delta;
        float y = (a1 * c2 - a2 * c1) / delta;

        Vector2 intersection = new Vector2(x, y);

        // Check if the intersection point is within both line segments
        if (IsPointOnLineSegment(p1, p2, intersection) && IsPointOnLineSegment(q1, q2, intersection))
        {
            return intersection;
        }

        return Vector2.zero;
    }

    bool IsPointOnLineSegment(Vector2 p1, Vector2 p2, Vector2 point)
    {
        return point.x >= Mathf.Min(p1.x, p2.x) && point.x <= Mathf.Max(p1.x, p2.x) &&
               point.y >= Mathf.Min(p1.y, p2.y) && point.y <= Mathf.Max(p1.y, p2.y);
    }

    private void OnDrawGizmos()
    {
        if (intersectionFound)
        {
            Gizmos.color = Color.red;
            //Gizmos.DrawSphere(intersectionPoint, 0.5f);
        }
    }

}
