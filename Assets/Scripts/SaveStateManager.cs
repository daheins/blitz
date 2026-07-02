using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SaveStateManager : MonoBehaviour
{
    public static SaveStateManager Instance;
    
    public GridLevel gridLevel;
    public Transform levelsParent;
    public Transform levelsScreen;
    public LevelButton levelButtonPrefab;
    
    private static string LevelsPath => Path.Combine(Application.dataPath, "Levels");
    private static string SaveStatePath => Path.Combine(Application.dataPath, "SaveState");

    private PlayerSaveState _playerSaveState;
    private Dictionary<int, LevelData> _allLevelsByIndex;
    private Dictionary<int, LevelButton> _allLevelButtonsByIndex;

    private void Start()
    {
        Instance = this;

        levelsScreen.gameObject.SetActive(false);

        LoadAllLevelPaths();
        
        LoadSaveState();

        UpdateAllLevelButtons();

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
        foreach (var pair in _allLevelsByIndex)
        {
            if (!saveState.LevelProgressStates.ContainsKey(pair.Key))
            {
                saveState.LevelProgressStates[pair.Key] = new LevelState();
                didModifySaveState = true;
            }
        }
        
        _playerSaveState = saveState;

        if (didModifySaveState)
        {
            WriteSaveState();
        }
    }

    private void UpdateAllLevelButtons()
    {
        foreach (int levelIndex in _allLevelButtonsByIndex.Keys)
        {
            var levelState = _playerSaveState.LevelProgressStates[levelIndex];
            
            int moveTarget = _allLevelsByIndex[levelIndex].moveTarget;
            bool isPerfect = moveTarget == levelState.highScore;
            _allLevelButtonsByIndex[levelIndex].UpdateState(levelState.isComplete, isPerfect);
        }
    }
    
    public void ToggleLevels()
    {
        levelsScreen.gameObject.SetActive(!levelsScreen.gameObject.activeSelf);
    }

    private void LoadAllLevelPaths()
    {
        List<string> levelFilenames = new List<string>(Directory.GetFiles(LevelsPath, "*.json"));
        levelFilenames.Sort();
        Debug.Log($"i found {levelFilenames.Count} levels");

        _allLevelsByIndex = new();
        _allLevelButtonsByIndex = new();

        foreach (string levelFilename in levelFilenames)
        {
            LevelData levelData = ParseLevelFile(levelFilename);
            levelData.FixCellsLength();
            
            LevelButton levelButton = Instantiate(levelButtonPrefab, levelsParent);
            levelButton.LoadWithLevelData(levelData);

            _allLevelButtonsByIndex[levelData.levelIndex] = levelButton;
            _allLevelsByIndex[levelData.levelIndex] = levelData;
        }
    }

    public void PlayNextLevel()
    {
        int nextIndex = gridLevel.GetLevelData().levelIndex + 1;
        if (nextIndex >= _allLevelsByIndex.Count)
            nextIndex = 0;
        PlayLevelAtIndex(nextIndex);
    }

    private void PlayLevelAtIndex(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _allLevelsByIndex.Count)
        {
            Debug.LogError($"Trying to load level #{levelIndex} but there are {_allLevelsByIndex.Count} different levels!");
            return;
        }
        
        LevelData levelData = _allLevelsByIndex[levelIndex];
        
        gridLevel.SetupGridForLevel(levelData);
    }

    public int LevelCount()
    {
        return _allLevelsByIndex.Count;
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
        LevelState levelState = _playerSaveState.LevelProgressStates[levelIndex];
        levelState.isComplete = isComplete;
        levelState.highScore = highScore;
        WriteSaveState();
        
        _allLevelButtonsByIndex[levelIndex].UpdateState(isComplete, isPerfect);
    }

    private void WriteSaveState()
    {
        string json = JsonConvert.SerializeObject(_playerSaveState, Formatting.Indented);
        File.WriteAllText(SaveStatePath, json);
    }
}
