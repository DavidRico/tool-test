using System;
using UnityEditor;
using UnityEngine;

public static class CustomEditorTools
{
    public static void ShowSmallButton(string label, Action onClick)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(label))
        {
            onClick.Invoke();
        }

        EditorGUILayout.EndHorizontal();
    }
}