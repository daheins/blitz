using System;
using TMPro;
using UnityEngine;

public class LevelEditor : MonoBehaviour
{
    public static LevelEditor Editor;
    
    public GridLevel gridLevel;
    public GameObject editPanel;

    public TextMeshProUGUI editLabel;
    
    private ItemType _currentSelectedItem = ItemType.None;

    private void Start()
    {
        Editor = this;
    }

    private void Awake()
    {
        editPanel.SetActive(false);
    }

    public void ToggleEditMode()
    {
        gridLevel.IsInEditMode = !gridLevel.IsInEditMode;
        
        editLabel.text = $"Edit Mode: {(gridLevel.IsInEditMode ? "On" : "Off")}";
        editPanel.SetActive(gridLevel.IsInEditMode);
    }

    public void SaveLevel()
    {
        LevelData.Save(gridLevel.GetLevelData());
    }

    public void ItemModeNone()
    {
        _currentSelectedItem = ItemType.None;
    }
    
    public void ItemModeWall()
    {
        _currentSelectedItem = ItemType.Wall;
    }
    
    public void ItemModePlayer()
    {
        _currentSelectedItem = ItemType.Player;
    }

    public void DidTapEditCell(GridCell cell)
    {
        gridLevel.PopulateCell(cell, _currentSelectedItem);
    }

    // public void DidDragIntoCell(GridCell cell)
    // {
    //     
    // }
}
