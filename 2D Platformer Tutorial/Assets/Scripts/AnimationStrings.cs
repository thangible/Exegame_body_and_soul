using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStrings : MonoBehaviour
{
    // Player
    internal static string isMoving = "isMoving";
    internal static string isRunning = "isRunning";
    internal static string isDashing = "isDashing";

    internal static string isGrounded = "isGrounded";
    internal static string yVelocity = "yVelocity";
    internal static string jump = "jump";
    internal static string isOnWall = "isOnWall";
    internal static string isOnCeiling = "isOnCeiling";

    internal static string fallThroughPlatform = "fallThroughPlatform";


    // Flying Boss
    internal static string flyingBoss_awaken = "awaken";
    internal static string flyingBoss_hasAwakened = "hasAwakened";

    internal static string flyingBoss_quickRangedAttack = "quickRangedAttack"; // no animation --> sub-state machine
    // quickRangedAttackForwardMovement is the default beginning sub-state
    internal static string flyingBoss_quickRangedAttackReturnMovement = "quickRangedAttackReturnMovement";
    internal static string flyingBoss_quickRangedAttackIdleMovement = "quickRangedAttackIdleMovement";

    internal static string flyingBoss_attackPlayer = "attackPlayer"; // no animation --> sub-state machine
    // attackPlayerForwardMovement is the default beginning sub-state
    internal static string flyingBoss_attackPlayerDashMovement = "attackPlayerDashMovement";
    internal static string flyingBoss_attackPlayerReturnMovement = "attackPlayerReturnMovement";

    internal static string flyingBoss_hitPlayerInProximity = "hitPlayerInProximity"; // no animation --> sub-state machine
    // hitPlayerInProximityDashMovement is the default beginning sub-state
    internal static string flyingBoss_hitPlayerInProximityReturnMovement = "hitPlayerInProximityReturnMovement";

    internal static string flyingBoss_slowRangedAttack = "slowRangedAttack";
}