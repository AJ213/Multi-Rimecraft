using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class MonoBehaviourSingleton<T> : MonoBehaviour
{
    private static Dictionary<Type, object> _singletons
        = new Dictionary<Type, object>();

    public static T Instance
    {
        get
        {
            return (T)_singletons[typeof(T)];
        }
    }

    private void OnDestroy()
    {
        if (gameObject.scene.name != "DontDestroyOnLoad")
        {
            _singletons.Remove(GetType());
        }
    }

    private void OnEnable()
    {
        if (_singletons.ContainsKey(GetType()))
        {
            Destroy(this);
        }
        else
        {
            _singletons.Add(GetType(), this);
        }
    }
}