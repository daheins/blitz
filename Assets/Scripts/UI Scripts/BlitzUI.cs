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
    public GameObject defeatNode;

    public InventoryItemIcon itemIconPrefab;
    public GameObject inventoryParent;

    // public GameObject moveCounterParent;
    // public TextMeshProUGUI moveCounterLabel;
    // public TextMeshProUGUI moveTargetLabel;
    
    public GameObject undoAndRestartInfoNode;

    private List<InventoryItemIcon> _inventoryIcons = new List<InventoryItemIcon>();

    private void Awake()
    {
        Instance = this;
    }

    public void StartGridLevel()
    {
        victoryNode.SetActive(false);
        defeatNode.SetActive(false);
        
        ClearInventoryItemIcons();
        UpdateMoveCounter();
        UpdateUndoAndRestartState();
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
    
    public void DisplayPlayerDefeat()
    {
        defeatNode.SetActive(true);
    }

    public void HideVictoryAndDefeatNodes()
    {
        victoryNode.SetActive(false);
        defeatNode.SetActive(false);
    }
    
    public void DidTapNextLevel()
    {
        victoryNode.SetActive(false);

        SaveStateManager.Instance.PlayNextLevel();
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

    public void TapLevelsButton()
    {
        MenuViewManager.Instance.ToggleLevels();
    }

    public void UpdateMoveCounter()
    {
        // LevelData levelData = gridLevel.GetLevelData();
        // if (levelData.moveTarget <= 0 || !SaveStateManager.Instance.PlayerSaveState.FeatureUnlockHighScores)
        // {
        //     moveCounterParent.SetActive(false);
        //     return;
        // }
        //
        // gridLevel.PortalGoal?.UpdatePortal(gridLevel);
        //
        // moveCounterParent.SetActive(true);
        // moveCounterLabel.text = $"{gridLevel.MoveCounter}";
        // moveTargetLabel.text = $"{levelData.moveTarget}";
    }

    public void UpdateUndoAndRestartState()
    {
        undoAndRestartInfoNode.SetActive(SaveStateManager.Instance.PlayerSaveState.FeatureUnlockUndoAndRestart);
    }
}
