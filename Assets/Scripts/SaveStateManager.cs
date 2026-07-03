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
        else
        {
            var firstIncompleteLevel = PlayerSaveState.LevelProgressStates
                .OrderBy(pair => pair.Key)
                .FirstOrDefault(pair => !pair.Value.isComplete);

            if (firstIncompleteLevel.Value != null)
            {
                levelIndexToStart = firstIncompleteLevel.Key;
            }
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
        TextAsset[] levelFiles = Resources.LoadAll<TextAsset>("Levels");
        List<TextAsset> sortedFiles = levelFiles.OrderBy(f => f.name).ToList();

        AllLevelDatasByIndex = new();

        foreach (TextAsset levelFile in sortedFiles)
        {
            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(levelFile.text);
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
    
    public void SaveLevel(LevelData data)
    {
        Debug.LogWarning("SAVING LEVEL");
        string levelFileName = $"level{data.levelIndex}.json";
        string path = Path.Combine(LevelsPath, levelFileName);
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
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
