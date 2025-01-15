using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AARules))]
public class AARulesEditor : Editor {
    public override void OnInspectorGUI() {
        EditorGUILayout.LabelField("更改完数据后，请点击以下按钮");
        if (GUILayout.Button("数据更新")) {
            AddressableProcessor.InitData(true);
        }
        DrawDefaultInspector();
    }
}
