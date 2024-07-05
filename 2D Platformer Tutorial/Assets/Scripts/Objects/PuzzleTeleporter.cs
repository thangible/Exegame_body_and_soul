using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleTeleporter : MonoBehaviour
{
    public Transform targetDestination;
    public GameObject player;
    public bool isTeleporterExit = false;
    public bool isGoal = false;
    [SerializeField] private CinemachineVirtualCamera vcamTarget;

    private PuzzlePlatform[] puzzlePlatforms;
    private PlayerController playerController;


    public void Awake()
    {
        puzzlePlatforms = FindObjectsOfType<PuzzlePlatform>();

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

            other.transform.position = targetDestination.position;
            foreach (PuzzlePlatform puzzlePlatform in puzzlePlatforms)
            {
                puzzlePlatform.ChangeActivated();
            }

            if (!isTeleporterExit)
            {
                playerController.PrepareForPuzzleSection();

                if (vcamTarget != null)
                {
                    SwitchCameraAndStyle(vcamTarget, CinemachineBlendDefinition.Style.Cut);
                }
            }
            else
            {
                if (isGoal)
                {
                    ProgressController.instance.SetHasSolvedPuzzle();
                }
                playerController.ResetAfterPuzzleSection();

                if (vcamTarget != null)
                {
                    SwitchCameraAndStyle(vcamTarget, CinemachineBlendDefinition.Style.Cut);
                }
            }

            StartCoroutine(SwitchBackToDefaultCameraStyle(CinemachineBlendDefinition.Style.EaseInOut));
        }
    }

    private void SwitchCameraAndStyle(CinemachineVirtualCamera targetCamera, CinemachineBlendDefinition.Style cameraStyle)
    {
        CameraManager.SwitchCamera(targetCamera);

        CinemachineBrain cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_DefaultBlend.m_Style = cameraStyle;
        }
    }

    private IEnumerator SwitchBackToDefaultCameraStyle(CinemachineBlendDefinition.Style cameraStyle)
    {
        yield return new WaitForSeconds(0.5f);

        CinemachineBrain cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_DefaultBlend.m_Style = cameraStyle;
        }
    }
}
