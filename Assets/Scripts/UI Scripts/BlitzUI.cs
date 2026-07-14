using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlitzUI : MonoBehaviour
{
    public const string DefeatReasonSpikes = "You stepped on spikes and died!";
    public const string DefeatReasonPortal = "The portal closed without you!";
    public const string DefeatReasonEnemy = "You got killed by an enemy!";
    public const string DefeatReasonDamage = "You died!";
        
    public static BlitzUI Instance;
    
    public GridLevel gridLevel;
    public GameObject victoryNode;
    public GameObject defeatNode;
    
    // Portal Results
    public GameObject portalResultsNode;
    public TextMeshProUGUI totalPortalLabel;
    public TextMeshProUGUI portalTimeScoreLabel;

    public InventoryItemIcon itemIconPrefab;
    public GameObject inventoryParent;

    public TextMeshProUGUI defeatTextLabel;
    
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
        
        portalResultsNode.SetActive(false);
        
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
    
    public void DisplayPlayerDefeat(string defeatReason = DefeatReasonDamage)
    {
        defeatNode.SetActive(true);

        defeatTextLabel.text = defeatReason;
    }

    public void HideVictoryAndDefeatNodes()
    {
        victoryNode.SetActive(false);
        defeatNode.SetActive(false);
    }
    
    public void DidTapNextLevel()
    {
        victoryNode.SetActive(false);

        if (gridLevel.IsPortalLevel)
        {
            if (SaveStateManager.Instance.IsOnFinalPortalLevel())
            {
                ShowPortalResults();
            }
            else
            {
                SaveStateManager.Instance.PlayNextPortalLevel();
            }
        }
        else
        {
            SaveStateManager.Instance.PlayNextLevel();
        }
    }

    private void ShowPortalResults()
    {
        portalResultsNode.SetActive(true);

        totalPortalLabel.text = $"{SaveStateManager.Instance.GetManifestLevels(true).Count}";
        portalTimeScoreLabel.text = $"{SaveStateManager.Instance.GetPortalTimeScore()}s";
    }

    public void DidTapPortalChallengeOverButton()
    {
        MenuViewManager.Instance.GoToHomeScreen();
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
        MenuViewManager.Instance.GoToLevelSelectScreen();
    }

    public void UpdateMoveCounter()
    {
        gridLevel.PortalGoal?.UpdatePortal(gridLevel);
    }

    public void UpdateUndoAndRestartState()
    {
        undoAndRestartInfoNode.SetActive(SaveStateManager.Instance.PlayerSaveState.FeatureUnlockUndoAndRestart);
    }
}
