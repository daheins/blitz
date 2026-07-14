using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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
    public GameObject editButton;
    public TextMeshProUGUI quitButtonLabel;

    public EditorPieceButton editorPieceButtonPrefab;
    
    private EditorPieceButton _currentSelectedButton;
    private bool _arePiecesLoaded;
    private bool _levelHasEdits = false;
    private LevelData _originalLevelData;
    private LevelData _editedLevelData;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!DevelopmentTools.Instance.showDebugUI)
        {
            gameObject.SetActive(false);
            return;
        }
        
        UpdateView();
    }

    private void UpdateView()
    {
        editButton.SetActive(false);
        editPanel.SetActive(gridLevel.IsInEditMode);
        
        if (gridLevel.IsInEditMode)
        {
            quitButtonLabel.text = _levelHasEdits ? "Quit (UNSAVED)" : "Quit";
        }
        else // not in edit mode
        {
            editButton.SetActive(true);
        }
    }

    public void StartEditMode()
    {
        gridLevel.IsInEditMode = true;

        _originalLevelData = gridLevel.GetLevelData();
        string json = JsonConvert.SerializeObject(_originalLevelData, Formatting.Indented);
        _editedLevelData = JsonConvert.DeserializeObject<LevelData>(json);
        _editedLevelData.Filename = _originalLevelData.Filename;

        if (!_arePiecesLoaded)
        {
            LoadPieces();
        }

        if (_currentSelectedButton != null)
        {
            // reset edit item button state
            DidTapItemButton(_currentSelectedButton);
        }
        
        UpdateView();
    }

    private void LoadPieces()
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

    public void QuitEditMode()
    {
        editPanel.SetActive(false);
        editButton.SetActive(true);

        gridLevel.IsInEditMode = false;
        
        gridLevel.SetupGridForLevel(_originalLevelData);
    }

    public void SaveLevel()
    {
        SaveStateManager.Instance.UpdateLevelWithChanges(_originalLevelData, _editedLevelData);
        SaveStateManager.Instance.SaveLevel(_originalLevelData);
        
        _levelHasEdits = false;
        UpdateView();
    }

    public void CreateNewLevel()
    {
        LevelData levelData = new LevelData();
        string guid = Guid.NewGuid().ToString("N")[..8];
        levelData.levelName = "temp name";
        levelData.levelIdentifier = guid;
        levelData.Filename = guid;
        
        gridLevel.SetupGridForLevel(levelData);

        StartEditMode();
        
        SaveStateManager.Instance.SaveLevel(levelData);
        SaveStateManager.Instance.AddLevelToManifest(guid);
        SaveStateManager.Instance.ReloadFromManifest();
    }

    public void DidTapEditCell(GridCell cell)
    {
        if (_currentSelectedButton != null)
        {
            if (!cell.CanAddPieceToCell(_currentSelectedButton.GridPiece))
                return;
            
            gridLevel.AddPieceToCell(cell, _currentSelectedButton.GridPiece);
            _editedLevelData.AddPiece(cell.gridX, cell.gridY, _currentSelectedButton.GridPiece.identifier);
            gridLevel.UpdateThreatenedCells();
        }
        else
        {
            cell.ResetCell();
            _editedLevelData.RemoveAllPieces(cell.gridX, cell.gridY);
            gridLevel.UpdateThreatenedCells();
        }
        
        _levelHasEdits = true;
        UpdateView();
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

    public void AddRowButton()
    {
        _editedLevelData.AddRow();
    }

    public void RemoveRowButton()
    {
        _editedLevelData.RemoveRow();
    }

    public void AddColumnButton()
    {
        _editedLevelData.AddColumn();
    }

    public void RemoveColumnButton()
    {
        _editedLevelData.RemoveColumn();
    }
}
