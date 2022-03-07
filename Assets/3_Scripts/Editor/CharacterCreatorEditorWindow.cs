using System;
using System.Collections.Generic;
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
    private Shader shader = Shader.Find("Toon/Lit");
    private float colliderHeight = 1.1f;
    private float colliderRadius = 0.2f;
    private AnimatorController animatorController;

    private Editor gameObjectEditor;

    [MenuItem("Tools/Character creator")]
    public static void CustomEditorWindow()
    {
        GetWindow<CharacterCreatorEditorWindow>("Character creator");
    }

    private void OnGUI()
    {
        ShowCreatePrefab();
        ShowConfigurePrefab();

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
                    prefab = CreatePrefabFromFBX(fbxFile);
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
            if (material == null) material = CreateMaterialFrom(materialColor, materialTexture, shader, prefab.name);
            prefab = ConfigurePrefab(prefab, material, animatorController, colliderRadius, colliderHeight);
            gameObjectEditor = Editor.CreateEditor(prefab);
        });
    }

    private static Material CreateMaterialFrom(Color color, Texture texture, Shader shader, string name)
    {
        Material newMaterial = new Material(shader)
        {
            color = color,
            mainTexture = texture
        };
        AssetDatabase.CreateAsset(newMaterial, "Assets/1_Graphics/Materials/" + name + ".mat");
        AssetDatabase.Refresh();
        return newMaterial;
    }

    private void ShowInteractivePreviewForPrefab()
    {
        GUILayout.FlexibleSpace();
        GUILayout.Label("Prefab preview", EditorStyles.largeLabel);
        if (gameObjectEditor == null && prefab != null)
        {
            gameObjectEditor = Editor.CreateEditor(prefab);
        }

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
    
    private static GameObject CreatePrefabFromFBX(GameObject model)
    {
        return SavePrefab(model, "Assets/2_Prefabs/" + model.name + ".prefab");
    }
    
    private static GameObject ConfigurePrefab(GameObject prefab, Material material, AnimatorController animatorController, float radius, float height)
    {
        prefab.GetComponent<Animator>().runtimeAnimatorController = animatorController;
        CapsuleCollider capsuleCollider = prefab.GetOrAddComponent<CapsuleCollider>();
        capsuleCollider.radius = radius;
        capsuleCollider.height = height;
        capsuleCollider.center = Vector3.up * (height / 2f);
        AssignMaterials(prefab, material);
        return SavePrefab(prefab, AssetDatabase.GetAssetPath(prefab));
    }

    private static void AssignMaterials(GameObject gameObject, Material material)
    {
        Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
        if (renderer == null) return;
        
        List<Material> materials = new List<Material>();
        foreach (Material rendererMaterial in renderer.sharedMaterials)
        {
            materials.Add(material);
        }
        renderer.sharedMaterials = materials.ToArray();
    }

    private static GameObject SavePrefab(GameObject gameObject, string path)
    {
        GameObject instanceRoot = PrefabUtility.InstantiatePrefab(gameObject) as GameObject;
        GameObject prefabAsset =
            PrefabUtility.SaveAsPrefabAsset(instanceRoot, path);
        DestroyImmediate(instanceRoot);
        return prefabAsset;
    }
}

public static class Extensions
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        return gameObject.TryGetComponent(out T component) ? component : gameObject.AddComponent<T>();
    }
}