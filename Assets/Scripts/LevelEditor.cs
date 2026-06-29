using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelEditor : MonoBehaviour, IItemButtonDelegate
{
    public static LevelEditor Instance;
    
    public GridLevel gridLevel;
    public GameObject editPanel;
    public GameObject itemsPanel;

    public ItemButton itemButtonPrefab;

    public TextMeshProUGUI editLabel;
    
    private PieceType _currentSelectedPiece;
    private ItemType _currentSelectedItem = ItemType.None;
    private bool _areItemsLoaded;

    private void Awake()
    {
        Instance = this;
        
        editPanel.SetActive(false);
    }

    public void ToggleEditMode()
    {
        gridLevel.IsInEditMode = !gridLevel.IsInEditMode;
        
        // editLabel.text = $"Edit Mode: {(gridLevel.IsInEditMode ? "On" : "Off")}";
        editPanel.SetActive(gridLevel.IsInEditMode);

        if (_areItemsLoaded)
        {
            foreach (GridItem piece in gridLevel.gridItems)
            {
                ItemButton itemButton = Instantiate(itemButtonPrefab, itemsPanel.transform);
                itemButton.Delegate = this;
                itemButton.LoadItem(piece);
            }
        }
    }

    public void SaveLevel()
    {
        LevelLoader.Instance.SaveLevel(gridLevel.GetLevelData());
    }

    public void CreateNewLevel()
    {
        LevelData levelData = new LevelData();
        levelData.levelIndex = LevelLoader.Instance.LevelCount();
        levelData.levelName = "temp";
        
        gridLevel.SetupGridForLevel(levelData);
    }

    public void PieceModeNone()
    {
        _currentSelectedPiece = PieceType.None;
    }
    
    public void PieceModeWall()
    {
        _currentSelectedPiece = PieceType.Wall;
    }
    
    public void PieceModePlayer()
    {
        _currentSelectedPiece = PieceType.Player;
    }
    
    public void PieceModeGoal()
    {
        _currentSelectedPiece = PieceType.Goal;
    }

    public void DidTapEditCell(GridCell cell)
    {
        gridLevel.PopulateCell(cell, _currentSelectedPiece, _currentSelectedItem);
    }

    public void DidTapItemButton(ItemButton itemButton)
    {
        // Piece Mode: Item
        _currentSelectedPiece = PieceType.Item;
        _currentSelectedItem = itemButton.GridItem.itemType;
    }
}
