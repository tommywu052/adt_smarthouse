using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FanController))]
public class FanControllerEditor : Editor
{
    private FanController Target;

    private void OnEnable()
    {
        Target = (FanController)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("On/Off"))
        {
            if (Target.IsOn) Target.TurnOff();
            else Target.TurnOn();
        }
    }
}
