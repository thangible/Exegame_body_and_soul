using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Animator[] pauseMenuButtonAnimators;

    private bool isPaused = false;


    void Start()
    {
        pauseMenuUI.SetActive(false);
        SetAnimatorUpdateMode(AnimatorUpdateMode.Normal);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Toggle pause menu
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void Pause()
    {
        print("PAUSE");
        MusicManager.instance.LowerMusicVolume(1f);
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        SetAnimatorUpdateMode(AnimatorUpdateMode.UnscaledTime);
        isPaused = true;
    }

    public void Resume()
    {
        print("RES");
        MusicManager.instance.RaiseMusicVolume(1f);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        SetAnimatorUpdateMode(AnimatorUpdateMode.Normal);
        isPaused = false;
    }

    private void SetAnimatorUpdateMode(AnimatorUpdateMode mode)
    {
        foreach (Animator animator in pauseMenuButtonAnimators)
        {
            animator.updateMode = mode;
        }
    }
}