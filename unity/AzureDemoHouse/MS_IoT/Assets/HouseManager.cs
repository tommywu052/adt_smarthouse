using System;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]

public class HouseManager : MonoBehaviour
{
    public enum HouseState
    {
        None,
        Wrong,
        Open,
        Close,
    }

    [SerializeField]
    private TextMeshPro m_ThermometerText;

    public static HouseManager Instance;
    public static Action<HouseState> OnStateChanged;

    [SerializeField]
    private HouseState m_State;
    public HouseState State { get => m_State; }

    private void Awake()
    {
        Instance = this;
    }

    public void TriggerState(HouseState state)
    {
        OnStateChanged?.Invoke(state);
    }

    public void UpdateThermometerValue(int value, Color color)
    {
        UpdateThermometerValue(value.ToString(), color);
    }

    public void UpdateThermometerValue(string str, Color color)
    {
        if (m_ThermometerText == null || string.IsNullOrEmpty(str)) return;
        m_ThermometerText.text = str;
        m_ThermometerText.color = color;
        m_ThermometerText.ForceMeshUpdate();
    }
}
