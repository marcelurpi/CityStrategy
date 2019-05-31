using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ChildGenerator : MonoBehaviour
{
    [SerializeField] private GameObject ChildPrefab;
    [SerializeField] private int ChildrenCount;

    private void Start()
    {
        if(ChildPrefab == null)
        {
            SaveFirstAsPrefab();
        }
    }

    public void SaveFirstAsPrefab()
    {
        if (transform.childCount > 0)
        {
            ChildPrefab = transform.GetChild(0).gameObject;
        }
    }

    public void GenerateChildren(int childrenCount = -1, bool destroyLast = true)
    {
        if (ChildPrefab == null)
        {
            SaveFirstAsPrefab();
        }
        if (childrenCount == -1)
        {
            childrenCount = ChildrenCount;
        }
        if (transform.childCount > childrenCount)
        {
            DestroyExtraChildren(childrenCount, destroyLast);
        }
        else if(transform.childCount < childrenCount)
        {
            for (int i = transform.childCount + 1; i < childrenCount + 1; i++)
            {
                GameObject child = Instantiate(ChildPrefab, transform);
                string lastChar = ChildPrefab.name.Substring(ChildPrefab.name.Length - 1);
                string otherChars = ChildPrefab.name.Substring(0, ChildPrefab.name.Length - 1);
                child.name = lastChar == "1" ? otherChars + i : otherChars + lastChar + i;
            }
        }
    }

    public void DestroyChildren(bool destroyLast = true)
    {
        if (destroyLast)
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
        else
        {
            while (transform.childCount > 1)
            {
                DestroyImmediate(transform.GetChild(1).gameObject);
            }
        }

    }

    public void DestroyExtraChildren(int childrenCount, bool destroyLast = true)
    {
        while (transform.childCount > childrenCount && (destroyLast || transform.childCount > 1))
        {
            DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);
        }
    }
}

[CustomEditor(typeof(ChildGenerator))]
public class ChildGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Separator();
        if (GUILayout.Button("Generate children"))
        {
            ChildGenerator generator = (ChildGenerator)serializedObject.targetObject;
            generator.GenerateChildren(destroyLast: false);
        }
        if (GUILayout.Button("Destroy children"))
        {
            ChildGenerator generator = (ChildGenerator)serializedObject.targetObject;
            generator.DestroyChildren(false);
        }
        if (GUILayout.Button("Save First As Prefab"))
        {
            ChildGenerator generator = (ChildGenerator)serializedObject.targetObject;
            generator.SaveFirstAsPrefab();
        }
    }
}
