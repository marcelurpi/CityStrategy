using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainPopularity : MonoBehaviour
{
    public static MainPopularity Instance;

    private float PopularityValue = 0;
    private Image BarImage;
    private RectTransform MinPopularityMarker;
    private RectTransform MaxPopularityMarker;
    private TextMeshProUGUI BarText;
    private Coroutine AddValueToBarCoroutine;

    private void Awake()
    {
        Instance = this;
        GetChildComponents();
        DistrictHandler.OnDistrictsSetUp += SetupBar;
    }

    public void AddValueToBar(float amount, float time)
    {
        PopularityValue += amount;
        if (AddValueToBarCoroutine == null) {
            AddValueToBarCoroutine = StartCoroutine(AddValueToBarThoughTime(time));
        }
    }

    private void GetChildComponents()
    {
        BarImage = transform.Find("ProgressBarFill").GetComponent<Image>();
        BarText = transform.Find("ProgressBarText").GetComponent<TextMeshProUGUI>();
        MinPopularityMarker = transform.Find("MinPopularityMarker").GetComponent<RectTransform>();
        MaxPopularityMarker = transform.Find("MaxPopularityMarker").GetComponent<RectTransform>();
    }

    private void SetupBar()
    {
        float totalValues = DistrictHandler.Instance.GetTotalDistrictValues();
        UpdateBar(totalValues);
        PopularityValue = totalValues;
        float barSizeX = BarImage.GetComponent<RectTransform>().sizeDelta.x;
        MinPopularityMarker.localPosition = new Vector3(barSizeX * (ConfigData.Game.PopularityToLose / 900 - 0.5f), MinPopularityMarker.localPosition.y, 0);
        MaxPopularityMarker.localPosition = new Vector3(barSizeX * (ConfigData.Game.PopularityToWin / 900 - 0.5f), MaxPopularityMarker.localPosition.y, 0);
    }

    private IEnumerator AddValueToBarThoughTime(float time)
    {
        float start = BarImage.fillAmount * ConfigData.District.MaxValue * 9;
        float elapsed = 0;
        while (elapsed < time)
        {
            float elapsedNormalized = elapsed / time;
            UpdateBar(Mathf.Lerp(start, PopularityValue, elapsedNormalized));
            elapsed += Time.deltaTime;
            yield return null;
        }
        UpdateBar(PopularityValue);
        AddValueToBarCoroutine = null;
    }

    public void UpdateBar(float value)
    {
        if(value <= ConfigData.Game.PopularityToLose)
        {
            GameHandler.Instance.GameOver();
        }
        else if (value >= ConfigData.Game.PopularityToWin)
        {
            GameHandler.Instance.GameWon();
        }
        float valueNormalized = value / (ConfigData.District.MaxValue * 9);
        BarImage.fillAmount = valueNormalized;
        BarText.text = Mathf.Round(value) + " / " + (ConfigData.District.MaxValue * 9);
        if (valueNormalized < 0.5f)
        {
            BarImage.color = Color.Lerp(Color.red, new Color(1, 0.75f, 0), valueNormalized * 2);
        }
        else
        {
            BarImage.color = Color.Lerp(new Color(1, 0.75f, 0), Color.green, valueNormalized * 2 - 1);
        }
    }
}
