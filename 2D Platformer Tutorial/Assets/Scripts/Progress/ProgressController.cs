using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;
using static UnityEngine.Rendering.DebugUI;

// control milestones in the level progress --> put on main camera
public class ProgressController : MonoBehaviour
{
    public static ProgressController instance;

    public float levelTimer = 0f;
    public float finalLevelTime;

    public Text currentLevelTimeText;
    public Text finalLevelTimeText;


    // Level 1
    public bool hasPickedUpAttack = false;


    // Level 2
    public bool hasSolvedPuzzle = false;

    public bool hasOvercomeFirstFallingPlatforms = false;
    public bool hasOvercomeLastFallingPlatforms = false;

    public bool hasDefeatedFlyingEnemy = false;



    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        levelTimer += Time.deltaTime;
        UpdateCurrentLevelTime();
    }


    public float GetCurrentLevelTime()
    {
        return levelTimer;
    }

    public void SetCurrentLevelTime(float newTime)
    {
        levelTimer = newTime;
    }

    public void SetHasPickedUpAttack()
    {
        hasPickedUpAttack = true;
    }

    public bool HasPickedUpAttack()
    {
        return hasPickedUpAttack;
    }

    public void SetHasSolvedPuzzle()
    {
        hasSolvedPuzzle = true;
    }

    public bool HasSolvedPuzzle()
    {
        return hasSolvedPuzzle;
    }

    public void SetOvercameFirstFallingFlatforms()
    {
        hasOvercomeFirstFallingPlatforms = true;
    }

    public bool HasOvercomeFirstFallingPlatforms()
    {
        return hasOvercomeFirstFallingPlatforms;
    }


    public void SetFlyingEnemyDefeated()
    {
        hasDefeatedFlyingEnemy = true;

        // on boss defeat, destroy remaining slow projectiles
        SlowProjectileScript.DestroyAllProjectiles();
    }

    public bool HasDefeatedFlyingEnemy()
    {
        return hasDefeatedFlyingEnemy;
    }


    public void SetOvercameLastFallingFlatforms()
    {
        hasOvercomeLastFallingPlatforms = true;
    }

    public bool HasOvercomeLastFallingPlatforms()
    {
        return hasOvercomeLastFallingPlatforms;
    }

    private void UpdateCurrentLevelTime()
    {
        if (currentLevelTimeText != null)
        {
            currentLevelTimeText.text = "Time: " + levelTimer.ToString("F2") + "s";
        }
    }

    private void UpdateFinalLevelTime()
    {
        if (finalLevelTimeText != null)
        {
            finalLevelTimeText.text = "Time: " + finalLevelTime.ToString("F2") + "s";
        }
    }

    
    // unnecessary, just to be formal
    public void ResetProgress(int level)
    {
        finalLevelTime = levelTimer;
        UpdateFinalLevelTime();

        /*
        if (level == 1)
        {
            hasPickedUpAttack = false;
        }

        if (level == 2)
        {
            hasSolvedPuzzle = false;
            hasOvercomeFirstFallingPlatforms = false;
            hasOvercomeLastFallingPlatforms = false;
            hasDefeatedFlyingEnemy = false;
        }*/
    }

    public void PlayAgain()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex, LoadSceneMode.Single);
    }


    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }

}
