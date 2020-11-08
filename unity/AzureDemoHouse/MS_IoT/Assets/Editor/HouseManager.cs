using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HouseManager))]
public class HouseManagerEditor : Editor
{
    private HouseManager Target;
    private int temperture;
    private Color color = Color.white;
    private void OnEnable()
    {
        Target = (HouseManager)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        temperture = EditorGUILayout.IntField(temperture);
        color = EditorGUILayout.ColorField(color);

        if (GUILayout.Button("Update thermometer value"))
        {
            Target.UpdateThermometerValue(temperture, color);
        }

        if (GUILayout.Button("Triggr"))
        {
            Target.TriggerState(Target.State);
        }
    }

}
