using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// control milestones in the level progress --> put on main camera
public class ProgressController : MonoBehaviour
{
    public static ProgressController instance;


    // Level 3
    public bool hasDefeatedFlyingEnemy = false;
    public bool hasOvercomeFallingPlatforms = false;


    private void Awake()
    {
        instance = this;
    }


    public void SetFlyingEnemyDefeated()
    {
        hasDefeatedFlyingEnemy = true;
    }

    public bool HasDefeatedFlyingEnemy()
    {
        return hasDefeatedFlyingEnemy;
    }


    public void SetOvercameFallingFlatforms()
    {
        hasOvercomeFallingPlatforms = true;
    }

    public bool HasOvercomeFallingPlatforms()
    {
        return hasOvercomeFallingPlatforms;
    }

}
