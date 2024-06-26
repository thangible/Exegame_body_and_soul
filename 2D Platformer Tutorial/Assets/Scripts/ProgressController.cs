using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// control milestones in the level progress --> put on main camera
public class ProgressController : MonoBehaviour
{
    public static ProgressController instance;


    // Level 2
    public bool hasSolvedPuzzle = false;

    public bool hasOvercomeFirstFallingPlatforms = false;
    public bool hasOvercomeLastFallingPlatforms = false;

    public bool hasDefeatedFlyingEnemy = false;


    private void Awake()
    {
        instance = this;
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

}
