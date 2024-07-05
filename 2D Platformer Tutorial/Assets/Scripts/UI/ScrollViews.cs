using UnityEngine;
using UnityEngine.UI;

public class ScrollViews : MonoBehaviour
{
    public GameObject contentScroll1;
    public GameObject contentScroll2;

    public GameObject textPrefab;
    public GameObject rectanglePrefab;

    public GameObject rectanglePrefabFirst;
    public GameObject rectanglePrefabSecond;
    public GameObject rectanglePrefabThird;

    public int level;


    void PopulateHighScoresScrollView()
    {
        float time_1 = PlayerPrefs.GetFloat("levelTime1_" + level, -1);
        float time_2 = PlayerPrefs.GetFloat("levelTime2_" + level, -1);
        float time_3 = PlayerPrefs.GetFloat("levelTime3_" + level, -1);

        if (time_1 >= 0)
        {
            AddTimeToScrollView(time_1, "first", contentScroll1);
        }
        if (time_2 >= 0)
        {
            AddTimeToScrollView(time_2, "second", contentScroll1);
        }
        if (time_3 >= 0)
        {
            AddTimeToScrollView(time_3, "third", contentScroll1);
        }
    }

    void AddTimeToScrollView(float time, string type, GameObject content)
    {
        if (time >= 0)
        {
            GameObject textObj = Instantiate(textPrefab, content.transform);
            Text textComponent = textObj.GetComponent<Text>();
            textComponent.text = "Time: " + time.ToString("F2");

            if (type == "first")
            {

                GameObject rectangleObj = Instantiate(rectanglePrefabFirst, content.transform);
            }
            else if (type == "second")
             {
                GameObject rectangleObj = Instantiate(rectanglePrefabSecond, content.transform);
            }
            else if (type == "third")
            {
                GameObject rectangleObj = Instantiate(rectanglePrefabThird, content.transform);
            }
            else
            {
                GameObject rectangleObj = Instantiate(rectanglePrefab, content.transform);
            }
        }
    }

    void PopulateHistoryScrollView()
    {
        string existingLevelTimesString = PlayerPrefs.GetString("levelTimeHistory_" + level);
        if (!string.IsNullOrEmpty(existingLevelTimesString))
        {
            string[] times = existingLevelTimesString.Split(',');
            foreach (string time in times)
            {
                // Add Text
                GameObject textObj = Instantiate(textPrefab, contentScroll2.transform);
                Text textComponent = textObj.GetComponent<Text>();
                textComponent.text = "Time: " + time;

                // Add Rectangle
                GameObject rectangleObj = Instantiate(rectanglePrefab, contentScroll2.transform);
            }
        }
    }

}
