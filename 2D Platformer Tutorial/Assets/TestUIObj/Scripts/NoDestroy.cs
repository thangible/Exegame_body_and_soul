using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
public class NoDestroy : MonoBehaviour
{
    private static NoDestroy _instance;
    public static NoDestroy Instance { get { return _instance; } }

   
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

    public void QuitGame()
    {
        Application.Quit();
    }

}
