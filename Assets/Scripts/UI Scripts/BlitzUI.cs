using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BlitzUI : MonoBehaviour
{
    public static BlitzUI Instance;
    
    public GridLevel gridLevel;
    public GameObject victoryNode;

    public InventoryItemIcon itemIconPrefab;
    public GameObject inventoryParent;

    public GameObject moveCounterParent;
    public TextMeshProUGUI moveCounterLabel;
    public TextMeshProUGUI moveTargetLabel;
    
    public GameObject undoAndRestartInfoNode;
    
    // Levels
    public Transform levelsParent;
    public Transform levelsScreen;
    public LevelButton levelButtonPrefab;
    public GameObject perfectLevelExplanation;
    private Dictionary<int, LevelButton> _allLevelButtonsByIndex;
    // Levels end

    private List<InventoryItemIcon> _inventoryIcons = new List<InventoryItemIcon>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        levelsScreen.gameObject.SetActive(false);

        CreateLevelButtons();
        UpdateAllLevelButtons();
    }

    private void CreateLevelButtons()
    {
        _allLevelButtonsByIndex = new();
        
        foreach (var levelData in SaveStateManager.Instance.AllLevelDatasByIndex.Values)
        {
            LevelButton levelButton = Instantiate(levelButtonPrefab, levelsParent);
            levelButton.LoadWithLevelData(levelData);

            _allLevelButtonsByIndex[levelData.levelIndex] = levelButton;
        }
    }

    public void StartGridLevel()
    {
        ClearInventoryItemIcons();
        UpdateMoveCounter();
        UpdateUndoAndRestartState();
    }
    
    
    public void ToggleLevels()
    {
        levelsScreen.gameObject.SetActive(!levelsScreen.gameObject.activeSelf);
        perfectLevelExplanation.SetActive(SaveStateManager.Instance.PlayerSaveState.FeatureUnlockHighScores);

        if (levelsScreen.gameObject.activeSelf)
        {
            UpdateAllLevelButtons();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z) && SaveStateManager.Instance.PlayerSaveState.FeatureUnlockUndoAndRestart)
        {
            GridLevel.GridCommandSystem.Undo();
        }

        if (Input.GetKeyDown(KeyCode.R) && SaveStateManager.Instance.PlayerSaveState.FeatureUnlockUndoAndRestart)
        {
            gridLevel.RestartLevel();
        }
    }
    
    public void DisplayPlayerVictory()
    {
        victoryNode.SetActive(true);
    }
    
    public void DidTapNextLevel()
    {
        victoryNode.SetActive(false);

        SaveStateManager.Instance.PlayNextLevel();
    }
    
    private void UpdateAllLevelButtons()
    {
        PlayerSaveState playerSaveState = SaveStateManager.Instance.PlayerSaveState;
        
        foreach (int levelIndex in _allLevelButtonsByIndex.Keys)
        {
            var levelState = playerSaveState.LevelProgressStates[levelIndex];
            
            int moveTarget = SaveStateManager.Instance.AllLevelDatasByIndex[levelIndex].moveTarget;
            bool isPerfect = moveTarget == levelState.highScore;
            _allLevelButtonsByIndex[levelIndex].UpdateState(levelState.isComplete, isPerfect);
        }
    }

    public void AddInventoryItemIcon(GridPiece gridPiece)
    {
        InventoryItemIcon inventoryItemIcon = Instantiate(itemIconPrefab, inventoryParent.transform);
        inventoryItemIcon.DisplayItem(gridPiece);
        _inventoryIcons.Add(inventoryItemIcon);
    }

    public void RemoveInventoryItemIcon(ItemType itemType)
    {
        InventoryItemIcon icon = _inventoryIcons.First(item => item.ItemType == itemType);
        _inventoryIcons.Remove(icon);
        Destroy(icon.gameObject);
    }

    public void ClearInventoryItemIcons()
    {
        _inventoryIcons.ForEach(icon => Destroy(icon.gameObject));
        _inventoryIcons.Clear();
    }

    public void UpdateMoveCounter()
    {
        LevelData levelData = gridLevel.GetLevelData();
        if (levelData.moveTarget <= 0 || !SaveStateManager.Instance.PlayerSaveState.FeatureUnlockHighScores)
        {
            moveCounterParent.SetActive(false);
            return;
        }
        
        moveCounterParent.SetActive(true);
        moveCounterLabel.text = $"{gridLevel.MoveCounter}";
        moveTargetLabel.text = $"{levelData.moveTarget}";
    }

    public void UpdateUndoAndRestartState()
    {
        undoAndRestartInfoNode.SetActive(SaveStateManager.Instance.PlayerSaveState.FeatureUnlockUndoAndRestart);
    }
}
