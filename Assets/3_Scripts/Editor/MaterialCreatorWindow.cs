using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MaterialCreatorWindow : EditorWindow
{
    private Material material;
    private Color materialColor;
    private Dictionary<string, Texture> materialTextures;
    private Shader shader;
    private string materialName;

    [MenuItem("Tools/Material creator")]
    public static void ShowWindow()
    {
        var window = GetWindow<MaterialCreatorWindow>();
        window.titleContent = new GUIContent("Material creator");
        window.Show();
    }

    private void OnGUI()
    {
        materialName = EditorGUILayout.TextField("Material name", materialName);
        materialColor = EditorGUILayout.ColorField("Base color for new material", materialColor);
        Shader prevShader = shader;
        shader = (Shader)EditorGUILayout.ObjectField("Shader", shader, typeof(Shader), false);
        if (shader == null) shader = Shader.Find("Toon/Lit");
        if (shader != prevShader)
        {
            materialTextures = new Dictionary<string, Texture>();
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    materialTextures.Add(ShaderUtil.GetPropertyName(shader, i), null);
                }
            }
        }

        for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
        {
            if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                string propertyName = ShaderUtil.GetPropertyName(shader, i);
                materialTextures[propertyName] = (Texture)EditorGUILayout.ObjectField(ShaderUtil.GetPropertyDescription(shader, i),
                    materialTextures[propertyName], typeof(Texture), false);
            }
        }

        if (string.IsNullOrEmpty(materialName)) return;

        CustomEditorTools.ShowSmallButton("Create material", () =>
        {
            material = AssetTools.CreateMaterialFrom(materialColor, materialTextures, shader, materialName);
            Selection.SetActiveObjectWithContext(material, material);
        });
    }
}