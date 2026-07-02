using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

public class SaveStateManager : MonoBehaviour
{
    public static SaveStateManager Instance;
    
    public GridLevel gridLevel;
    
    private static string LevelsPath => Path.Combine(Application.dataPath, "Levels");
    private static string SaveStatePath => Path.Combine(Application.dataPath, "SaveState");

    public PlayerSaveState PlayerSaveState { get; private set; }
    public Dictionary<int, LevelData> AllLevelDatasByIndex;

    private void Start()
    {
        Instance = this;

        LoadAllLevelPaths();
        
        LoadSaveState();

        int levelIndexToStart = 0;
        if (DevelopmentTools.Instance.startAtLastLevel)
        {
            levelIndexToStart = LevelCount() - 1;
        }
        PlayLevelAtIndex(levelIndexToStart);
    }

    private void LoadSaveState()
    {
        PlayerSaveState saveState;
        if (!File.Exists(SaveStatePath))
        {
            saveState = new PlayerSaveState();
        }
        else
        {
            string json = File.ReadAllText(SaveStatePath);
            saveState = JsonConvert.DeserializeObject<PlayerSaveState>(json);
        }
        
        bool didModifySaveState = false;
        foreach (var pair in AllLevelDatasByIndex)
        {
            if (!saveState.LevelProgressStates.ContainsKey(pair.Key))
            {
                saveState.LevelProgressStates[pair.Key] = new LevelState();
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
        List<string> levelFilenames = new List<string>(Directory.GetFiles(LevelsPath, "*.json"));
        levelFilenames.Sort();
        Debug.Log($"i found {levelFilenames.Count} levels");

        AllLevelDatasByIndex = new();

        foreach (string levelFilename in levelFilenames)
        {
            LevelData levelData = ParseLevelFile(levelFilename);
            levelData.FixCellsLength();
            
            AllLevelDatasByIndex[levelData.levelIndex] = levelData;
        }
    }

    public void PlayNextLevel()
    {
        int nextIndex = gridLevel.GetLevelData().levelIndex + 1;
        if (nextIndex >= AllLevelDatasByIndex.Count)
            nextIndex = 0;
        PlayLevelAtIndex(nextIndex);
    }

    private void PlayLevelAtIndex(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= AllLevelDatasByIndex.Count)
        {
            Debug.LogError($"Trying to load level #{levelIndex} but there are {AllLevelDatasByIndex.Count} different levels!");
            return;
        }
        
        LevelData levelData = AllLevelDatasByIndex[levelIndex];
        
        gridLevel.SetupGridForLevel(levelData);
    }

    public int LevelCount()
    {
        return AllLevelDatasByIndex.Count;
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
        Debug.LogWarning("SAVING LEVEL");
        string levelFileName = $"level{data.levelIndex}.json";
        string path = Path.Combine(LevelsPath, levelFileName);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"Saved level to {path}");
    }
    
    public void SetLevelState(int levelIndex, bool isComplete, int highScore, bool isPerfect)
    {
        LevelState levelState = PlayerSaveState.LevelProgressStates[levelIndex];
        levelState.isComplete = isComplete;
        levelState.highScore = highScore;
        
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
    }

    private void WriteSaveState()
    {
        string json = JsonConvert.SerializeObject(PlayerSaveState, Formatting.Indented);
        File.WriteAllText(SaveStatePath, json);
    }
}
