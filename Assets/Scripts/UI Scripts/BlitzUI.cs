using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BlitzUI : MonoBehaviour
{
    public GridLevel gridLevel;
    public GameObject victoryNode;
    public LevelLoader levelLoader;

    public InventoryItemIcon itemIconPrefab;
    public GameObject inventoryParent;

    public GameObject moveCounterParent;
    public TextMeshProUGUI moveCounterLabel;
    public TextMeshProUGUI moveTargetLabel;

    private List<InventoryItemIcon> _inventoryIcons = new List<InventoryItemIcon>();
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            GridLevel.GridCommandSystem.Undo();
        }

        if (Input.GetKeyDown(KeyCode.R))
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

        levelLoader.PlayNextLevel();
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

    public void UpdateMoveCounter()
    {
        LevelData levelData = gridLevel.GetLevelData();
        if (levelData.moveTarget <= 0)
        {
            moveCounterParent.SetActive(false);
            return;
        }
        
        moveCounterParent.SetActive(true);
        moveCounterLabel.text = $"{gridLevel.MoveCounter}";
        moveTargetLabel.text = $"{levelData.moveTarget}";
    }
}
