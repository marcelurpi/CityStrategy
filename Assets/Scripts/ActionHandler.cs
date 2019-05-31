using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ActionHandler : MonoBehaviour
{
    public static event Action OnActionUsed;

    public static ActionHandler Instance;

    private bool Selectable;
    private float CurrentMultiplier;
    private GraphicRaycaster UIGraphicRaycaster;
    private List<RaycastResult> RaycastResults;
    private District SelectedDistrict;
    private ActionData[] SelectedDistrictActions;
    private Transform ChildrenTransform;

    private const int MAX_ACTIONS = 5;
    private const float CONSEQUENCE_MAX_VALUE_FOR_LERPING = 60f;
    private const float CONSEQUENCE_MIN_LERP_VALUE = 0.5f;

    private void Awake()
    {
        Instance = this;
        UIGraphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        RaycastResults = new List<RaycastResult>();
        ChildrenTransform = transform.Find("Children");
    }

    private void Start()
    {
        ChildrenTransform.GetComponent<ChildGenerator>().GenerateChildren(MAX_ACTIONS);
        ChildrenTransform.GetComponent<GridLayout2D>().UpdateChildrenPosition(new Vector2Int(MAX_ACTIONS, 1), true);
        HideAllActions();
        Selectable = true;
    }

    public void SetSelectedDistrict(District district)
    {
        if (district != null)
        {
            SelectedDistrict = district;
            SelectedDistrictActions = district.GetActions();
            SetupActions();
            ShowAllActions();
        }
        else
        {
            HideAllActions();
        }
    }

    public float GetCurrentMultiplier()
    {
        return CurrentMultiplier;
    }

    public void SetCurrentMultiplier(float value)
    {
        CurrentMultiplier = value;
    }

    public void HideAllActions()
    {
        ChildrenTransform.gameObject.SetActive(false);
    }

    public void ShowAllActions()
    {
        ChildrenTransform.gameObject.SetActive(true);
    }

    public void SetSelectable(bool value)
    {
        Selectable = value;
    }

    public bool AreSelectable()
    {
        return Selectable;
    }

    public void DisableAllActions()
    {
        foreach (Transform child in ChildrenTransform)
        {
            child.Find("Sprite").GetComponent<Image>().color = Color.gray;
        }
    }

    public void SelectAction(int actionIndex)
    {
        if (actionIndex >= SelectedDistrictActions.Length)
        {
            Debug.LogError("<b>Error:</b> More choices than actions for district <b>" + SelectedDistrict.name + "</b>");
        }
        ActionData actionSelected = SelectedDistrictActions[actionIndex];
        StartCoroutine(ShowAllConsequences(actionSelected));
    }

    public void ShowDistrictsAffectedColors(int index)
    {
        if (!ConfigData.Action.ShowDistrictsAffected)
        {
            return;
        }
        ActionData action = SelectedDistrictActions[index];
        foreach (ActionConsequence consequence in action.Consequences)
        {
            if (!DistrictHandler.Instance.AreDistrictsFromDataDisabled(consequence.District))
            {
                Color color = consequence.Value < 0 ? Color.red : Color.green;
                float value = Mathf.Max(Mathf.Abs(consequence.Value) / CONSEQUENCE_MAX_VALUE_FOR_LERPING, CONSEQUENCE_MIN_LERP_VALUE);
                color = Color.Lerp(Color.white, color, value);

                DistrictHandler.Instance.ChangeDistrictColor(consequence.District, color);
            }
        }
    }

    private void SetupActions()
    {
        int selectedDistrictActionsCount = SelectedDistrictActions.Length;
        ChildrenTransform.GetComponent<GridLayout2D>().UpdateChildrenPosition(new Vector2Int(selectedDistrictActionsCount, 1), true);
        for (int i = 0; i < selectedDistrictActionsCount; i++)
        {
            string actionText = SelectedDistrictActions[i].name;
            ChildrenTransform.GetChild(i).Find("Text").GetComponent<TextMeshProUGUI>().text = actionText;
        }
    }

    private void EnableAllActions()
    {
        foreach (Transform child in ChildrenTransform)
        {
            child.Find("Sprite").GetComponent<Image>().color = Color.white;
        }
    }

    private IEnumerator ShowAllConsequences(ActionData actionSelected)
    {
        foreach (ActionConsequence consequence in actionSelected.Consequences)
        {
            if (!DistrictHandler.Instance.AreDistrictsFromDataDisabled(consequence.District))
            {
                DistrictHandler.Instance.AddDistrictValue(consequence.District, CurrentMultiplier * consequence.Value);
                yield return StartCoroutine(ShowConsequence(consequence.Description));
            }
        }
        if (ConfigData.Action.ConsequenceAllAfterTurn)
        {
            DistrictHandler.Instance.AddDistrictValueToAll(CurrentMultiplier * ConfigData.Action.ValueConsequenceAfterTurn);
            yield return ShowConsequence(ConfigData.Action.DescriptionConsequenceAfterTurn);
        }
        Selectable = true;
        if (!SelectedDistrict.IsDisabled())
        {
            EnableAllActions();
        }
        OnActionUsed?.Invoke();
    }

    private IEnumerator ShowConsequence(string consequenceDescription)
    {
        GameHandler.Instance.ShowCurrentConsequenceText(consequenceDescription);
        yield return new WaitForSeconds(ConfigData.Action.TimeToShowConsequence);
        yield return new WaitForSeconds(ConfigData.Action.TimeBetweenTurns);
    }
}
