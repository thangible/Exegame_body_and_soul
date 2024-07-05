using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// run coroutines in static method with this
public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;

    public static CoroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                var runnerGameObject = new GameObject("CoroutineRunner");
                _instance = runnerGameObject.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(runnerGameObject);
            }
            return _instance;
        }
    }
}
