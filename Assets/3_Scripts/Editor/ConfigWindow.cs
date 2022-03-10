using UnityEditor;
using UnityEngine;


public class ConfigWindow : EditorWindow
{
    [MenuItem("Tools/Character creator configuration")]
    public static void ShowWindow()
    {
        var window = GetWindow<ConfigWindow>();
        window.titleContent = new GUIContent("Configuration window");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        ConfigAsset.instance.csvFile =
            (TextAsset)EditorGUILayout.ObjectField("CSV file", ConfigAsset.instance.csvFile, typeof(TextAsset), false);
        
        if (EditorGUI.EndChangeCheck() && ConfigAsset.instance.csvFile != null)
        {
            ConfigAsset.instance.Reload();
        }
        CustomEditorTools.ShowSmallButton("Reload", ConfigAsset.instance.Reload);
        
        EditorGUI.BeginChangeCheck();

        ConfigAsset.instance.store =
            (StoreSO)EditorGUILayout.ObjectField("Store asset", ConfigAsset.instance.store, typeof(StoreSO), false);
        
        ConfigAsset.instance.colliderDefaultRadius = EditorGUILayout.FloatField("Default collider radius",
            ConfigAsset.instance.colliderDefaultRadius);
        
        ConfigAsset.instance.colliderDefaultHeight = EditorGUILayout.FloatField("Default collider height",
            ConfigAsset.instance.colliderDefaultHeight);
        
        ConfigAsset.instance.prefabsPath = EditorGUILayout.TextField("Prefabs folder location",
            ConfigAsset.instance.prefabsPath);
        
        ConfigAsset.instance.materialsPath = EditorGUILayout.TextField("Materials folder location",
            ConfigAsset.instance.materialsPath);
        
        if (EditorGUI.EndChangeCheck())
        {
            ConfigAsset.instance.SaveToFile();
        }
        
    }
}