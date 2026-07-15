using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

public class SaveStateManager : MonoBehaviour
{
    public static SaveStateManager Instance;
    
    public GridLevel gridLevel;
    
    private static string LevelsPath => Path.Combine(Application.dataPath, "Resources", LevelsResourceString);
    private static string ManifestPath => Path.Combine(Application.dataPath, "Resources", "level_manifest.json");
    private const string LevelsResourceString = "Levels";
    private const string ManifestResourceString = "level_manifest";
    
    private static string PortalManifestPath => Path.Combine(Application.dataPath, "Resources", "portal_level_manifest.json");
    private const string PortalManifestResourceString = "portal_level_manifest";
    
    private const string PrefsSaveState = "SaveState";

    public PlayerSaveState PlayerSaveState { get; private set; }
    public Dictionary<string, LevelData> LevelDatas { get; private set; }

    private LevelManifest _levelManifest;
    private List<LevelData> _allManifestLevels;
    
    private LevelManifest _portalLevelManifest;
    private List<LevelData> _allPortalLevels;
    private int _currentPortalIndex = -1;

    private void Awake()
    {
        Instance = this;

        LoadLevelManifest(ManifestPath, ManifestResourceString, out _levelManifest);
        LoadAllLevelPaths();
        LoadSaveState();
    }

    public void ReloadFromManifest()
    {
        LoadAllLevelPaths();
        LoadSaveState();
    }

    private void LoadSaveState()
    {
        PlayerSaveState saveState;
        if (PlayerPrefs.HasKey(PrefsSaveState))
        {
            string json = PlayerPrefs.GetString(PrefsSaveState);
            saveState = JsonConvert.DeserializeObject<PlayerSaveState>(json);
        }
        else
        {
            saveState = new PlayerSaveState();
        }
        
        bool didModifySaveState = false;
        foreach (LevelData levelData in _allManifestLevels)
        {
            if (!saveState.LevelProgressStates.ContainsKey(levelData.levelIdentifier))
            {
                saveState.LevelProgressStates[levelData.levelIdentifier] = new();
                didModifySaveState = true;
            }
        }
        
        PlayerSaveState = saveState;

        if (didModifySaveState)
        {
            WriteSaveState();
        }
    }

    public List<LevelData> GetManifestLevels(bool portal = false)
    {
        if (portal) return _allPortalLevels;

        return _allManifestLevels;
    }

    private void LoadLevelManifest(string path, string resourceString, out LevelManifest manifest)
    {
#if UNITY_EDITOR
        string manifestJson = File.ReadAllText(path);
        manifest = JsonConvert.DeserializeObject<LevelManifest>(manifestJson);
#else
        TextAsset manifestAsset = Resources.Load<TextAsset>(resourceString);
        manifest = JsonConvert.DeserializeObject<LevelManifest>(manifestAsset.text);
#endif
    }

    private void LoadAllLevelPaths()
    {
        LevelDatas = new();
#if UNITY_EDITOR
        string[] filePaths = Directory.GetFiles(LevelsPath, "*.json");
        foreach (string filePath in filePaths)
        {
            string json = File.ReadAllText(filePath);
            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json);
            levelData.Filename = Path.GetFileNameWithoutExtension(filePath);
            LevelDatas[levelData.levelIdentifier] = levelData;
        }
#else
        TextAsset[] levelFiles = Resources.LoadAll<TextAsset>("Levels");
        foreach (TextAsset levelFile in levelFiles)
        {
            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(levelFile.text);
            levelData.Filename = levelFile.name;
            LevelDatas[levelData.levelIdentifier] = levelData;
        }
#endif
        
