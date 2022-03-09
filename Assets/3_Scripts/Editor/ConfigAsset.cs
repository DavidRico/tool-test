using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[FilePath("Assets/CharacterCreatorConfig.asset", FilePathAttribute.Location.ProjectFolder)]
public class ConfigAsset : ScriptableSingleton<ConfigAsset>
{
    public TextAsset csvFile;
    public List<Dictionary<string, object>> parsedCsv;
    public string[] characterNamesFromCsv;

    public StoreSO store;
    
    public float colliderDefaultHeight;
    public float colliderDefaultRadius;

    private void OnEnable()
    {
        parsedCsv = null;
        Setup();
    }

    public void Setup()
    {
        if (csvFile == null) return;
        parsedCsv = CSVReader.Read(csvFile);
        characterNamesFromCsv = parsedCsv.Select(dictionary => dictionary["Name"] as string).ToArray();
    }

    public void Reload()
    {
        Setup();
        SaveToFile();
    }

    public void SaveToFile()
    {
        Save(true);
    }

}