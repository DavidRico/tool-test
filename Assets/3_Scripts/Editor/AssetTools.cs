using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AssetTools
{
    public static Material CreateMaterialFrom(Color color, Dictionary<string, Texture> textures, Shader shader, string name)
    {
        Material newMaterial = new Material(shader)
        {
            color = color,
        };
        
        foreach (string key in textures.Keys)
        {
            newMaterial.SetTexture(key, textures[key]);
        }
        
        string materialPath = ConfigAsset.instance.materialsPath + name + ".mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) != null)
        {
            if (EditorUtility.DisplayDialog("Warning!",
                    "There is already a material asset with the same name. Do you want to overwrite it?", "Overwrite",
                    "Cancel"))
            {
                AssetDatabase.DeleteAsset(materialPath);
            }
            else
            {
                return null;
            }
        }

        AssetDatabase.CreateAsset(newMaterial, materialPath);
        AssetDatabase.Refresh();
        return newMaterial;
    }

    public static GameObject CreatePrefab(GameObject model, Material[] materials, AnimatorController animatorController,
        float colliderRadius, float colliderHeight, string prefabName)
    {
        string path = ConfigAsset.instance.prefabsPath + prefabName + ".prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            if (!EditorUtility.DisplayDialog("Warning!",
                    "There is already a prefab with the same name. Do you want to overwrite it?", "Overwrite",
                    "Cancel"))
            {
                return null;
            }
        }
        GameObject instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
        instance.GetComponent<Animator>().runtimeAnimatorController = animatorController;
        CapsuleCollider capsuleCollider = instance.GetOrAddComponent<CapsuleCollider>();
        capsuleCollider.radius = colliderRadius;
        capsuleCollider.height = colliderHeight;
        capsuleCollider.center = Vector3.up * (colliderHeight / 2f);
        instance.GetComponentInChildren<Renderer>().sharedMaterials = materials;
        GameObject prefabAsset =
            PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return prefabAsset;
    }

    public static void ConfigureTextureImportSettings(Texture texture)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
        importer.textureType = TextureImporterType.Sprite;
        importer.maxTextureSize = 512;
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
    }

    public static void CreateEntryInStore(string name, int price, Sprite icon, GameObject prefab)
    {
        StoreItem existing = ConfigAsset.instance.store.StoreItems.Find(x => x.Name == name);
        if (existing != null)
        {
            if (EditorUtility.DisplayDialog("Warning!",
                    "There is an entry in the store with the same name. Do you want to overwrite it?", "Overwrite",
                    "Cancel"))
            {
                Undo.RecordObject(ConfigAsset.instance.store, "Overwrite store entry");
                existing.Price = price;
                existing.Icon = icon;
                existing.Prefab = prefab;
            }
            else return;
        }
        else
        {
            StoreItem newItem = new StoreItem();
            int id = 0;
            while (ConfigAsset.instance.store.StoreItems.Exists(x => x.Id == id))
            {
                id++;
            }

            Undo.RecordObject(ConfigAsset.instance.store, "Create store entry");
            newItem.Id = id;
            newItem.Name = name;
            newItem.Price = price;
            newItem.Prefab = prefab;
            newItem.Icon = icon;
            ConfigAsset.instance.store.StoreItems.Add(newItem);
        }
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        return gameObject.TryGetComponent(out T component) ? component : gameObject.AddComponent<T>();
    }
}