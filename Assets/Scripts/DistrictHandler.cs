using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistrictHandler : MonoBehaviour
{
    public static DistrictHandler Instance;
    public static Action OnDistrictsSetUp;

    private bool Selectable;
    private District[] Districts;
    private DistrictData[] DistrictDatas;
    private Transform ChildrenTransform;

    private void Awake()
    {
        Instance = this;
        ActionHandler.OnActionUsed += () => { Selectable = true; };
        GetDistricts();
    }

    private void Start()
    {
        SetupDistricts();
        Selectable = true;
    }

    public void AddDistrictValue(DistrictData data, float amount)
    {
        for (int i = 0; i < DistrictDatas.Length; i++)
        {
            if (DistrictDatas[i].name == data.name)
            {
                Districts[i].AddValue(amount);
            }
        }
    }

    public void AddDistrictValueToAll(float amount)
    {
        foreach (District district in Districts)
        {
            district.AddValue(amount);
        }
    }

    public bool AreDistrictsFromDataDisabled(DistrictData data)
    {
        for (int i = 0; i < DistrictDatas.Length; i++)
        {
            if (DistrictDatas[i].name == data.name)
            {
                return Districts[i].IsDisabled();
            }
        }
        return true;
    }

    public float GetTotalDistrictValues()
    {
        float values = 0;
        foreach (District district in Districts)
        {
            if (district != null)
            {
                values += district.GetValue();
            }
        }
        return values;
    }

    public void ResetDistrictColors()
    {
        foreach (District district in Districts)
        {
            if (!district.IsDisabled())
            {
                district.SetDistrictSpriteColor(Color.white);
            }
        }
    }

    public void ChangeDistrictColor(DistrictData data, Color color)
    {
        for (int i = 0; i < DistrictDatas.Length; i++)
        {
            if (DistrictDatas[i].name == data.name)
            {
                Districts[i].SetDistrictSpriteColor(color);
            }
        }
    }

    public void HideAllDistricts()
    {
        gameObject.SetActive(false);
    }

    public bool AreSelectable()
    {
        return Selectable;
    }

    public void SetSelectable(bool value)
    {
        Selectable = value;
    }

    private void GetDistricts()
    {
        ChildrenTransform = transform.Find("Children");
        int districtCount = ConfigData.District.Districts.Length;
        int districtsPerRow = Mathf.CeilToInt(Mathf.Sqrt(districtCount));
        ChildrenTransform.GetComponent<GridLayout2D>().MaxCells = new Vector2Int(districtsPerRow, districtsPerRow);
        ChildrenTransform.GetComponent<ChildGenerator>().GenerateChildren(districtCount);
    }

    private void SetupDistricts()
    {
        int districtCount = ConfigData.District.Districts.Length;
        DistrictData[] districtsData = GetDistrictsData();
        Districts = new District[districtCount];
        DistrictDatas = new DistrictData[districtCount];
        for (int i = 0; i < ChildrenTransform.childCount; i++)
        {
            Districts[i] = ChildrenTransform.GetChild(i).GetComponent<District>();
            Districts[i].Setup(districtsData[i]);
            DistrictDatas[i] = districtsData[i];
        }
        OnDistrictsSetUp?.Invoke();
    }

    private DistrictData[] GetDistrictsData()
    {
        if (ConfigData.District.AllUnique)
        {
            return CenterDistricts(ConfigData.District.Districts);
        }
        int districtCount = ConfigData.District.Districts.Length;
        List<DistrictData> districtsSelected = new List<DistrictData>();
        List<DistrictData> districtsNotUnique = new List<DistrictData>();
        foreach (DistrictData district in ConfigData.District.Districts)
        {
            if (district.IsUnique)
            {
                districtsSelected.Add(district);
            }
            else
            {
                districtsNotUnique.Add(district);
            }
        }
        if (districtsSelected.Count == 0)
        {
            Debug.LogError("<b>Warning:</b> No District with <b>IsUnique</b> enabled");
        }
        while (districtsSelected.Count < districtCount)
        {
            int randomIndex = UnityEngine.Random.Range(0, districtsNotUnique.Count);
            districtsSelected.Add(districtsNotUnique[randomIndex]);
        }
        if (!ConfigData.District.RandomOrder)
        {
            districtsSelected.Sort(CompareDistrictsByInitialLetter);
        }
        return CenterDistricts(districtsSelected);
    }

    private static int CompareDistrictsByInitialLetter(DistrictData d1, DistrictData d2)
    {
        return d1.GetInitialLetter().CompareTo(d2.GetInitialLetter());
    }

    private DistrictData[] CenterDistricts(IEnumerable<DistrictData> districts)
    {
        int length = BasicUtils.GetEnumerableLength(districts);
        DistrictData[] districtsCentered = new DistrictData[length];
        List<DistrictData> districtsNotCenter = new List<DistrictData>();
        int centerIndex = length / 2;
        bool centerExists = false;
        foreach (DistrictData district in districts)
        {
            if (district.IsCenter)
            {
                if (centerExists)
                {
                    Debug.LogError("<b>Error:</b> More than one District with <b>IsCenter</b> enabled");
                    return null;
                }
                districtsCentered[centerIndex] = district;
                centerExists = true;
            }
            else
            {
                districtsNotCenter.Add(district);
            }
        }
        if (!centerExists)
        {
            Debug.LogError("<b>Warning:</b> No District with <b>IsCenter</b> enabled");
        }
        int lastIndex = 0;
        while (districtsNotCenter.Count > 0)
        {
            int districtIndex = ConfigData.District.RandomOrder ? UnityEngine.Random.Range(0, districtsNotCenter.Count) : 0;
            districtsCentered[lastIndex] = districtsNotCenter[districtIndex];
            districtsNotCenter.RemoveAt(districtIndex);
            lastIndex += (centerExists && lastIndex == centerIndex - 1) ? 2 : 1;
        }
        return districtsCentered;
    }
}
