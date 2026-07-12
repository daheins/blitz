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
    
    private static string LevelsPath => Path.Combine(Application.dataPath, "Resources", "Levels");
    private static string PrefsSaveState = "SaveState";

    public PlayerSaveState PlayerSaveState { get; private set; }
    public List<LevelData> AllLevelDatas { get; private set; }

    private void Awake()
    {
        Instance = this;
        
        LoadAllLevelPaths();
        
        LoadSaveState();
    }

    private void Start()
    {
        int levelIndexToStart = 0;
        if (DevelopmentTools.Instance.startAtLastLevel)
        {
            levelIndexToStart = LevelCount() - 1;
        }
        else
        {
            // Claude did this, it's messy but works for now
            int firstIncomplete = AllLevelDatas.FindIndex(levelData => 
                PlayerSaveState.LevelProgressStates.TryGetValue(levelData.levelIdentifier, out LevelState state) && !state.isComplete);

            if (firstIncomplete != -1)
                levelIndexToStart = firstIncomplete;
        }
        PlayLevelAtIndex(levelIndexToStart);
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
        foreach (LevelData levelData in AllLevelDatas)
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

    private void LoadAllLevelPaths()
    {
        TextAsset manifestAsset = Resources.Load<TextAsset>("level_manifest");
        LevelManifest manifest = JsonConvert.DeserializeObject<LevelManifest>(manifestAsset.text);

        Dictionary<string, LevelData> levelsByIdentifier = new();
#if UNITY_EDITOR
        string[] filePaths = Directory.GetFiles(LevelsPath, "*.json");
        foreach (string filePath in filePaths)
        {
            string json = File.ReadAllText(filePath);
            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json);
            levelData.Filename = Path.GetFileNameWithoutExtension(filePath);
            levelsByIdentifier[levelData.levelIdentifier] = levelData;
        }
#else
        TextAsset[] levelFiles = Resources.LoadAll<TextAsset>("Levels");
        foreach (TextAsset levelFile in levelFiles)
        {
            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(levelFile.text);
            levelData.Filename = levelFile.name;
            levelsByIdentifier[levelData.levelIdentifier] = levelData;
        }
#endif

        AllLevelDatas = new();
        foreach (string id in manifest.levelIdentifiers)
        {
            if (levelsByIdentifier.TryGetValue(id, out LevelData levelData))
                AllLevelDatas.Add(levelData);
            else
                Debug.LogWarning($"Manifest references unknown level: {id}");
        }

    }

    public void PlayNextLevel()
    {
        string currentIdentifier = gridLevel.GetLevelData().levelIdentifier;
        int currentIndex = AllLevelDatas.FindIndex(l => l.levelIdentifier == currentIdentifier);

        int nextIndex = currentIndex + 1;
        if (nextIndex >= AllLevelDatas.Count)
            nextIndex = 0;

        PlayLevelAtIndex(nextIndex);
    }

    private void PlayLevelAtIndex(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= AllLevelDatas.Count)
        {
            Debug.LogError($"Trying to load level #{levelIndex} but there are {AllLevelDatas.Count} different levels!");
            return;
        }
        
        LevelData levelData = AllLevelDatas[levelIndex];
        
        gridLevel.SetupGridForLevel(levelData);
    }

    public int LevelCount()
    {
        return AllLevelDatas.Count;
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
    }

    private void WriteSaveState()
    {
        string json = JsonConvert.SerializeObject(PlayerSaveState, Formatting.Indented);
        PlayerPrefs.SetString(PrefsSaveState, json);
    }
}
