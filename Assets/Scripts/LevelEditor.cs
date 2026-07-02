using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelEditor : MonoBehaviour, IItemButtonDelegate
{
    public static LevelEditor Instance;
    
    public GridLevel gridLevel;
    public GameObject editPanel;
    public GameObject specialPiecesPanel;
    public GameObject itemPiecesPanel;
    public GameObject terrainPiecesPanel;
    public GameObject enemyPiecesPanel;

    public EditorPieceButton editorPieceButtonPrefab;

    public TextMeshProUGUI editLabel;
    
    private EditorPieceButton _currentSelectedButton;
    private bool _arePiecesLoaded;

    private void Awake()
    {
        if (!DevelopmentTools.Instance.showDebugUI)
        {
            gameObject.SetActive(false);
            return;
        }
        
        Instance = this;
        
        editPanel.SetActive(false);
    }

    public void ToggleEditMode()
    {
        gridLevel.IsInEditMode = !gridLevel.IsInEditMode;
        
        // editLabel.text = $"Edit Mode: {(gridLevel.IsInEditMode ? "On" : "Off")}";
        editPanel.SetActive(gridLevel.IsInEditMode);

        if (!_arePiecesLoaded)
        {
            List<GridPiece> specialPieces = new List<GridPiece>
            {
                gridLevel.goalPrefab,
                gridLevel.playerPrefab
            };
            
            foreach (GridPiece piece in specialPieces)
            {
                EditorPieceButton editorPieceButton = Instantiate(editorPieceButtonPrefab, specialPiecesPanel.transform);
                editorPieceButton.Delegate = this;
                editorPieceButton.LoadPiece(piece);
            }
            
            foreach (GridPiece piece in gridLevel.gridItems)
            {
                EditorPieceButton editorPieceButton = Instantiate(editorPieceButtonPrefab, itemPiecesPanel.transform);
                editorPieceButton.Delegate = this;
                editorPieceButton.LoadPiece(piece);
            }
            
            foreach (GridPiece piece in gridLevel.gridTerrains)
            {
                EditorPieceButton editorPieceButton = Instantiate(editorPieceButtonPrefab, terrainPiecesPanel.transform);
                editorPieceButton.Delegate = this;
                editorPieceButton.LoadPiece(piece);
            }
            
            foreach (GridPiece piece in gridLevel.gridEnemies)
            {
                EditorPieceButton editorPieceButton = Instantiate(editorPieceButtonPrefab, enemyPiecesPanel.transform);
                editorPieceButton.Delegate = this;
                editorPieceButton.LoadPiece(piece);
            }

            _arePiecesLoaded = true;
        }
    }

    public void SaveLevel()
    {
        SaveStateManager.Instance.SaveLevel(gridLevel.GetLevelData());
    }

    public void CreateNewLevel()
    {
        LevelData levelData = new LevelData();
        levelData.levelIndex = SaveStateManager.Instance.LevelCount();
        levelData.levelName = "temp";
        
        gridLevel.SetupGridForLevel(levelData);
    }

    public void DidTapEditCell(GridCell cell)
    {
        if (_currentSelectedButton != null)
        {
            if (!cell.CanAddPieceToCell(_currentSelectedButton.GridPiece))
                return;
            
            gridLevel.AddPieceToCell(cell, _currentSelectedButton.GridPiece);
            gridLevel.GetLevelData().AddPiece(cell.gridX, cell.gridY, _currentSelectedButton.GridPiece.identifier);
        }
        else
        {
            cell.ResetCell();
            gridLevel.GetLevelData().RemoveAllPieces(cell.gridX, cell.gridY);
        }
    }

    public void DidTapItemButton(EditorPieceButton editorPieceButton)
    {
        _currentSelectedButton?.highlight.gameObject.SetActive(false);

        if (editorPieceButton == _currentSelectedButton)
        {
            _currentSelectedButton = null;
            return;
        }
        
        editorPieceButton.highlight.gameObject.SetActive(true);
        
        _currentSelectedButton = editorPieceButton;
    }
}
