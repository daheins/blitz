using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuViewManager : MonoBehaviour
{
    public static MenuViewManager Instance;
    
    public GridLevel gridLevel;
    
    // Levels
    public Transform levelsParent;
    public Transform levelsScreen;
    public LevelButton levelButtonPrefab;
    private Dictionary<string, LevelButton> _levelButtonsByIdentifier;
    // Levels end

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        levelsScreen.gameObject.SetActive(false);

        CreateLevelButtons();
    }
    
    private void CreateLevelButtons()
    {
        _levelButtonsByIdentifier = new();
        
        foreach (var levelData in SaveStateManager.Instance.AllLevelDatas)
        {
            LevelButton levelButton = Instantiate(levelButtonPrefab, levelsParent);
            levelButton.LoadWithLevelData(levelData);

            _levelButtonsByIdentifier[levelData.levelIdentifier] = levelButton;
        }
    }
    
    public void ToggleLevels()
    {
        levelsScreen.gameObject.SetActive(!levelsScreen.gameObject.activeSelf);

        if (levelsScreen.gameObject.activeSelf)
        {
            UpdateAllLevelButtons();
        }
    }
    
    private void UpdateAllLevelButtons()
    {
        PlayerSaveState playerSaveState = SaveStateManager.Instance.PlayerSaveState;
        
        foreach (var pair in _levelButtonsByIdentifier)
        {
            LevelData levelData = pair.Value.LevelData;
            var levelState = playerSaveState.LevelProgressStates[pair.Key];
            
            int moveTarget = levelData.moveTarget;
            bool isPerfect = moveTarget == levelState.highScore;
            pair.Value.UpdateState(levelState.isComplete, isPerfect);
        }
    }

    public void DisplayGridLevel(LevelData levelData)
    {
        HideAllMenus();
        
        gridLevel.SetupGridForLevel(levelData);
    }

    private void HideAllMenus()
    {
        levelsScreen.gameObject.SetActive(false);
    }
}
