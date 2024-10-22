using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;
using System.Runtime.ExceptionServices;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.Audio;
using UnityEngine.UIElements;


// Script for menu canvas
public class NoDestroy : MonoBehaviour
{
    private static NoDestroy _instance;
    public static NoDestroy Instance { get { return _instance; } }

    private static int _selectedLevel = 1;

    public GameObject firstMenu;

    public GameObject kinectButtonActive;
    public GameObject kinectButtonInactive;
    public GameObject keyboardButtonActive;
    public GameObject keyboardButtonInactive;

    public AudioMixer audioMixer;
    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Slider sfxSlider;

    public GameObject transitionsContainer;

    public Text loadLevelText;
    public Text levelTimeText;


    [Header("Time Scores")]
    public Text levelFirstTimeText;
    public Text levelSecondTimeText;
    public Text levelThirdTimeText;

    public GameObject historyScrollViewContent;
    public GameObject historyItemPrefab;


    private void Awake()
    {
     
        if (_instance != null)
        {
            Destroy(this.gameObject); return;
        }
        else
        {
            _instance = this;
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        LoadVolume();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            firstMenu.SetActive(true);
            MusicManager.instance.PlayMusic("MainMenu", fadeDuration: 1f);
        }
    }







    public void PlayGame(int index)
    {
        //SceneManager.LoadSceneAsync(index);

        transitionsContainer.SetActive(true);

        SceneManager.LoadSceneAsync(index).completed += (AsyncOperation asyncOperation) =>
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Settings
                UpdateSettings(player);
            }

            transitionsContainer.SetActive(false);

