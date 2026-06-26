using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IItemButtonDelegate
{
    public void DidTapItemButton(ItemButton itemButton);
}

public class ItemButton : MonoBehaviour
{
    public TextMeshProUGUI itemButtonLabel;
    public Image itemButtonImage;
    
    public IItemButtonDelegate Delegate;
    
    public GridItem GridItem { get; private set; }

    public void LoadItem(GridItem gridItem)
    {
        itemButtonLabel.text = gridItem.itemDisplayName;
        itemButtonImage.sprite = gridItem.itemSprite.sprite;

        GridItem = gridItem;
    }

    public void DidTapItemButton()
    {
        Delegate.DidTapItemButton(this);
    }
}
