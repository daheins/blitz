using UnityEngine;

public enum ItemType { None, Player, Wall, Goal, Enemy }

public class GridItem : MonoBehaviour
{
    public ItemType itemType;
}