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
    
    // Items
    public GridItem goalPrefab;
    public GridItem wallPrefab;
    
    public Transform gridObjectParent;

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
                
                ItemType itemType = data.GetItem(x, y);
                
                PopulateCell(cell, itemType);
                
                // TerrainType terrainType = levelData.Terrains[x, y];
                // make all cells ground
                cell.GridTerrain = Instantiate(groundPrefab, cell.transform.position, Quaternion.identity,
                    cell.terrainAnchor.transform);
            }
        }

        gridObjectParent.position = new Vector2(-(float)_levelData.width / 2, -(float)_levelData.height / 2);
    }

    public void PopulateCell(GridCell cell, ItemType itemType)
    {
        // Debug.Log($"populating cell: {cell.gridX}, {cell.gridY} with item {itemType}");
        _levelData.SetItem(itemType, cell.gridX, cell.gridY);
        
        cell.RemoveCellItem();
        
        // handle items
        switch (itemType)
        {
            case ItemType.Player:
                if (_player == null)
                {
                    PlayerScript player = Instantiate(playerPrefab, cell.transform.position, Quaternion.identity,
                        cell.itemAnchor.transform);
                    GridItem playerItem = player.GetComponent<GridItem>();
                    cell.GridItem = playerItem;
                    SetupPlayer(player, cell, playerItem);
                }
                else
                {
                    MovePlayerToCell(cell);
                }

                break;
            case ItemType.Wall:
                cell.GridItem = Instantiate(wallPrefab, cell.transform.position, Quaternion.identity,
                    cell.itemAnchor.transform);
                break;
            case ItemType.Goal:
                cell.GridItem = Instantiate(goalPrefab, cell.transform.position, Quaternion.identity,
                    cell.itemAnchor.transform);
                break;
            case ItemType.Enemy:
            case ItemType.None:
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
    
    private void SetupPlayer(PlayerScript player, GridCell cell, GridItem playerItem)
    {
        _player = player;
        player.playerCell = cell;
        player.playerItem = playerItem;
        player.Level = this;
        MovePlayerToCell(cell);
    }

    public void PlayerLiftedUp()
    {
        UpdateHoveringCell();
        FindValidCells();
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
        TransferItemToCell(_player.playerItem, cell);
    }
    
    private void TransferItemToCell(GridItem item, GridCell cell)
    {
        item.transform.SetParent(cell.itemAnchor.transform, false);
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
                if (cell.GridItem?.itemType == ItemType.Wall)
                    break;
                _validCellsFromHover.Add(cell);
                cell.SetHoverState(HoverState.Valid);
                
                current += dir;
            }
        }
    }
}
