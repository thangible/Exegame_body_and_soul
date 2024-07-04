using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;
using System.Runtime.ExceptionServices;
using UnityEngine.InputSystem;

// Script for menu canvas
public class NoDestroy : MonoBehaviour
{
    private static NoDestroy _instance;
    public static NoDestroy Instance { get { return _instance; } }

    private static int _selectedLevel = 1;

    public GameObject firstMenu;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            firstMenu.SetActive(true);
        }
    }


    public void PlayGame(int index)
    {
        //SceneManager.LoadSceneAsync(index);
        SceneManager.LoadSceneAsync(index).completed += (AsyncOperation asyncOperation) =>
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Settings
                UpdateSettings(player);
            }
        };
    }


    public void SelectLevel(int level)
    {
        _selectedLevel = level;
        UpdateLevelText();
        UpdateLevelTimeText();
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

    public void PlaySelectedLevel()
    {
        //SceneManager.LoadSceneAsync(_selectedLevel);
        SceneManager.LoadSceneAsync(_selectedLevel).completed += (AsyncOperation asyncOperation) =>
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Settings
                UpdateSettings(player);
            }
        };
    }

    public void LoadCheckpointForSelectedLevel()
    {
        StartCoroutine(LoadCheckpointCoroutine());
    }
    private IEnumerator LoadCheckpointCoroutine()
    {

        int isFinished= PlayerPrefs.GetInt("isFinished_" + _selectedLevel);

        if (isFinished == 0)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_selectedLevel);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            LoadLevel(_selectedLevel, player);

            // Settings
            UpdateSettings(player);
        }
        else
        {
            PlaySelectedLevel();
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
            int cameraNumber = PlayerPrefs.GetInt("cameraNumber_" + level);
            CameraManager.SetActiveCamera(cameraNumber);


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




    public void SwitchControls(int inputMethod)
    {
        PlayerPrefs.SetInt("input", inputMethod);
        PlayerPrefs.Save();
    }

    public void DeleteAllScores()
    {
        PlayerPrefs.DeleteKey("levelTime1_" + _selectedLevel);
        PlayerPrefs.DeleteKey("levelTime2_" + _selectedLevel);
        PlayerPrefs.DeleteKey("levelTime3_" + _selectedLevel);
        PlayerPrefs.SetString("levelTimeHistory_" + _selectedLevel, "");

        PlayerPrefs.Save();
    }



    public void QuitGame()
    {
        Application.Quit();
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

}
