using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "District Data", fileName = "NewDistrictData")]
public class DistrictData : ScriptableObject
{
    [Header("District")]
    public bool IsUnique = false;
    public bool IsCenter = false;
    public float StartValue = 50;

    [Header("Actions")]
    public ActionData[] Actions;

    public void CheckCorrectInformation()
    {
        if (Actions.Length == 0)
        {
            Debug.Log("<b>Warning:</b> No Actions in District <b>" + name + "</b>");
        }
        foreach (ActionData action in Actions)
        {
            if (action == null)
            {
                Debug.LogError("<b>Error:</b> Empty Action in District <b>" + name + "</b>");
            }
            else
            {
                action.CheckCorrectInformation(name);
            }
        }
        CheckUnusedActions();
    }

    public void CheckUnusedActions()
    {
        string path = "/Settings/Actions/" + name + "/";
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + path);
        if (!dir.Exists)
        {
            Debug.LogError("<b>Error:</b> Folder for actions of District <b>" + name + "</b> doesn't exist. it should be at <b> Assets" + path + "</b>");
            return;
        }
        foreach (FileInfo file in dir.GetFiles())
        {
            string fileName = file.Name.Replace(".asset", "");
            bool notMetaFile = fileName.Length <= 5 || fileName.Substring(fileName.Length - 5, 5) != ".meta";
            if (notMetaFile && !Array.Exists(Actions, action => action.name == fileName))
            {
                Debug.Log("<b>Warning:</b> Action <b>" + fileName + "</b> is not used. Probably it should be in the actions of District <b>" + name + "</b>");
            }
        }
    }

    public string GetInitialLetter()
    {
        return name.Substring(0, 1);
    }
}
