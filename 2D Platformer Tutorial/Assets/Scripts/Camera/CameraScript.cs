using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    public SpriteRenderer player;
    public float camScaler = 1;

    private CinemachineVirtualCamera vcam;

    // Start is called before the first frame update
    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();

        if (vcam == null)
        {
            var camera = Camera.main;
            var brain = (camera == null) ? null : camera.GetComponent<CinemachineBrain>();
            vcam = (brain == null) ? null : brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        }

        adjustOrthoSize();
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Automatically adapt orthographic size with respect to the player
    public void adjustOrthoSize()
    {
        // this still assuming that the targetRadio is the whole level and not just the player
        //float screenRatio = (float)Screen.width / (float)Screen.height;
        //float targetRatio = player.bounds.size.x / player.bounds.size.y;
        /*
        if (screenRatio >= targetRatio)
        {
            vcam.m_Lens.OrthographicSize = player.bounds.size.y / 2; // Camera.main.orthographicSize with normal camera
        } else
        {
            float differenceInSize = targetRatio / screenRatio;
            vcam.m_Lens.OrthographicSize = player.bounds.size.y / 2 * differenceInSize;
        }
        */


        // another method (https://www.youtube.com/watch?v=gFWQHordrtA)
        var horizontal = player.bounds.size.x * (float)Screen.height / (float)Screen.width; // camera.pixelHeight / camera.pixelWidth;
        var vertical = player.bounds.size.y;
        var size = (Mathf.Max(horizontal, vertical) * 0.5f) * camScaler;
        vcam.m_Lens.OrthographicSize = size;
    }


    public void adjustOrthoSize(float camScaler)
    {
        // another method (https://www.youtube.com/watch?v=gFWQHordrtA)
        var horizontal = player.bounds.size.x * (float)Screen.height / (float)Screen.width;
        var vertical = player.bounds.size.y;
        var size = (Mathf.Max(horizontal, vertical) * 0.5f) * camScaler;
        vcam.m_Lens.OrthographicSize = size;
    }

}