        _allManifestLevels = new();
        foreach (string id in _levelManifest.levelIdentifiers)
        {
            if (LevelDatas.TryGetValue(id, out LevelData levelData))
                _allManifestLevels.Add(levelData);
            else
                Debug.LogWarning($"Manifest references unknown level: {id}");
        }
    }

    public void AddLevelToManifest(string levelIdentifier)
    {
        _levelManifest.levelIdentifiers.Add(levelIdentifier);
        
        string json = JsonConvert.SerializeObject(_levelManifest, Formatting.Indented);
        File.WriteAllText(ManifestPath, json);
    }

    public void PlayNextLevel()
    {
        string currentIdentifier = gridLevel.GetLevelData().levelIdentifier;
        int currentIndex = _allManifestLevels.FindIndex(l => l.levelIdentifier == currentIdentifier);

        int nextIndex = currentIndex + 1;
        if (nextIndex >= _allManifestLevels.Count)
            nextIndex = 0;

        PlayLevelAtIndex(nextIndex);
    }

    private void PlayLevelAtIndex(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _allManifestLevels.Count)
        {
            Debug.LogError($"Trying to load level #{levelIndex} but there are {_allManifestLevels.Count} different levels!");
            return;
        }
        
        LevelData levelData = _allManifestLevels[levelIndex];
        
        gridLevel.SetupGridForLevel(levelData);
    }

    public void LoadPortalManifest()
    {
        LoadLevelManifest(PortalManifestPath, PortalManifestResourceString, out _portalLevelManifest);

        _allPortalLevels = new();
        foreach (string id in _portalLevelManifest.levelIdentifiers)
        {
            if (LevelDatas.TryGetValue(id, out LevelData levelData))
                _allPortalLevels.Add(levelData);
            else
                Debug.LogWarning($"Portal manifest references unknown level: {id}");
        }
    }

    public void StartPortalChallenge()
    {
        LoadPortalManifest();

        _currentPortalIndex = 0;

        LevelData levelData = _allPortalLevels[_currentPortalIndex];
        gridLevel.SetupGridForLevel(levelData, true);

        BlitzUI.Instance.StartPortalTimer();
    }

    public void PlayNextPortalLevel()
    {
        _currentPortalIndex++;
        
        LevelData levelData = _allPortalLevels[_currentPortalIndex];
        
        gridLevel.SetupGridForLevel(levelData, true);
    }

    public void UpdateLevelWithChanges(LevelData data, LevelData updatedLevel)
    {
        string json = JsonConvert.SerializeObject(updatedLevel);
        JsonConvert.PopulateObject(json, data);
    }
    
    public void SaveLevel(LevelData data)
    {
        string levelFileName = $"{data.Filename}.json";
        string path = Path.Combine(LevelsPath, levelFileName);
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
        Debug.Log($"Saved level {levelFileName} to {path}. Data: {json}");
    }
    
    public void SetLevelState(string levelIdentifier, bool isComplete, int highScore, bool isPerfect)
    {
        LevelState levelState = PlayerSaveState.LevelProgressStates[levelIdentifier];
        levelState.isComplete = isComplete;
        if (highScore < levelState.highScore)
        {
            levelState.highScore = highScore;
        }
        
        CheckFeatureUnlocks();
        
        WriteSaveState();
    }

    private void CheckFeatureUnlocks()
    {
        int totalLevelsComplete = PlayerSaveState.LevelProgressStates.Values.Count(state => state.isComplete);

        if (totalLevelsComplete >= PlayerSaveState.KLevelsToUnlockHighScores)
        {
            PlayerSaveState.FeatureUnlockHighScores = true;
        }
        
        if (totalLevelsComplete >= PlayerSaveState.KLevelsToUnlockUndoAndRestart)
        {
            PlayerSaveState.FeatureUnlockUndoAndRestart = true;
        }

        if (totalLevelsComplete >= _allManifestLevels.Count)
        {
            PlayerSaveState.FeatureUnlockPortalMode = true;
        }
    }

    public bool IsOnFinalPortalLevel()
    {
        return _currentPortalIndex == _allPortalLevels.Count - 1;
    }

    private void WriteSaveState()
    {
        string json = JsonConvert.SerializeObject(PlayerSaveState, Formatting.Indented);
        PlayerPrefs.SetString(PrefsSaveState, json);
    }
}
