using Cinemachine;
using UnityEngine;

public class PuzzleTeleporter : MonoBehaviour
{
    public Transform targetDestination;
    public GameObject player;
    public bool isTeleporterExit = false;
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
                    CameraManager.SwitchCamera(vcamTarget);

                    CinemachineBrain cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
                    if (cinemachineBrain != null)
                    {
                        cinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
                    }
                }
            } 
            else
            {
                playerController.ResetAfterPuzzleSection();
                ProgressController.instance.SetHasSolvedPuzzle();

                if (vcamTarget != null)
                {
                    CameraManager.SwitchCamera(vcamTarget);

                    CinemachineBrain cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
                    if (cinemachineBrain != null)
                    {
                        cinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
                    }
                }
            }
        }
    }
}
