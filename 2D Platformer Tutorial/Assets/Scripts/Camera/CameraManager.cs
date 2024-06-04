using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// Switching between multiple cameras (https://www.youtube.com/watch?v=wmTCWMcjIzo) --> place on main camera
public class CameraManager : MonoBehaviour
{
    //public static CameraManager instance;

    static List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>();

    public static CinemachineVirtualCamera ActiveCamera = null;


    public static bool IsActiveCamera(CinemachineVirtualCamera camera)
    {
        return camera == ActiveCamera;
    }

    public static void SwitchCamera(CinemachineVirtualCamera newCamera)
    {
        newCamera.Priority = 10;
        ActiveCamera = newCamera;

        foreach (CinemachineVirtualCamera cam in cameras)
        {
            if (cam != newCamera)
            {
                cam.Priority = 0;
            }
        }
    }


    public static void SwitchActiveCameraConfiner(Collider2D newCollider)
    {
        if (newCollider == null)
        {
            return;
        }

        CinemachineVirtualCamera vcam = null;
        int highestPriority = int.MinValue;

        // get highest prio cam
        foreach (var cam in cameras)
        {
            if (cam.Priority > highestPriority)
            {
                vcam = cam;
                highestPriority = cam.Priority;
            }
        }

        // change bounds
        if (vcam != null)
        {
            var confiner = vcam.GetComponent<CinemachineConfiner2D>();
            if (confiner != null)
            {
                confiner.m_BoundingShape2D = newCollider;
            }
        }
    }

    public static void Register(CinemachineVirtualCamera camera)
    {
        cameras.Add(camera);
    }

    public static void Unregister(CinemachineVirtualCamera camera)
    {
        cameras.Remove(camera);
    }
}