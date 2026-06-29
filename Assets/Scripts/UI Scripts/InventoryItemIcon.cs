using UnityEngine;
using UnityEngine.UI;

public class InventoryItemIcon : MonoBehaviour
{
    public ItemType ItemType { get; private set; }
    
    public Image itemIconImage;
    
    public void DisplayItem(GridItem gridItem)
    {
        itemIconImage.sprite = gridItem.itemSprite.sprite;

        ItemType = gridItem.itemType;
    }
}
