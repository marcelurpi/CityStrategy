using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config Data", fileName = "NewConfigData")]
public class ConfigData : ScriptableObject
{
    [Header("Configurations")]
    [SerializeField] private GameHandlerConfig GameHandlerConfig;
    public static GameHandlerConfig Game;

    [SerializeField] private DistrictHandlerConfig DistrictHandlerConfig;
    public static DistrictHandlerConfig District;

    [SerializeField] private ActionHandlerConfig ActionHandlerConfig;
    public static ActionHandlerConfig Action;

    public void SetAsGameConfigFile()
    {
        Game = GameHandlerConfig;
        District = DistrictHandlerConfig;
        Action = ActionHandlerConfig;
        CheckCorrectInformation();
    }

    private void CheckCorrectInformation()
    {
        Game.CheckCorrectInformation(name);
        District.CheckCorrectInformation(name);
    }
}

[Serializable]
public class GameHandlerConfig
{
    [Header("Years")]
    public int DaysPerYear = 25;
    public int LastDaysCount = 10;
    public float LastDaysMultiplier = 2;

    [Header("Popularity")]
    public float PopularityToLose = 150;
    public float PopularityToWin = 750;

    [Header("Sound")]
    public GameSound ClickSound;
    public GameSound CheerSound;
    public GameSound BooSound;

    public void CheckCorrectInformation(string config)
    {
        CheckCorrectGameSound("Click Sound", ClickSound, config);
        CheckCorrectGameSound("Cheer Sound", CheerSound, config);
        CheckCorrectGameSound("Boo Sound", BooSound, config);
    }

    private void CheckCorrectGameSound(string soundName, GameSound sound, string config)
    {
        if(sound.Clip == null)
        {
            Debug.LogError("<b>Error:</b> the <b>Clip</b> of <b>" + soundName + "</b> is empty in <b>" + config + "</b>");
        }
    }
}

[Serializable]
public class DistrictHandlerConfig
{
    public enum ShakeDurationEnum
    {
        TurnDuration,
        SlideDuration,
        FixedDuration,
    }

    [Header("Districts")]
    public bool AllUnique = true;
    public bool RandomOrder = false;
    public DistrictData[] Districts;

    [Header("District Progress")]
    public float MaxValue = 100;
    public float LowValueWarning = 25;
    public float TotalSlideTime = 0.5f;
    public bool DisableAtZero = true;

    [Header("Shaking")]
    public bool ShakeEnabled = true;
    public ShakeDurationEnum ShakeDuration = ShakeDurationEnum.SlideDuration;
    public float FixedShakeDuration = -1;
    public float ShakeMagnitude = 0.01f;

    public void CheckCorrectInformation(string config)
    {
        if(Districts.Length == 0)
        {
            Debug.Log("<b>Warning:</b> No Districts in Config <b>" + config + "</b>");
        }
        foreach(DistrictData district in Districts)
        {
            if(district == null)
            {
                Debug.LogError("<b>Error:</b> Empty District in Config <b>" + config + "</b>");
            }
            else
            {
                district.CheckCorrectInformation();
            }
        }
        CheckUnusedDistricts(config);
    }

    public void CheckUnusedDistricts(string config)
    {
        string path = "/Settings/Districts/";
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + path);
        if (!dir.Exists)
        {
            Debug.LogError("<b>Error:</b> Folder for Districts doesn't exist. it should be at <b>Assets" + path + "</b>");
            return;
        }
        foreach (FileInfo file in dir.GetFiles())
        {
            string fileName = file.Name.Replace(".asset", "");
            if (fileName.Length <= 5 || fileName.Substring(fileName.Length - 5, 5) != ".meta")
            {
                if (!Array.Exists(Districts, district => district.name == fileName))
                {
                    Debug.Log("<b>Warning:</b> District <b>" + fileName + "</b> is not used. Probably it should be in the Districts of Config <b>" + config + "</b>");
                }
            }
        }
    }

    public float GetShakeDuration()
    {
        switch (ShakeDuration)
        {
            case ShakeDurationEnum.TurnDuration:
                return ConfigData.Action.GetTotalTurnTime();
            case ShakeDurationEnum.SlideDuration:
                return TotalSlideTime;
            case ShakeDurationEnum.FixedDuration:
                return FixedShakeDuration;
            default:
                Debug.LogError("<b>Error:</b> ShakeDuration outside of enum");
                return 0;
        }
    }
}

[Serializable]
public class ActionHandlerConfig
{
    [Header("Consequences")]
    public bool ShowDistrictsAffected = true;
    public float TimeToShowConsequence = 3;
    public float TimeBetweenTurns = 0.25f;

    [Header("After Turn")]
    public bool ConsequenceAllAfterTurn = false;
    public float ValueConsequenceAfterTurn = 0;
    public string DescriptionConsequenceAfterTurn;

    public float GetTotalTurnTime()
    {
        return TimeToShowConsequence + TimeBetweenTurns;
    }
}

[Serializable]
public class GameSound
{
    public AudioClip Clip;
    public float Volume = 1;
    public bool UseTurnDuration = false;
    public float Duration = -1;
}