using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    public static LevelSelector Instance;
    
    public GridLevel gridLevel;
    public Transform levelsParent;
    public LevelButton levelButtonPrefab;
    
    private static string LevelsPath => Path.Combine(Application.dataPath, "Levels");
    
    private List<LevelData> _allLevels;

    private void Start()
    {
        Instance = this;

        levelsParent.gameObject.SetActive(false);

        LoadAllLevelPaths();

        LevelData levelData = _allLevels.First();
        
        gridLevel.SetupGridForLevel(levelData);
    }
    
    public void ToggleLevels()
    {
        levelsParent.gameObject.SetActive(!levelsParent.gameObject.activeSelf);
    }

    public void LoadAllLevelPaths()
    {
        List<string> levelFilenames = new List<string>(Directory.GetFiles(LevelsPath, "*.json"));
        levelFilenames.Sort();
        Debug.Log($"i found {levelFilenames.Count} levels");

        _allLevels = new List<LevelData>();

        foreach (string levelFilename in levelFilenames)
        {
            LevelData levelData = ParseLevelFile(levelFilename);
            
            LevelButton levelButton = Instantiate(levelButtonPrefab, levelsParent);
            levelButton.LoadWithLevelData(levelData);
            
            _allLevels.Add(levelData);
        }
    }

    public int NextLevelIndex()
    {
        return _allLevels.Count;
    }
    
    private LevelData ParseLevelFile(string filename)
    {
        string path = Path.Combine(LevelsPath, filename);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Level not found: {path}");
            return null;
        }
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<LevelData>(json);
    }
    
    public void SaveLevel(LevelData data)
    {
        string levelFileName = $"level{data.levelIndex}.json";
        string path = Path.Combine(LevelsPath, levelFileName);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"Saved level to {path}");
    }

}
