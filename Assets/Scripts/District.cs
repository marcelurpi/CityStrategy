using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class District : MonoBehaviour
{
    private bool Disabled;
    private float Value;
    private DistrictData Data;
    private TextMeshPro NameText;
    private TextMeshPro IconText;
    private TextMeshPro ProgressBarText;
    private SpriteRenderer Sprite;
    private SpriteRenderer ProgressBarFillRenderer;
    private Coroutine AddValueCoroutine;
    private Coroutine UpdateBackgroundRepeatedlyCoroutine;

    private const float DISTRICT_INCREASED_SIZE = 1.05f;
    private const float DISTRICT_DECREASED_SIZE = 0.9f;
    private const float DISTRICT_INCREASED_SIZE_BIG = 1.2f;

    private void Awake()
    {
        GetChildComponents();
    }

    private void OnMouseEnter()
    {
        if (!Input.GetMouseButton(0) && DistrictHandler.Instance.AreSelectable() && !Disabled)
        {
            transform.localScale = Vector2.one * DISTRICT_INCREASED_SIZE;
        }
    }

    private void OnMouseExit()
    {
        if (!Input.GetMouseButton(0) && DistrictHandler.Instance.AreSelectable())
        {
            transform.localScale = Vector2.one;
        }
    }

    private void OnMouseDown()
    {
        if (DistrictHandler.Instance.AreSelectable() && !Disabled)
        {
            transform.localScale = Vector2.one * DISTRICT_DECREASED_SIZE;
            GameHandler.Instance.PlaySound(ConfigData.Game.ClickSound);
            ActionHandler.Instance.SetSelectedDistrict(this);
        }
    }

    private void OnMouseUp()
    {
        if (DistrictHandler.Instance.AreSelectable())
        {
            transform.localScale = Vector2.one;
        }
    }

    public void Setup(DistrictData data)
    {
        Data = data;
        name = data.name;
        Value = data.StartValue;
        NameText.text = data.name;
        IconText.text = data.GetInitialLetter();
        Disabled = false;
        UpdateProgressBar();
    }

    public void AddValue(float amount)
    {
        if (!Disabled)
        {
            if (AddValueCoroutine == null)
            {
                AddValueCoroutine = StartCoroutine(AddValueThroughTime(amount));
                if (ConfigData.District.ShakeEnabled)
                {
                    StartCoroutine(ShakeDistrict());
                }
                StartCoroutine(IncreaseDecreaseDistrictsSize(amount));
            }
            else
            {
                Debug.LogError("<b>Error:</b> Cannot add value to District <b>" + name + "</b> while another value is still being added");
            }
        }
    }

    private IEnumerator AddValueThroughTime(float amount)
    {
        float start = Value;
        float end = Mathf.Clamp(Value + amount, 0, ConfigData.District.MaxValue);
        PlayCrowdSound(amount > 0);
        float elapsed = 0;
        float duration = ConfigData.District.TotalSlideTime;
        MainPopularity.Instance.AddValueToBar(amount, duration);
        while (elapsed < duration)
        {
            float elapsedNormalized = elapsed / duration;
            Value = Mathf.Lerp(start, end, elapsedNormalized);
            UpdateProgressBar();
            elapsed += Time.deltaTime;
            yield return null;
        }
        Value = end;
        UpdateProgressBar();
        AddValueCoroutine = null;
    }

    public ActionData[] GetActions()
    {
        return Data.Actions;
    }

    public bool IsDisabled()
    {
        return Disabled;
    }

    public float GetValue()
    {
        return Value;
    }

    private void GetChildComponents()
    {
        Transform levelTexts = transform.Find("LevelTexts");
        NameText = transform.Find("NameText").GetComponent<TextMeshPro>();
        IconText = transform.Find("IconText").GetComponent<TextMeshPro>();

        Transform progressBar = transform.Find("ProgressBar");
        ProgressBarText = progressBar.Find("ProgressBarText").GetComponent<TextMeshPro>();
        ProgressBarFillRenderer = progressBar.Find("ProgressBarFill").GetComponent<SpriteRenderer>();

        Sprite = transform.Find("Background").Find("Sprite").GetComponent<SpriteRenderer>();
    }

    private void UpdateProgressBar()
    {
        ProgressBarText.text = Mathf.Round(Value) + " / " + ConfigData.District.MaxValue;
        float valueNormalized = Value / ConfigData.District.MaxValue;
        ProgressBarFillRenderer.size = new Vector2(valueNormalized, 1);
        if (valueNormalized < 0.5f)
        {
            ProgressBarFillRenderer.color = Color.Lerp(Color.red, new Color(1, 0.75f, 0), valueNormalized * 2);
        }
        else
        {
            ProgressBarFillRenderer.color = Color.Lerp(new Color(1, 0.75f, 0), Color.green, valueNormalized * 2 - 1);
        }
        if(Value <= ConfigData.District.LowValueWarning && UpdateBackgroundRepeatedlyCoroutine == null)
        {
            UpdateBackgroundRepeatedlyCoroutine = StartCoroutine(UpdateBackgroundRepeatedly());
        }
        if(Value == 0 && ConfigData.District.DisableAtZero)
        {
            DisableDistrict();
        }
    }

    private void PlayCrowdSound(bool cheer)
    {
        GameSound crowdSound = cheer ? ConfigData.Game.CheerSound : ConfigData.Game.BooSound;
        if (crowdSound.Clip != null)
        {
            GameHandler.Instance.PlaySound(crowdSound);
        }
        else
        {
            Debug.LogError("<b>Error: Crowd " + (cheer ? "Cheer" : "Boo") + " Sound</b> is null. Check Config Sounds");
        }
    }

    private void DisableDistrict()
    {
        Disabled = true;
        if (UpdateBackgroundRepeatedlyCoroutine != null)
        {
            StopCoroutine(UpdateBackgroundRepeatedlyCoroutine);
            UpdateBackgroundRepeatedlyCoroutine = null;
        }
        SetDistrictSpriteColor(Color.gray);
        StartCoroutine(ShakeDistrict(0.05f));
        NameText.gameObject.SetActive(false);
        IconText.gameObject.SetActive(false);
        ProgressBarFillRenderer.transform.parent.gameObject.SetActive(false);
    }

    private IEnumerator UpdateBackgroundRepeatedly()
    {
        while (true)
        {
            SetDistrictSpriteColor(Color.gray);
            yield return new WaitForSeconds(0.5f);
            SetDistrictSpriteColor(Color.white);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator ShakeDistrict(float magn = 0)
    {
        float elapsed = 0;
        Vector3 originalPosition = transform.localPosition;
        float magnitude = magn == 0 ? ConfigData.District.ShakeMagnitude : magn;
        float duration = ConfigData.District.GetShakeDuration();
        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = originalPosition + new Vector3(x, y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPosition;
    }

    private IEnumerator IncreaseDecreaseDistrictsSize(float amount)
    {
        float v = Mathf.Min(Mathf.Abs(amount) / (40 * ActionHandler.Instance.GetCurrentMultiplier()), 1);
        float increasedSize = Mathf.Lerp(1, DISTRICT_INCREASED_SIZE_BIG, v);
        transform.localScale = Vector2.one * increasedSize;
        float duration = ConfigData.Action.GetTotalTurnTime();
        yield return new WaitForSeconds(duration);
        transform.localScale = Vector2.one;
    }

    public void SetDistrictSpriteColor(Color color)
    {
        Sprite.color = color;
    }
}
