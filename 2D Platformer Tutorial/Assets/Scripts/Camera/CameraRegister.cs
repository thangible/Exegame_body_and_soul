using Cinemachine;
using UnityEngine;

public class CameraRegister : MonoBehaviour
{
    private void OnEnable()
    {
        CameraManager.Register(GetComponent<CinemachineVirtualCamera>());
    }
    private void OnDisable()
    {
        CameraManager.Unregister(GetComponent<CinemachineVirtualCamera>());
    }
}