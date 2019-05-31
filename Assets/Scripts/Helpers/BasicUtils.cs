using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public static class BasicUtils
{
    private static Camera MainCamera;
    private static PointerEventData MousePointerEventData;

    public static void UpdateGameObjectText(GameObject textGameObject, string newText, Color textColor = new Color(), Action actionAfterUpdatingIfColor = null)
    {
        if (textColor != new Color())
        {
            textGameObject.GetComponent<TextMeshProUGUI>().color = textColor;
            actionAfterUpdatingIfColor?.Invoke();
        }
        textGameObject.GetComponent<TextMeshProUGUI>().text = newText;
    }

    public static List<int> GetRandomOrderRangedList(int max)
    {
        List<int> rangedList = GetRangedList(max);
        List<int> randomOrderRangedList = new List<int>(max);
        for (int i = 0; i < max; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, rangedList.Count);
            randomOrderRangedList.Add(rangedList[randomIndex]);
            rangedList.RemoveAt(randomIndex);
        }
        return randomOrderRangedList;
    }

    public static List<int> GetRangedList(int max)
    {
        List<int> rangedList = new List<int>(max);
        for (int i = 0; i < max; i++)
        {
            rangedList.Add(i);
        }
        return rangedList;
    }

    public static int GetEnumerableLength<T>(IEnumerable<T> enumerable)
    {
        int length = -1;
        if (enumerable.GetType() == typeof(List<T>))
        {
            length = ((List<T>)enumerable).Count;
        }
        else if (enumerable.GetType() == typeof(T[]))
        {
            length = ((T[])enumerable).Length;
        }
        else
        {
            Debug.LogError("<b>Error: BasicUtils</b> doesn't have a method of getting length of an enumerable of type " + typeof(T));
        }
        return length;
    }

    public static Vector2 GetWorldMousePosition2D()
    {
        if(MainCamera == null)
        {
            MainCamera = Camera.main;
        }
        return MainCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    public static PointerEventData GetMousePointerEventData()
    {
        if (MousePointerEventData == null)
        {
            MousePointerEventData = new PointerEventData(FindObjectOfTypeInScene<EventSystem>());
        }
        MousePointerEventData.position = Input.mousePosition;
        return MousePointerEventData;
    }

    public static void StartStaticCoroutine(IEnumerator coroutine)
    {
        CreateMonoBehaviourInstanceIfNeeded();
        MonoBehaviourUtils.StartStaticCoroutine(coroutine);
    }

    public static T FindObjectOfTypeInScene<T>() where T : UnityEngine.Object
    {
        CreateMonoBehaviourInstanceIfNeeded();
        return MonoBehaviourUtils.FindObjectOfTypeInScene<T>();
    }

    public static GameObject FindGameObjectInScene(string name)
    {
        CreateMonoBehaviourInstanceIfNeeded();
        return MonoBehaviourUtils.FindGameObjectInScene(name);
    }

    public static void WaitForSeconds(float seconds, Action action)
    {
        StartStaticCoroutine(WaitForSecondsCoroutine(seconds, action));
    }

    private static void CreateMonoBehaviourInstanceIfNeeded()
    {
        if (MonoBehaviourUtils.Instance == null)
        {
            GameObject staticCoroutinesHelper = new GameObject("StaticCoroutinesHelper", typeof(MonoBehaviourUtils));
            staticCoroutinesHelper.hideFlags = HideFlags.HideInHierarchy;
            MonoBehaviourUtils.Instance = staticCoroutinesHelper.GetComponent<MonoBehaviourUtils>();
        }
    }

    private static IEnumerator WaitForSecondsCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action();
    }
}
