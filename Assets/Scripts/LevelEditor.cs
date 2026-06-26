using System;
using TMPro;
using UnityEngine;

public class LevelEditor : MonoBehaviour
{
    public static LevelEditor Instance;
    
    public GridLevel gridLevel;
    public GameObject editPanel;

    public TextMeshProUGUI editLabel;
    
    private ItemType _currentSelectedItem = ItemType.None;

    private void Start()
    {
        Instance = this;
    }

    private void Awake()
    {
        editPanel.SetActive(false);
    }

    public void ToggleEditMode()
    {
        gridLevel.IsInEditMode = !gridLevel.IsInEditMode;
        
        // editLabel.text = $"Edit Mode: {(gridLevel.IsInEditMode ? "On" : "Off")}";
        editPanel.SetActive(gridLevel.IsInEditMode);
    }

    public void SaveLevel()
    {
        LevelSelector.Instance.SaveLevel(gridLevel.GetLevelData());
    }

    public void CreateNewLevel()
    {
        LevelData levelData = new LevelData();
        levelData.levelIndex = LevelSelector.Instance.NextLevelIndex();
        levelData.levelName = "temp";
        
        gridLevel.SetupGridForLevel(levelData);
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
    
    public void ItemModeGoal()
    {
        _currentSelectedItem = ItemType.Goal;
    }

    public void DidTapEditCell(GridCell cell)
    {
        gridLevel.PopulateCell(cell, _currentSelectedItem);
    }
}
