using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action Data", fileName = "NewActionData")]
public class ActionData : ScriptableObject
{
    [Header("Consequences")]
    public ActionConsequence[] Consequences;

    public void CheckCorrectInformation(string district)
    {
        if (Consequences.Length == 0)
        {
            Debug.Log("<b>Warning:</b> No Consequences in Action <b>" + name + "</b> in District " + district + "</b>");
        }
        foreach (ActionConsequence consequence in Consequences)
        {
            if (consequence == null)
            {
                Debug.LogError("<b>Error:</b> Empty Consequence in Action <b>" + name + "</b> in District <b>" + district + "</b>");
            }
            else if (consequence.District == null)
            {
                Debug.LogError("<b>Error:</b> Empty District in Consequence <b>" + consequence.Description + "</b> in Action <b>" + name + "</b> in District <b>" + district + "</b>");
            }
        }
    }
}

[Serializable]
public class ActionConsequence
{
    public string Description;
    public DistrictData District;
    public float Value = 0;
}

