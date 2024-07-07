using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Animator[] pauseMenuButtonAnimators;
    public AudioMixer audioMixer;

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
        audioMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume") / 2);
        audioMixer.SetFloat("SFXVolume", PlayerPrefs.GetFloat("SFXVolume") / 2);
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        SetAnimatorUpdateMode(AnimatorUpdateMode.UnscaledTime);
        isPaused = true;
    }

    public void Resume()
    {
        audioMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume"));
        audioMixer.SetFloat("SFXVolume", PlayerPrefs.GetFloat("SFXVolume"));
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
