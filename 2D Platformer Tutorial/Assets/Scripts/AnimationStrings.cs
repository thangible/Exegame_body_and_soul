using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStrings : MonoBehaviour
{
    // Player

    internal static string isMoving = "isMoving";
    internal static string isRunning = "isRunning";
    internal static string isDashing = "isDashing";
    internal static string canMove = "canMove";

    internal static string isGrounded = "isGrounded";
    internal static string yVelocity = "yVelocity"; // for falling
    internal static string jump = "jump";
    internal static string isOnWall = "isOnWall";
    internal static string isOnCeiling = "isOnCeiling";

    internal static string attack = "attack";

    internal static string fallThroughPlatform = "fallThroughPlatform";



    // Flying Boss

    //
    internal static string flyingBoss_awaken = "awaken";
    internal static string flyingBoss_hasAwakened = "hasAwakened";

    //
    internal static string flyingBoss_moveWithoutAttackingPlayer = "moveWithoutAttackingPlayer"; // no animation --> sub-state machine
    // moveWithoutAttackingForwardMovement is the default beginning sub-state
    internal static string flyingBoss_moveWithoutAttackingPlayerReturnMovement = "moveWithoutAttackPlayerReturnMovement";

    //
    internal static string flyingBoss_attackPlayer = "attackPlayer"; // no animation --> sub-state machine
    // attackPlayerForwardMovement is the default beginning sub-state
    internal static string flyingBoss_attackPlayerDashMovement = "attackPlayerDashMovement";
    internal static string flyingBoss_attackPlayerReturnMovement = "attackPlayerReturnMovement";

    //
    internal static string flyingBoss_quickRangedAttack = "quickRangedAttack"; // idle movement

    //
    internal static string flyingBoss_slowRangedAttack = "slowRangedAttack"; // idle movement

    //
    internal static string flyingBoss_hitPlayerInProximity = "hitPlayerInProximity"; // no animation --> sub-state machine
    // hitPlayerInProximityDashMovement is the default beginning sub-state
    internal static string flyingBoss_hitPlayerInProximityReturnMovement = "hitPlayerInProximityReturnMovement";

}