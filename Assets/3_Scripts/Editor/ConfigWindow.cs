using System;
using UnityEditor;
using UnityEngine;


public class ConfigWindow : EditorWindow
{
    [MenuItem("Tools/Configuration window")]
    public static void ShowWindow()
    {
        var window = GetWindow<ConfigWindow>();
        window.titleContent = new GUIContent("Configuration window");
        window.Show();
    }

    private void OnGUI()
    {
        TextAsset previousCsv = ConfigAsset.instance.csvFile;
        ConfigAsset.instance.csvFile =
            (TextAsset)EditorGUILayout.ObjectField("CSV file", ConfigAsset.instance.csvFile, typeof(TextAsset), false);

        if (ConfigAsset.instance.csvFile != null &&
            (ConfigAsset.instance.csvFile != previousCsv || ConfigAsset.instance.parsedCsv == null))
        {
            ConfigAsset.instance.Reload();
        }

        CustomEditorTools.ShowSmallButton("Reload", ConfigAsset.instance.Reload);

        StoreSO storeAsset = ConfigAsset.instance.store;
        ConfigAsset.instance.store =
            (StoreSO)EditorGUILayout.ObjectField("Store asset", ConfigAsset.instance.store, typeof(StoreSO), false);
        if (storeAsset != ConfigAsset.instance.store)
        {
            ConfigAsset.instance.SaveToFile();
        }

        float prevcolliderDefaultRadius = ConfigAsset.instance.colliderDefaultRadius;
        ConfigAsset.instance.colliderDefaultRadius = EditorGUILayout.FloatField("Default collider radius",
            ConfigAsset.instance.colliderDefaultRadius);
        float prevcolliderDefaultHeight = ConfigAsset.instance.colliderDefaultHeight;
        ConfigAsset.instance.colliderDefaultHeight = EditorGUILayout.FloatField("Default collider height",
            ConfigAsset.instance.colliderDefaultHeight);
        if (prevcolliderDefaultRadius != ConfigAsset.instance.colliderDefaultRadius ||
            prevcolliderDefaultHeight != ConfigAsset.instance.colliderDefaultHeight)
        {
            ConfigAsset.instance.SaveToFile();
        }
    }
}