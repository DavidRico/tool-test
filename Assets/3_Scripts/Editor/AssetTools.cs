using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AssetTools
{
    public static Material CreateMaterialFrom(Color color, Texture texture, Shader shader, string name)
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


    public static GameObject CreatePrefabFromFBX(GameObject model)
    {
        return SavePrefab(model, "Assets/2_Prefabs/" + model.name + ".prefab");
    }

    public static GameObject ConfigurePrefab(GameObject prefab, Material material,
        AnimatorController animatorController, float radius, float height)
    {
        prefab.GetComponent<Animator>().runtimeAnimatorController = animatorController;
        CapsuleCollider capsuleCollider = prefab.GetOrAddComponent<CapsuleCollider>();
        capsuleCollider.radius = radius;
        capsuleCollider.height = height;
        capsuleCollider.center = Vector3.up * (height / 2f);
        AssignMaterials(prefab, material);
        return SavePrefab(prefab, AssetDatabase.GetAssetPath(prefab));
    }

    public static void AssignMaterials(GameObject gameObject, Material material)
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

    public static GameObject SavePrefab(GameObject gameObject, string path)
    {
        GameObject instanceRoot = PrefabUtility.InstantiatePrefab(gameObject) as GameObject;
        GameObject prefabAsset =
            PrefabUtility.SaveAsPrefabAsset(instanceRoot, path);
        Object.DestroyImmediate(instanceRoot);
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
        StoreItem existing = Store.Instance.StoreItems.Find(x => x.Name == name);
        if (existing != null)
        {
            if (EditorUtility.DisplayDialog("Warning!",
                    "There is an entry in the store with the same name. Do you want to overwrite it?", "Overwrite",
                    "Cancel"))
            {
                Undo.RecordObject(Store.Instance, "Overwrite store entry");
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
            while (Store.Instance.StoreItems.Exists(x=>x.Id == id))
            {
                id++;
            }

            Undo.RecordObject(Store.Instance, "Create store entry");
            newItem.Id = id;
            newItem.Name = name;
            newItem.Price = price;
            newItem.Prefab = prefab;
            newItem.Icon = icon;
            Store.Instance.StoreItems.Add(newItem);
        }
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        return gameObject.TryGetComponent(out T component) ? component : gameObject.AddComponent<T>();
    }
}