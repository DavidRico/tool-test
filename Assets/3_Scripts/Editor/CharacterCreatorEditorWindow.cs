using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class CharacterCreatorEditorWindow : EditorWindow
{
    private GameObject fbxFile;
    private Texture materialTexture;
    private GameObject prefab;
    private Material material;
    private Color materialColor;
    private Shader shader;
    private float colliderHeight = 1.1f;
    private float colliderRadius = 0.2f;
    private AnimatorController animatorController;

    private Texture storeIcon;
    private string characterName;
    private int characterPrice;
    private TextAsset csvFile;
    private List<Dictionary<string, object>> parsedCsv;
    private string[] characterNamesFromCsv;
    private int characterIndex;

    private Editor gameObjectEditor;

    [MenuItem("Tools/Character creator")]
    public static void CustomEditorWindow()
    {
        GetWindow<CharacterCreatorEditorWindow>("Character creator");
    }

    private void OnEnable()
    {
        if (shader == null) shader = Shader.Find("Toon/Lit");
    }

    private void OnGUI()
    {
        ShowCreatePrefab();
        ShowConfigurePrefab();
        ShowConfigureStoreIcon();
        ShowCreateStoreEntry();

        ShowInteractivePreviewForPrefab();
    }

    private void ShowCreatePrefab()
    {
        GUILayout.Label("Step 1: Create a prefab from fbx", EditorStyles.largeLabel);

        fbxFile = (GameObject)EditorGUILayout.ObjectField("FBX file", fbxFile, typeof(GameObject), false);

        if (fbxFile != null)
        {
            if (PrefabUtility.GetPrefabAssetType(fbxFile) != PrefabAssetType.Model)
            {
                fbxFile = null;
                ShowNotification(new GUIContent("Drag the fbx asset, not a prefab"));
            }
            else
            {
                ShowSmallButton("Create prefab", () =>
                {
                    prefab = AssetTools.CreatePrefabFromFBX(fbxFile);
                    gameObjectEditor = Editor.CreateEditor(prefab);
                });
            }
        }
    }

    private void ShowConfigurePrefab()
    {
        GUILayout.Label("Step 2: Configure the prefab", EditorStyles.largeLabel);

        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
        material = (Material)EditorGUILayout.ObjectField("Use existing material", material, typeof(Material), false);
        if (material == null)
        {
            materialColor = EditorGUILayout.ColorField("Base color for new material", materialColor);
            materialTexture =
                (Texture)EditorGUILayout.ObjectField("Texture for new material", materialTexture, typeof(Texture),
                    false);
            shader = (Shader)EditorGUILayout.ObjectField("Shader", shader, typeof(Shader));
        }

        animatorController = (AnimatorController)EditorGUILayout.ObjectField("Animator controller", animatorController,
            typeof(AnimatorController), false);
        EditorGUILayout.BeginHorizontal();
        colliderRadius = EditorGUILayout.FloatField("Collider radius", colliderRadius);
        colliderHeight = EditorGUILayout.FloatField("Collider height", colliderHeight);
        EditorGUILayout.EndHorizontal();
        ShowSmallButton("Configure prefab", () =>
        {
            if (material == null)
                material = AssetTools.CreateMaterialFrom(materialColor, materialTexture, shader, prefab.name);
            prefab = AssetTools.ConfigurePrefab(prefab, material, animatorController, colliderRadius, colliderHeight);
            gameObjectEditor = Editor.CreateEditor(prefab);
        });
    }

    private void ShowConfigureStoreIcon()
    {
        GUILayout.Label("Step 3: Configure import settings for store icon", EditorStyles.largeLabel);
        storeIcon = (Texture)EditorGUILayout.ObjectField("Store icon", storeIcon, typeof(Texture), false);
        ShowSmallButton("Configure", () => AssetTools.ConfigureTextureImportSettings(storeIcon));
    }

    private void ShowCreateStoreEntry()
    {
        GUILayout.Label("Step 4: Create entry in the store", EditorStyles.largeLabel);

        TextAsset previousCsv = csvFile;
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV file", csvFile, typeof(TextAsset), false);
        if (csvFile != null && (csvFile != previousCsv || parsedCsv == null))
        {
            parsedCsv = CSVReader.Read(csvFile);
            characterNamesFromCsv = parsedCsv.Select(dictionary => dictionary["Name"] as string).ToArray();
        }

        if (csvFile == null)
        {
            EditorGUILayout.HelpBox("Can't find the csv file", MessageType.Error);
        }
        else
        {
            characterIndex = EditorGUILayout.Popup("Character to create", characterIndex, characterNamesFromCsv);
            characterName = parsedCsv[characterIndex]["Name"] as string;
            characterPrice = (int)parsedCsv[characterIndex]["Price"];
        }

        Sprite storeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(storeIcon));
        if (string.IsNullOrEmpty(characterName))
        {
            EditorGUILayout.HelpBox("Character name cannot be empty", MessageType.Error);
            return;
        }

        if (characterPrice <= 0)
        {
            EditorGUILayout.HelpBox("Did you forget to set the price?", MessageType.Error);
            return;
        }

        if (prefab == null)
        {
            EditorGUILayout.HelpBox("You need to set the prefab reference", MessageType.Error);
            return;
        }

        if (storeSprite == null)
        {
            EditorGUILayout.HelpBox("Store icon not found", MessageType.Error);
            return;
        }

        ShowSmallButton("Create in store",
            () => AssetTools.CreateEntryInStore(characterName, characterPrice, storeSprite, prefab));
    }

    private void ShowInteractivePreviewForPrefab()
    {
        GUILayout.FlexibleSpace();
        GUILayout.Label("Prefab preview", EditorStyles.largeLabel);
        if (gameObjectEditor == null && prefab != null)
        {
            gameObjectEditor = Editor.CreateEditor(prefab);
        }

        if (gameObjectEditor != null)
            gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), GUIStyle.none);
    }

    private static void ShowSmallButton(string label, Action onClick)
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