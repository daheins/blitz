using UnityEngine;
using UnityEngine.UI;

public class InventoryItemIcon : MonoBehaviour
{
    public ItemType ItemType { get; private set; }
    
    public Image itemIconImage;
    
    public void DisplayItem(GridPiece gridPiece)
    {
        itemIconImage.sprite = gridPiece.sprite.sprite;

        ItemType = gridPiece.itemType;
    }
}
