using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class CharacterCreatorEditorWindow : EditorWindow
{
    private GameObject fbxFile;
    private GameObject prefab;
    private Shader shader;
    private float colliderHeight;
    private float colliderRadius;
    private AnimatorController animatorController;

    private Texture storeIcon;
    private string characterName;
    private int characterPrice;
    private int characterIndex;
    private int filteredCharacterIndex;
    private int numberOfMaterials;
    private Material[] materials;

    private bool showExistingCharacters;

    private Editor gameObjectEditor;

    [MenuItem("Tools/Character creator")]
    public static void ShowWindow()
    {
        var window = GetWindow<CharacterCreatorEditorWindow>();
        window.titleContent = new GUIContent("Character creator");
        window.Show();
    }

    private void OnEnable()
    {
        if (shader == null) shader = Shader.Find("Toon/Lit");
    }

    private void OnGUI()
    {
        ShowCompleteProcess();
        ShowConfigButtonAndInteractivePreviewForPrefab();
    }
    
    private void ShowCompleteProcess()
    {
        if (!ShowModelConfiguration()) return;
        if (!ShowAnimatorAndColliderConfiguration()) return;
        if (!ShowStoreInfoConfiguration()) return;
        if (!ShowCreatePrefabAndStoreInfo()) return;
    }

    private bool ShowModelConfiguration()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Model and material configuration", EditorStyles.largeLabel);
        fbxFile = (GameObject)EditorGUILayout.ObjectField("FBX file", fbxFile, typeof(GameObject), false);
        if (fbxFile == null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Select a model asset to continue", MessageType.Info);
            return false;
        }

        if (PrefabUtility.GetPrefabAssetType(fbxFile) != PrefabAssetType.Model)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Selected object is not a model asset. Use the model, not a prefab",
                MessageType.Error);
            return false;
        }

        if (numberOfMaterials == 0)
        {
            GameObject temporalPrefab = PrefabUtility.InstantiatePrefab(fbxFile) as GameObject;
            numberOfMaterials = temporalPrefab.GetComponentInChildren<Renderer>().sharedMaterials.Length;
            DestroyImmediate(temporalPrefab);
            return false;
        }

        if (materials == null || materials.Length != numberOfMaterials) materials = new Material[numberOfMaterials];

        for (int i = 0; i < numberOfMaterials; i++)
        {
            materials[i] =
                (Material)EditorGUILayout.ObjectField($"Material slot {i}", materials[i], typeof(Material), false);
        }

        if (materials.Any(x => x == null))
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Do you need to create materials?", MessageType.Info);
            if (GUILayout.Button("Material creator"))
            {
                MaterialCreatorWindow.ShowWindow();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("You will be able to continue once all the material slots have been set",
                MessageType.None);
            return false;
        }

        return true;
    }
    
    private bool ShowAnimatorAndColliderConfiguration()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Animator and collider configuration", EditorStyles.largeLabel);
        animatorController = (AnimatorController)EditorGUILayout.ObjectField("Animator controller", animatorController,
            typeof(AnimatorController), false);
        EditorGUILayout.BeginHorizontal();
        
        if (colliderHeight == 0) colliderHeight = ConfigAsset.instance.colliderDefaultHeight;
        if (colliderRadius == 0) colliderRadius = ConfigAsset.instance.colliderDefaultRadius;
        
        colliderRadius = EditorGUILayout.FloatField("Collider radius", colliderRadius);
        colliderHeight = EditorGUILayout.FloatField("Collider height", colliderHeight);
        EditorGUILayout.EndHorizontal();
        if (animatorController == null || colliderHeight == 0 || colliderRadius == 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "You will be able to continue once the animator controller and collider size have been set",
                MessageType.None);
            return false;
        }

        return true;
    }
    
    private bool ShowStoreInfoConfiguration()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Store info configuration", EditorStyles.largeLabel);

        if (ConfigAsset.instance.csvFile == null)
        {
            EditorGUILayout.HelpBox("Can't find the csv file. Enter the configuration to set it", MessageType.Error);
            return false;
        }
        
        if (ConfigAsset.instance.store == null)
        {
            EditorGUILayout.HelpBox("Can't find the store asset file. Enter the configuration to set it", MessageType.Error);
            return false;
        }

        showExistingCharacters = EditorGUILayout.Toggle("Show existing characters", showExistingCharacters);

        if (showExistingCharacters)
        {
            characterIndex = EditorGUILayout.Popup("Character to create", characterIndex, ConfigAsset.instance.characterNamesFromCsv);
        }
        else
        {
            List<string> names = ConfigAsset.instance.characterNamesFromCsv.ToList();
            names.RemoveAll(x => ConfigAsset.instance.store.StoreItems.Exists(y => y.Name == x));
            
            if (names.Count == 0)
            {
                EditorGUILayout.HelpBox("Can't find a character in the csv file that is not already in the store", MessageType.Error);
                return false;
            }
            filteredCharacterIndex = EditorGUILayout.Popup("Character to create", filteredCharacterIndex, names.ToArray());
            if (filteredCharacterIndex == -1) filteredCharacterIndex = 0;
            characterIndex = ConfigAsset.instance.characterNamesFromCsv.ToList().IndexOf(names[filteredCharacterIndex]);
        }
        
        characterName = ConfigAsset.instance.parsedCsv[characterIndex]["Name"] as string;
        characterPrice = (int)ConfigAsset.instance.parsedCsv[characterIndex]["Price"];
        storeIcon = (Texture)EditorGUILayout.ObjectField("Store icon", storeIcon, typeof(Texture), false);

        if (storeIcon == null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("You need to set the store icon to continue", MessageType.None);
            return false;
        }

        return true;
    }
    
    private bool ShowCreatePrefabAndStoreInfo()
    {
        Sprite storeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(storeIcon));
        if (storeSprite == null)
        {
            EditorGUILayout.HelpBox("Store icon sprite not found, is it correctly configured?", MessageType.Error);
            return false;
        }

        CustomEditorTools.ShowSmallButton("Add to store", () =>
        {
            prefab = AssetTools.CreatePrefab(fbxFile, materials, animatorController, colliderRadius, colliderHeight,
                characterName);
            AssetTools.ConfigureTextureImportSettings(storeIcon);
            AssetTools.CreateEntryInStore(characterName, characterPrice, storeSprite, prefab);
            Selection.SetActiveObjectWithContext(prefab, prefab);
            gameObjectEditor = Editor.CreateEditor(prefab);
        });
        return true;
    }

    private void ShowConfigButtonAndInteractivePreviewForPrefab()
    {
        GUILayout.FlexibleSpace();
        CustomEditorTools.ShowSmallButton("Configuration", ConfigWindow.ShowWindow);

        if (prefab == null) return;

        GUILayout.Label("Prefab preview", EditorStyles.largeLabel);
        if (gameObjectEditor == null)
        {
            gameObjectEditor = Editor.CreateEditor(prefab);
        }

        if (gameObjectEditor != null)
        {
            gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), GUIStyle.none);
        }
    }
}