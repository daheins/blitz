using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GridLevel : MonoBehaviour
{
    private LevelData _levelData;
    
    public GridCell cellPrefab;
    public GridTerrain groundPrefab;
    public PlayerScript playerPrefab;
    
    // Pieces
    public GridPiece goalPrefab;
    public GridPiece wallPrefab;
    
    public Transform gridObjectParent;
    public BlitzUI blitzUI;

    private PlayerScript _player;

    private GridCell _hoveringCell;
    private List<GridCell> _validCellsFromHover;
    private bool _isInEditMode;

    public bool IsInEditMode
    {
        get => _isInEditMode;
        set
        {
            _isInEditMode = value;

            if (_isInEditMode)
            {
                SetupGridForLevel(_levelData);
            }
        }
    }

    public LevelData GetLevelData()
    {
        return _levelData;
    }

    
    public GridCell[,] Cells { get; private set; }

    public void SetupGridForLevel(LevelData data)
    {
        Debug.Log($"setting up level: {data.levelName}");
        _levelData = data;
        
        Cells = new GridCell[data.width,data.height];
        
        foreach (Transform child in gridObjectParent) {
            Destroy(child.gameObject);
        }

        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                GridCell cell = CreateEmptyCell(x, y);
                cell.ResetCell();
                
                PieceType pieceType = data.GetPiece(x, y);
                
                PopulateCell(cell, pieceType);
                
                // TerrainType terrainType = levelData.Terrains[x, y];
                // make all cells ground
                cell.GridTerrain = Instantiate(groundPrefab, cell.transform.position, Quaternion.identity,
                    cell.terrainAnchor.transform);
            }
        }

        gridObjectParent.position = new Vector2(-(_levelData.width - 1f) / 2, -(_levelData.height - 1f) / 2);
    }

    public void PopulateCell(GridCell cell, PieceType pieceType, GridItem gridItem = null)
    {
        // Debug.Log($"populating cell: {cell.gridX}, {cell.gridY} with item {itemType}");
        _levelData.SetPiece(pieceType, cell.gridX, cell.gridY);
        
        cell.RemoveCellPiece();
        
        // handle items
        switch (pieceType)
        {
            case PieceType.Player:
                if (_player == null)
                {
                    PlayerScript player = Instantiate(playerPrefab, cell.transform.position, Quaternion.identity,
                        cell.pieceAnchor.transform);
                    GridPiece playerPiece = player.GetComponent<GridPiece>();
                    cell.GridPiece = playerPiece;
                    SetupPlayer(player, cell, playerPiece);
                }
                else
                {
                    MovePlayerToCell(cell);
                }

                break;
            case PieceType.Wall:
                cell.GridPiece = Instantiate(wallPrefab, cell.transform.position, Quaternion.identity,
                    cell.pieceAnchor.transform);
                break;
            case PieceType.Goal:
                cell.GridPiece = Instantiate(goalPrefab, cell.transform.position, Quaternion.identity,
                    cell.pieceAnchor.transform);
                break;
            case PieceType.Item:
            case PieceType.None:
                break;
        }
    }
    
    private GridCell CreateEmptyCell(int x, int y)
    {
        Vector2 position = new Vector2(x, y);

        GridCell cell = Cells[x,y] = Instantiate(cellPrefab);
        cell.transform.SetParent(gridObjectParent, false);
        cell.transform.localPosition = position;
        cell.gridX = x;
        cell.gridY = y;

        return cell;
    }
    
    private void SetupPlayer(PlayerScript player, GridCell cell, GridPiece playerPiece)
    {
        _player = player;
        player.playerCell = cell;
        player.playerPiece = playerPiece;
        player.Level = this;
        MovePlayerToCell(cell);
    }

    public void PlayerLiftedUp()
    {
        FindValidCells();
        UpdateHoveringCell();
    }

    public void PlayerDragged()
    {
        UpdateHoveringCell();
    }

    public void PlayerPutDown()
    {
        if (_validCellsFromHover.Contains(_hoveringCell))
        {
            _player.playerCell = _hoveringCell;
        }

        MovePlayerToCell(_player.playerCell);

        _hoveringCell.SetHoverState(HoverState.None);
        foreach (GridCell cell in _validCellsFromHover)
        {
            cell.SetHoverState(HoverState.None);
        }
        
        _hoveringCell = null;
        _validCellsFromHover = new List<GridCell>();

        CheckForVictory();
    }

    private void UpdateHoveringCell()
    {
        GridCell hoveringCell = CellAtMousePosition();
        if (hoveringCell == null)
        {
            return;
        }

        if (hoveringCell != _hoveringCell)
        {
            if (_hoveringCell != null)
            {
                if (_validCellsFromHover.Contains(_hoveringCell))
                {
                    _hoveringCell.SetHoverState(HoverState.Valid);
                }
                else
                {
                    _hoveringCell.SetHoverState(HoverState.None);
                }
            }

            if (hoveringCell != null)
            {
                if (_validCellsFromHover == null || _validCellsFromHover.Contains(hoveringCell))
                {
                    hoveringCell.SetHoverState(HoverState.Current);
                }
                else
                {
                    hoveringCell.SetHoverState(HoverState.Invalid);
                }
            }
            
            _hoveringCell = hoveringCell;
        }
    }

    private GridCell CellAtMousePosition()
    {
        Vector2 localPos = gridObjectParent.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        return CellFromPosition(localPos);
    }

    private GridCell CellFromPosition(Vector3 position)
    {
        int gridX = Mathf.FloorToInt(position.x + .5f);
        int gridY = Mathf.FloorToInt(position.y + .5f);
        return Cells[gridX, gridY];
    }

    private GridCell CellAtCoordinate(Vector2Int gridCoordinate)
    {
        return Cells[gridCoordinate.x, gridCoordinate.y];
    }

    private bool IsInBounds(Vector2Int gridCoordinate)
    {
        if (gridCoordinate.x < 0 || gridCoordinate.x >= _levelData.width) return false;
        if (gridCoordinate.y < 0 || gridCoordinate.y >= _levelData.height) return false;

        return true;
    }

    private void MovePlayerToCell(GridCell cell)
    {
        _player.playerCell = cell;
        TransferPieceToCell(_player.playerPiece, cell);
    }
    
    private void TransferPieceToCell(GridPiece piece, GridCell cell)
    {
        piece.transform.SetParent(cell.pieceAnchor.transform, false);
    }

    private void CheckForVictory()
    {
        if (_player.playerCell?.GridPiece?.pieceType == PieceType.Goal)
        {
            blitzUI.DisplayPlayerVictory();
        }
    }

    private void FindValidCells()
    {
        _validCellsFromHover = new List<GridCell> { _player.playerCell };

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int current = _player.playerCell.GridCoordinates + dir;
            
            while (IsInBounds(current))
            {
                GridCell cell = CellAtCoordinate(current);
                if (cell.GridPiece?.pieceType == PieceType.Wall)
                    break;
                _validCellsFromHover.Add(cell);
                cell.SetHoverState(HoverState.Valid);
                
                current += dir;
            }
        }
    }
}
