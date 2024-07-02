using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoad : MonoBehaviour
{
    // player pos
    public GameObject player;
    private float x, y, z;


    public void Save(int level)
    {
        if (player != null && player.transform != null)
        {
            // Respawn point
            Transform transform = RespawnController.instance.GetRespawnPoint();
            x = transform.position.x; 
            y = transform.position.y; 
            z = transform.position.z;

            PlayerPrefs.SetFloat("x_" + level, x);
            PlayerPrefs.SetFloat("y_" + level, y);
            PlayerPrefs.SetFloat("z_" + level, z);


            // Camera
            int cameraNumber = CameraManager.GetActiveCamera();
            PlayerPrefs.SetInt("cameraNumber_" + level, cameraNumber);


            // Progress
            float currentLevelTime = ProgressController.instance.GetCurrentLevelTime();
            PlayerPrefs.SetFloat("levelTime_" + level, currentLevelTime);


            string value = "false";

            if (level == 1)
            {
                if (ProgressController.instance.HasPickedUpAttack())
                {
                    value = "true";
                }
                PlayerPrefs.SetString("hasPickedUpAttack", value);
            }

            if (level == 2)
            {
                if (ProgressController.instance.HasSolvedPuzzle())
                {
                    value = "true";
                }
                else
                {
                    value = "false";
                }
                PlayerPrefs.SetString("hasSolvedPuzzle", value);

                if (ProgressController.instance.HasOvercomeFirstFallingPlatforms())
                {
                    value = "true";
                }
                else
                {
                    value = "false";
                }
                PlayerPrefs.SetString("hasOvercomeFirstFallingPlatforms", value);

                if (ProgressController.instance.HasDefeatedFlyingEnemy())
                {
                    value = "true";
                }
                else
                {
                    value = "false";
                }
                PlayerPrefs.SetString("hasDefeatedFlyingEnemy", value);

                if (ProgressController.instance.HasOvercomeLastFallingPlatforms())
                {
                    value = "true";
                }
                else
                {
                    value = "false";
                }
                PlayerPrefs.SetString("hasOvercomeLastFallingPlatforms", value);
            }
        }
    }


    public void Load(int level)
    {
        if (player != null && player.transform != null)
        {
            // Respawn point
            x = PlayerPrefs.GetFloat("x_" + level, x);
            y = PlayerPrefs.GetFloat("y_" + level, y);
            z = PlayerPrefs.GetFloat("z_" + level, z);

            Vector3 position = new Vector3(x, y, z);
            player.transform.position = position;


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
        }
    }
}
