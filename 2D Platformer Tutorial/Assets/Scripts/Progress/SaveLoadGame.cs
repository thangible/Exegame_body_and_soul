using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveLoadGame : MonoBehaviour
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

            PlayerPrefs.SetFloat("x_" + level.ToString(), x);
            PlayerPrefs.SetFloat("y_" + level.ToString(), y);
            PlayerPrefs.SetFloat("z_" + level.ToString(), z);


            // Camera
            int cameraNumber = CameraManager.GetActiveCamera();
            PlayerPrefs.SetInt("cameraNumber_" + level, cameraNumber);


            // Progress
            PlayerPrefs.SetInt("isFinished_" + level, 0);

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

            PlayerPrefs.Save();
        }
    }



    public void LoadLevel(int level)
    {
        if (player != null && player.transform != null)
        {
            player.SetActive(false);

            // Respawn point
            float x = PlayerPrefs.GetFloat("x_" + level.ToString());
            float y = PlayerPrefs.GetFloat("y_" + level.ToString());
            float z = PlayerPrefs.GetFloat("z_" + level.ToString());

            Vector3 position = new Vector3(x, y, z);
            player.transform.position = position;


            // Settings
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



    public void FinishLevel(int level)
    {
        if (player != null && player.transform != null)
        {
            player.SetActive(false);

            // Respawn point
            PlayerPrefs.DeleteKey("x_" + level);
            PlayerPrefs.DeleteKey("y_" + level);
            PlayerPrefs.DeleteKey("z_" + level);


            // Camera
            PlayerPrefs.DeleteKey("cameraNumber_" + level);


            // Progress
            PlayerPrefs.SetInt("isFinished_" + level, 1);

            float finalLevelTime = ProgressController.instance.GetCurrentLevelTime();
            PlayerPrefs.DeleteKey("levelTime_" + level);
            ProgressController.instance.ResetProgress(level);

            // top 3 times
            float default_value = 99999999.00f;
            float time_1 = PlayerPrefs.GetFloat("levelTime1_" + level, default_value);
            float time_2 = PlayerPrefs.GetFloat("levelTime2_" + level, default_value);
            float time_3 = PlayerPrefs.GetFloat("levelTime3_" + level, default_value);

            if (finalLevelTime < time_1)
            {
                time_3 = time_2;
                time_2 = time_1;
                time_1 = finalLevelTime;
            }
            else if (finalLevelTime < time_2)
            {
                time_3 = time_2;
                time_2 = finalLevelTime;
            }
            else if (finalLevelTime < time_3)
            {
                time_3 = finalLevelTime;
            }

            if (time_1 != default_value) {
                PlayerPrefs.SetFloat("levelTime1_" + level, time_1);
            }
            if (time_2 != default_value)
            {
                PlayerPrefs.SetFloat("levelTime2_" + level, time_2);
            }
            if (time_3 != default_value)
            {
                PlayerPrefs.SetFloat("levelTime3_" + level, time_3);
            }


            // time history
            string existingLevelTimesString = PlayerPrefs.GetString("levelTimeHistory_" + level);
            System.DateTime now = System.DateTime.Now;
            string currentDate = now.ToString("yyyy-MM-dd");
            string currentTime = now.ToString("HH:mm:ss");
            string newString = "[" + currentDate + ";" + currentTime + ";" + finalLevelTime.ToString("F2") + "]";

            string updatedTimesString;
            if (string.IsNullOrEmpty(existingLevelTimesString))
            {
                updatedTimesString = newString;
            }
            else
            {
                updatedTimesString = existingLevelTimesString + ";" + newString;
            }
            PlayerPrefs.SetString("levelTimeHistory_" + level, updatedTimesString);


            if (level == 1)
            {
                PlayerPrefs.DeleteKey("hasPickedUpAttack");
            }

            if (level == 2)
            {
                PlayerPrefs.DeleteKey("hasSolvedPuzzle");
                PlayerPrefs.DeleteKey("hasOvercomeFirstFallingPlatforms");
                PlayerPrefs.DeleteKey("hasDefeatedFlyingEnemy");
                PlayerPrefs.DeleteKey("hasOvercomeLastFallingPlatforms");
            }

            PlayerPrefs.Save();
            player.SetActive(true);
        }
    }
}
