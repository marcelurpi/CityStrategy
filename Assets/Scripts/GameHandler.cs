using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameHandler : MonoBehaviour
{
    public static GameHandler Instance;

    [SerializeField] private ConfigData GameConfigFile;

    private int CurrentDay;
    private float CurrentDayTextFontSizeBase;
    private TextMeshProUGUI AnnouncerText;
    private TextMeshProUGUI CurrentDayText;
    private AudioSource MainAudioSource;

    private const float CURRENT_DAY_TEXT_FONT_SIZE_INCREASED = 50f;
    private const float CURRENT_DAY_TEXT_FONT_SIZE_INCREASED_TIME = 1f;
    private const float CURRENT_CONSEQUENCE_MAX_SPACE_ALLOWED = 1400f;
    private const float CURRENT_CONSEQUENCE_MAX_FONT_SIZE = 40f;

    private void Awake()
    {
        Instance = this;
        SetupConfigFile();
        GetGameObjects();
        ActionHandler.OnActionUsed += () => { StartCoroutine(PassDay()); };
    }

    private void Start()
    {
        AnnouncerText.gameObject.SetActive(false);
        bool isLast = ConfigData.Game.DaysPerYear == ConfigData.Game.LastDaysCount;
        ActionHandler.Instance.SetCurrentMultiplier(isLast ? ConfigData.Game.LastDaysMultiplier : 1);
        UpdateCurrentDayText(1, isLast);
    }

    public void GameOver()
    {
        DistrictHandler.Instance.HideAllDistricts();
        ActionHandler.Instance.HideAllActions();
        CurrentDayText.gameObject.SetActive(false);
        AnnouncerText.text = "Game Over";
        AnnouncerText.color = Color.red;
        AnnouncerText.gameObject.SetActive(true);
    }

    public void GameWon()
    {
        DistrictHandler.Instance.HideAllDistricts();
        ActionHandler.Instance.HideAllActions();
        CurrentDayText.gameObject.SetActive(false);
        AnnouncerText.text = "You Won";
        AnnouncerText.color = Color.green;
        AnnouncerText.gameObject.SetActive(true);
    }

    public void PlaySound(GameSound sound)
    {
        MainAudioSource.Stop();
        MainAudioSource.loop = false;
        MainAudioSource.clip = sound.Clip;
        MainAudioSource.Play();
        float duration = sound.UseTurnDuration ? ConfigData.Action.GetTotalTurnTime() : sound.Duration == -1 ? sound.Clip.length : sound.Duration;
        StartCoroutine(FadeInSound(sound.Volume, duration));
        StartCoroutine(FadeOutSoundAtDuration(sound.Clip, duration));
    }

    private void SetupConfigFile()
    {
        if (GameConfigFile == null)
        {
            Debug.LogError("<b>Error: GameConfigFile</b> not found in <b>GameHandler</b>");
        }
        GameConfigFile.SetAsGameConfigFile();
    }

    public void ShowCurrentConsequenceText(string text)
    {
        CurrentDayText.fontSize = Mathf.Min(CURRENT_CONSEQUENCE_MAX_SPACE_ALLOWED / text.Length, CURRENT_CONSEQUENCE_MAX_FONT_SIZE);
        CurrentDayText.text = text;
    }

    private void GetGameObjects()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        AnnouncerText = canvas.transform.Find("AnnouncerText").GetComponent<TextMeshProUGUI>();
        CurrentDayText = canvas.transform.Find("CurrentDayText").GetComponent<TextMeshProUGUI>();
        CurrentDayTextFontSizeBase = CurrentDayText.fontSize;
        MainAudioSource = GetComponent<AudioSource>();
    }

    private IEnumerator PassDay()
    {
        CurrentDayText.fontSize = CurrentDayTextFontSizeBase;
        yield return new WaitForSeconds(CURRENT_DAY_TEXT_FONT_SIZE_INCREASED_TIME / 2);
        CurrentDayText.fontSize = CURRENT_DAY_TEXT_FONT_SIZE_INCREASED;
        CurrentDay++;
        if (CurrentDay >= ConfigData.Game.DaysPerYear)
        {
            UpdateCurrentDayText(1);
            ActionHandler.Instance.SetCurrentMultiplier(1);
            GameOver();
        }
        else if (CurrentDay >= ConfigData.Game.DaysPerYear - ConfigData.Game.LastDaysCount)
        {
            ActionHandler.Instance.SetCurrentMultiplier(ConfigData.Game.LastDaysMultiplier);
            UpdateCurrentDayText(isLast: true);
        }
        else
        {
            UpdateCurrentDayText();
        }
        yield return new WaitForSeconds(CURRENT_DAY_TEXT_FONT_SIZE_INCREASED_TIME);
        CurrentDayText.fontSize = CurrentDayTextFontSizeBase;
    }

    private void UpdateCurrentDayText(int day = -1, bool isLast = false)
    {
        if (day != -1)
        {
            CurrentDay = day;
        }
        string currentDayText = "DAY " + CurrentDay + " / " + ConfigData.Game.DaysPerYear;
        if (isLast)
        {
            currentDayText += (" (x" + ConfigData.Game.LastDaysMultiplier + ")");
        }
        CurrentDayText.text = currentDayText;
    }

    private IEnumerator FadeInSound(float volume, float duration)
    {
        MainAudioSource.volume = 0;
        float fadeInDurationPercent = 0.25f;
        float endVolume = volume;
        float fadeInDuration = duration * fadeInDurationPercent;
        float x = 0;
        while (x < 1)
        {
            MainAudioSource.volume = Mathf.Lerp(0, endVolume, x);
            x += Time.deltaTime / fadeInDuration;
            yield return null;
        }
        MainAudioSource.volume = endVolume;
    }

    private IEnumerator FadeOutSoundAtDuration(AudioClip clip, float duration)
    {
        float durationPercentToStartFade = 0.75f;
        yield return new WaitForSeconds(duration * durationPercentToStartFade);
        if (MainAudioSource.clip == clip)
        {
            float startVolume = MainAudioSource.volume;
            float fadeOutDuration = duration * (1 - durationPercentToStartFade);
            float x = 0;
            while (x < 1)
            {
                MainAudioSource.volume = Mathf.Lerp(startVolume, 0, x);
                x += Time.deltaTime / fadeOutDuration;
                yield return null;
            }
            MainAudioSource.volume = 0;
            MainAudioSource.Stop();
        }
    }
}
