using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionObject : MonoBehaviour
{
    private const float ACTION_INCREASED_SIZE = 1.1f;
    private const float ACTION_DECREASED_SIZE = 0.9f;

    private void OnMouseEnter()
    {
        if (!Input.GetMouseButton(0) && ActionHandler.Instance.AreSelectable())
        {
            transform.localScale = Vector2.one * ACTION_INCREASED_SIZE;
            ActionHandler.Instance.ShowDistrictsAffectedColors(transform.GetSiblingIndex());
        }
    }

    private void OnMouseExit()
    {
        if (!Input.GetMouseButton(0))
        {
            transform.localScale = Vector2.one;
            DistrictHandler.Instance.ResetDistrictColors();
        }
    }

    private void OnMouseDown()
    {
        DistrictHandler.Instance.ResetDistrictColors();
        transform.localScale = Vector2.one * ACTION_DECREASED_SIZE;
        GameHandler.Instance.PlaySound(ConfigData.Game.ClickSound);
        ActionHandler.Instance.SetSelectable(false);
        DistrictHandler.Instance.SetSelectable(false);
        ActionHandler.Instance.DisableAllActions();
        ActionHandler.Instance.SelectAction(transform.GetSiblingIndex());
    }

    private void OnMouseUpAsButton()
    {
        transform.localScale = Vector2.one;
    }
}
