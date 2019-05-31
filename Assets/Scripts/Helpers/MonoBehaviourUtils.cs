using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourUtils : MonoBehaviour
{
    public static MonoBehaviourUtils Instance;

    public static void StartStaticCoroutine(IEnumerator coroutine)
    {
        Instance.StartCoroutine(coroutine);
    }

    public static T FindObjectOfTypeInScene<T>() where T : UnityEngine.Object
    {
        T gameObject = FindObjectOfType<T>();
        if (gameObject == null)
        {
            Debug.LogError("<b>Error:</b> Didn't find gameObject of type <b>" + typeof(T).Name + "</b> in scene");
        }
        return gameObject;
    }

    public static GameObject FindGameObjectInScene(string name)
    {
        GameObject gameObject = GameObject.Find(name);
        if (gameObject == null)
        {
            Debug.LogError("<b>Error:</b> Didn't find <b>" + name + "</b> gameObject in scene");
        }
        return gameObject;
    }
}
