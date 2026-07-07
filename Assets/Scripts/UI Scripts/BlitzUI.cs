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
    private Dictionary<string, LevelButton> _levelButtonsByIdentifier;
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
        _levelButtonsByIdentifier = new();
        
        foreach (var levelData in SaveStateManager.Instance.AllLevelDatas)
        {
            LevelButton levelButton = Instantiate(levelButtonPrefab, levelsParent);
            levelButton.LoadWithLevelData(levelData);

            _levelButtonsByIdentifier[levelData.levelIdentifier] = levelButton;
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
            gridLevel.GridCommandSystem.Undo();
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
        
        foreach (var pair in _levelButtonsByIdentifier)
        {
            LevelData levelData = pair.Value.LevelData;
            var levelState = playerSaveState.LevelProgressStates[pair.Key];
            
            int moveTarget = levelData.moveTarget;
            bool isPerfect = moveTarget == levelState.highScore;
            pair.Value.UpdateState(levelState.isComplete, isPerfect);
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
        
        gridLevel.PortalGoal?.UpdatePortal(gridLevel);
        
        moveCounterParent.SetActive(true);
        moveCounterLabel.text = $"{gridLevel.MoveCounter}";
        moveTargetLabel.text = $"{levelData.moveTarget}";
    }

    public void UpdateUndoAndRestartState()
    {
        undoAndRestartInfoNode.SetActive(SaveStateManager.Instance.PlayerSaveState.FeatureUnlockUndoAndRestart);
    }
}
