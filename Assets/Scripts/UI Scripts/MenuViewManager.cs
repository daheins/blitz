using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuViewManager : MonoBehaviour
{
    public static MenuViewManager Instance;
    
    public GridLevel gridLevel;

    public Transform menuMain;
    public Transform menuLevelSelect;
    public GameObject portalLockedNode;
    
    // Levels
    public Transform levelsParent;
    public LevelButton levelButtonPrefab;
    private List<LevelButton> _levelButtons;
    // Levels end

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        HideAllMenus();
        
        GoToHomeScreen();

        CreateLevelButtons();
    }
    
    private void CreateLevelButtons()
    {
        _levelButtons = new();
        
        foreach (var levelData in SaveStateManager.Instance.GetManifestLevels())
        {
            LevelButton levelButton = Instantiate(levelButtonPrefab, levelsParent);
            levelButton.LoadWithLevelData(levelData);

            _levelButtons.Add(levelButton);
        }
    }

    public void GoToHomeScreen()
    {
        HideAllMenus();
        
        menuMain.gameObject.SetActive(true);

        portalLockedNode.SetActive(true);

        if (SaveStateManager.Instance.PlayerSaveState.FeatureUnlockPortalMode)
        {
            portalLockedNode.SetActive(false);
        }
    }
    
    public void GoToLevelSelectScreen()
    {
        HideAllMenus();
        
        menuLevelSelect.gameObject.SetActive(true);
        UpdateAllLevelButtons();
    }

    public void GoToPortalMode()
    {
        if (!SaveStateManager.Instance.PlayerSaveState.FeatureUnlockPortalMode) return;
        
        HideAllMenus();
        
        SaveStateManager.Instance.StartPortalChallenge();
    }
    
    private void UpdateAllLevelButtons()
    {
        PlayerSaveState playerSaveState = SaveStateManager.Instance.PlayerSaveState;

        bool foundUnbeatenLevel = false;
        foreach (LevelButton levelButton in _levelButtons)
        {
            if (foundUnbeatenLevel)
            {
                levelButton.UpdateState(false, true);
                continue;
            }
            
            bool isComplete = playerSaveState.LevelProgressStates[levelButton.LevelData.levelIdentifier].isComplete;
            levelButton.UpdateState(isComplete, false);

            if (!isComplete)
                foundUnbeatenLevel = true;
        }
    }

    public void DisplayGridLevel(LevelData levelData)
    {
        HideAllMenus();
        
        gridLevel.SetupGridForLevel(levelData);
    }
    

    private void HideAllMenus()
    {
        menuMain.gameObject.SetActive(false);
        menuLevelSelect.gameObject.SetActive(false);
    }
}
