using UnityEngine;
using UnityEngine.Serialization;

public enum ItemType { Spring, }

public class GridItem : MonoBehaviour
{
    public string itemIdentifier;
    public string itemDisplayName;
    public ItemType itemType;
    public SpriteRenderer itemSprite;
}
