using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlitzUI : MonoBehaviour
{
    public GameObject victoryNode;
    public LevelLoader levelLoader;

    public InventoryItemIcon itemIconPrefab;
    public GameObject inventoryParent;

    private List<InventoryItemIcon> _inventoryIcons = new List<InventoryItemIcon>();

    public void DisplayPlayerVictory()
    {
        victoryNode.SetActive(true);
    }
    
    public void DidTapNextLevel()
    {
        victoryNode.SetActive(false);

        levelLoader.PlayNextLevel();
    }

    public void AddInventoryItemIcon(GridItem gridItem)
    {
        InventoryItemIcon inventoryItemIcon = Instantiate(itemIconPrefab, inventoryParent.transform);
        inventoryItemIcon.DisplayItem(gridItem);
        _inventoryIcons.Add(inventoryItemIcon);
    }

    public void RemoveInventoryItemIcon(ItemType itemType)
    {
        InventoryItemIcon icon = _inventoryIcons.First(item => item.ItemType == itemType);
        _inventoryIcons.Remove(icon);
        Destroy(icon.gameObject);
    }
}
