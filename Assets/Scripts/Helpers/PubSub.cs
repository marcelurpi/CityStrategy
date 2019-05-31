using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PubSub
{
    private static Dictionary<Type, List<Action<object>>> listeners = new Dictionary<Type, List<Action<object>>>();

    public static void RegisterListener<T>(Action<object> listener) where T : class
    {
        if (!listeners.ContainsKey(typeof(T)))
        {
            listeners.Add(typeof(T), new List<Action<object>>());
        }
        listeners[typeof(T)].Add(listener);
    }

    public static void Publish<T>(T publishedEvent) where T : class
    {
        if(!listeners.ContainsKey(typeof(T)))
        {
            Debug.LogError("<b>Warning:</b> Publishing <b>" + typeof(T).Name + "</b> with no listeners registered");
            return;
        }
        foreach (var action in listeners[typeof(T)])
        {
            action.Invoke(publishedEvent);
        }
    }
}
