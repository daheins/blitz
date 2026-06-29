using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum ItemType { None, Spring, }

public class GridItem : MonoBehaviour
{
    public string itemIdentifier;
    public string itemDisplayName;
    public ItemType itemType;
    public SpriteRenderer itemSprite;
    public GridPiece GridPiece { get; private set; }

    public void Awake()
    {
        GridPiece = GetComponent<GridPiece>();
    }
}
