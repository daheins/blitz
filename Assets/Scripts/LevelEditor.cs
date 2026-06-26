using System;
using TMPro;
using UnityEngine;

public class LevelEditor : MonoBehaviour
{
    public static LevelEditor Instance;
    
    public GridLevel gridLevel;
    public GameObject editPanel;

    public TextMeshProUGUI editLabel;
    
    private PieceType _currentSelectedPiece = PieceType.None;

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
        levelData.levelIndex = LevelSelector.Instance.LevelCount();
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
        gridLevel.PopulateCell(cell, _currentSelectedPiece);
    }
}
