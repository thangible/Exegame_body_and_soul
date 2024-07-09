using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Collections;
using UnityEditor.Rendering.LookDev;
using Unity.VisualScripting;

// Switching between multiple cameras (based on https://www.youtube.com/watch?v=wmTCWMcjIzo) --> place on main camera
public class CameraManager : MonoBehaviour
{
    //public static CameraManager instance;

    private static Dictionary<string, CinemachineVirtualCamera> cameras = new Dictionary<string, CinemachineVirtualCamera>();

    public static CinemachineVirtualCamera ActiveCamera = null;


    public static string GetActiveCamera()
    {
        foreach (var camera in cameras.Values)
        {
            if (camera.Priority == 10)
            {
                return camera.gameObject.name;
            }
        }

        return null;
    }

    public static void SetActiveCamera(string cameraName)
    {
        foreach (var pair in cameras)
        {
            CinemachineVirtualCamera cam = pair.Value;

            if (cam.gameObject.name == cameraName)
            {
                cam.Priority = 10;
                ActiveCamera = cam;
            }
            else
            {
                cam.Priority = 0;
            }
        }
    }


    public static bool IsActiveCamera(CinemachineVirtualCamera camera)
    {
        return camera == ActiveCamera;
    }

    public static void SwitchCamera(CinemachineVirtualCamera newCamera)
    {
        foreach (var cam in cameras.Values)
        {
            if (cam == newCamera)
            {
                cam.Priority = 10;
                ActiveCamera = newCamera;
            }
            else
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
        foreach (var cam in cameras.Values)
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

    public static void SwitchCameraStyle(CinemachineBlendDefinition.Style cameraStyle)
    {
        CinemachineBrain cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_DefaultBlend.m_Style = cameraStyle;
        }

        CoroutineRunner.Instance.StartCoroutine(SwitchBackToDefaultCameraStyle(CinemachineBlendDefinition.Style.EaseInOut));
    }

    private static IEnumerator SwitchBackToDefaultCameraStyle(CinemachineBlendDefinition.Style cameraStyle)
    {
        yield return new WaitForSeconds(0.5f);

        CinemachineBrain cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_DefaultBlend.m_Style = cameraStyle;
        }
    }



    public static void Register(CinemachineVirtualCamera camera)
    {
        cameras[camera.gameObject.name] = camera;
    }

    public static void Unregister(CinemachineVirtualCamera camera)
    {
        cameras.Remove(camera.gameObject.name);
    }
}