            PlayGameMusic(index);
        };
        
    }


    public void SelectLevel(int level)
    {
        _selectedLevel = level;
        UpdateLevelText();
        UpdateLevelTimeText();
    }

    public void PlaySelectedLevel()
    {
        //SceneManager.LoadSceneAsync(_selectedLevel);

        transitionsContainer.SetActive(true);
        
        SceneManager.LoadSceneAsync(_selectedLevel).completed += (AsyncOperation asyncOperation) =>
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Settings
                UpdateSettings(player);
            }

            transitionsContainer.SetActive(false);

            PlayGameMusic(_selectedLevel);
        };
        
    }



    public void LoadCheckpointForSelectedLevel()
    {
        StartCoroutine(LoadCheckpointCoroutine());
    }
    private IEnumerator LoadCheckpointCoroutine()
    {

        int isFinished = PlayerPrefs.GetInt("isFinished_" + _selectedLevel);

        if (isFinished == 0)
        {
            /*
            LevelManager.Instance.LoadScene(_selectedLevel);
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Settings
                UpdateSettings(player);
            }

            PlayGameMusic(_selectedLevel);
            */


            transitionsContainer.SetActive(true);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_selectedLevel);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            LoadLevel(_selectedLevel, player);

            // Settings
            UpdateSettings(player);

            transitionsContainer.SetActive(false);

            PlayGameMusic(_selectedLevel);
        }
        else
        {
            PlaySelectedLevel();
        }
    }



    private void UpdateLevelText()
    {
        if (loadLevelText != null)
        {
            loadLevelText.text = "Level " + _selectedLevel;
        }
    }

    private void UpdateLevelTimeText()
    {
        if (levelTimeText != null)
        {
            float levelTime = PlayerPrefs.GetFloat("levelTime_" + _selectedLevel, 0f);
            levelTimeText.text = "Current time: " + levelTime.ToString("F2") + "s";
        }
    }




    private void PlayGameMusic(int sceneIndex)
    {
        if (sceneIndex == 1)
        {

            MusicManager.instance.PlayMusic("Level1");
        }
        else if (sceneIndex == 2)
        {
            if (!ProgressController.instance.hasDefeatedFlyingEnemy)
            {
                MusicManager.instance.PlayMusic("Level2");
            }
            else
            {
                MusicManager.instance.PlayMusic("Level2_PostBoss", fadeDuration: 1f);
            }
        }
    }



 


    public void UpdateSettings(GameObject player)
    {
        int inputMethod = PlayerPrefs.GetInt("input", -1);
        PlayerInput playerInput = player.GetComponent<PlayerInput>();

        if (inputMethod == -1 || inputMethod == 0)
        {
            playerInput.enabled = true;
        }
        else if (inputMethod == 1)
        {
            playerInput.enabled = false;
        }
    }



    public void LoadLevel(int level, GameObject player)
    {
        if (player != null && player.transform != null)
        {
            player.SetActive(false);

            // Respawn point
            float x = PlayerPrefs.GetFloat("x_" + level);
            float y = PlayerPrefs.GetFloat("y_" + level);
            float z = PlayerPrefs.GetFloat("z_" + level);

            Vector3 position = new Vector3(x, y, z);
            player.transform.position = position;


            // Settings
            UpdateSettings(player);


            // Camera
            string cameraName = PlayerPrefs.GetString("cameraName_" + level, null);
            if (cameraName != null)
            {
                CameraManager.SetActiveCamera(cameraName);
            }
            print("LOAD nd");
            print(cameraName);


            // Progress
            float currentLevelTime = PlayerPrefs.GetFloat("levelTime_" + level);
            ProgressController.instance.SetCurrentLevelTime(currentLevelTime);


            if (level == 1)
            {
                if (PlayerPrefs.GetString("hasPickedUpAttack") == "true")
                {

                    ProgressController.instance.SetHasPickedUpAttack();
                }
            }

            if (level == 2)
            {
                ProgressController.instance.SetHasPickedUpAttack();

                if (PlayerPrefs.GetString("hasSolvedPuzzle") == "true")
                {

                    ProgressController.instance.HasSolvedPuzzle();
                }

                if (PlayerPrefs.GetString("hasOvercomeFirstFallingPlatforms") == "true")
                {

                    ProgressController.instance.HasOvercomeFirstFallingPlatforms();
                }

                if (PlayerPrefs.GetString("hasDefeatedFlyingEnemy") == "true")
                {

                    ProgressController.instance.HasDefeatedFlyingEnemy();
                }

                if (PlayerPrefs.GetString("hasOvercomeLastFallingPlatforms") == "true")
                {

                    ProgressController.instance.HasOvercomeLastFallingPlatforms();
                }
            }

            player.SetActive(true);
        }
    }



    public void PlayClickSound()
    {
        SoundManager.instance.PlaySound2D("Click");
    }


    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void UpdateSoundEffectVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }

    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);

        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }



    public void SwitchControls(int inputMethod)
    {
        PlayerPrefs.SetInt("input", inputMethod);
        PlayerPrefs.Save();
    }

    public void ActivateControlsButtons()
    {
        if (PlayerPrefs.GetInt("input", -1) == 0) // keyboard
        {
            kinectButtonActive.SetActive(false);
            kinectButtonInactive.SetActive(true);
            keyboardButtonActive.SetActive(true);
            keyboardButtonInactive.SetActive(false);
        } 
        else if (PlayerPrefs.GetInt("input", -1) == 1) // kinect
        {
            kinectButtonActive.SetActive(true);
            kinectButtonInactive.SetActive(false);
            keyboardButtonActive.SetActive(false);
            keyboardButtonInactive.SetActive(true);
        }
    }


    public void DeleteAllScores()
    {
        PlayerPrefs.DeleteKey("levelTime1_" + _selectedLevel);
        PlayerPrefs.DeleteKey("levelTime2_" + _selectedLevel);
        PlayerPrefs.DeleteKey("levelTime3_" + _selectedLevel);
        PlayerPrefs.SetString("levelTimeHistory_" + _selectedLevel, "");

        PlayerPrefs.Save();
    }



    public void PopulateHighScoresScrollView()
    {
        float time_1 = PlayerPrefs.GetFloat("levelTime1_" + _selectedLevel, -1);
        float time_2 = PlayerPrefs.GetFloat("levelTime2_" + _selectedLevel, -1);
        float time_3 = PlayerPrefs.GetFloat("levelTime3_" + _selectedLevel, -1);

        if (time_1 >= 0 && levelFirstTimeText != null)
        {
            levelFirstTimeText.text = "1st: " + time_1.ToString("F2");
        } 
        else if (time_1 < 0 && levelFirstTimeText != null)
        {
            levelFirstTimeText.text = "1st: 0,00";
        }

        if (time_2 >= 0 && levelSecondTimeText != null)
        {
            levelSecondTimeText.text = "2nd: " + time_2.ToString("F2");
        }
        else if (time_2 < 0 && levelSecondTimeText != null)
        {
            levelSecondTimeText.text = "2nd: 0,00";
        }

        if (time_3 >= 0 && levelThirdTimeText != null)
        {
            levelThirdTimeText.text = "3rd: " + time_3.ToString("F2");
        }
        else if (time_3 < 0 && levelThirdTimeText != null)
        {
            levelThirdTimeText.text = "3rd: 0,00";
        }
    }

    public void PopulateHistoryScrollView()
    {
        foreach (Transform child in historyScrollViewContent.transform)
        {
            Destroy(child.gameObject);
        }

        string existingLevelTimesString = PlayerPrefs.GetString("levelTimeHistory_" + _selectedLevel);
        print(existingLevelTimesString);
        print("");
        if (!string.IsNullOrEmpty(existingLevelTimesString))
        {
            string[] entries = existingLevelTimesString.Split(new string[] { "];[" }, StringSplitOptions.RemoveEmptyEntries);
            print(entries);

            foreach (string entry in entries)
            {
                string cleanEntry = entry.Trim('[', ']');
                string[] parts = cleanEntry.Split(';');

                if (parts.Length >= 3)
                {
                    string date = parts[0].Trim();
                    string time = parts[1].Trim();
                    string finalTime = parts[2].Trim();

                    string displayText = "Date: " + date + ", " + time + " | Time: " + finalTime;

                    GameObject historyItemObj = Instantiate(historyItemPrefab, historyScrollViewContent.transform);
                    Text textComponent = historyItemObj.GetComponentInChildren<Text>();
                    textComponent.text = displayText;
                }
            }
        }
    }


    public void QuitGame()
    {
        Application.Quit();
    }


}
