using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public Button[] buttons;


    public void Awake()
    {
        // unlocking levels in the menu (https://www.youtube.com/watch?v=2XQsKNHk1vk)
        int unlockedLevelID = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for  (int i = 0; i < buttons.Length; i++) 
        {
            buttons[i].interactable = false;
        }
        for (int i = 0; i < unlockedLevelID; i++)
        {
            buttons[i].interactable = true;
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadLevel(int levelID)
    {
        string levelName = "Level" + levelID;
        SceneManager.LoadScene(levelName);
    }
}
