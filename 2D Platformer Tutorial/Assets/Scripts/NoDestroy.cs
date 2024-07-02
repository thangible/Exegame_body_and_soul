using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Script for menu canvas
public class NoDestroy : MonoBehaviour
{
    private static NoDestroy _instance;
    public static NoDestroy Instance { get { return _instance; } }

    private static int _selectedLevel = 1;

    public Text loadLevelText;
    public Text levelTimeText;


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
    }


   

    void Start()
    {
         DontDestroyOnLoad(gameObject);
    }

    public void PlayGame(int index)
    {
        SceneManager.LoadSceneAsync(index);
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
        SceneManager.LoadSceneAsync(_selectedLevel);
    }

    public void LoadCheckpointForSelectedLevel()
    {
        StartCoroutine(LoadCheckpointCoroutine());
    }

    private IEnumerator LoadCheckpointCoroutine()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_selectedLevel);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        SaveLoad saveLoad = FindObjectOfType<SaveLoad>();
        if (saveLoad != null)
        {
            saveLoad.Load(_selectedLevel);
        }
    }


    public void QuitGame()
    {
        Application.Quit();
    }

}
