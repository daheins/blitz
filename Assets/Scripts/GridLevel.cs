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

    public void Start()
    {
        _levelData = LevelData.Load();
        SetupGridForLevel(_levelData);
    }

    public void SetupGridForLevel(LevelData data)
    {
        Cells = new GridCell[_levelData.width,_levelData.height];
        
        foreach (Transform child in gridObjectParent) {
            Destroy(child.gameObject);
        }
        
        Debug.Log($"setting up level: {data}");

        for (int y = 0; y < _levelData.height; y++)
        {
            for (int x = 0; x < _levelData.width; x++)
            {
                GridCell cell = CreateEmptyCell(x, y);
                
                ItemType itemType = _levelData.GetItem(x, y);
                
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
        Debug.Log($"populating cell: {cell.gridX}, {cell.gridY} with item {itemType}");
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
        _hoveringCell = null;
        _validCellsFromHover = new List<GridCell>();
    }

    private void UpdateHoveringCell()
    {
        GridCell hoveringCell = CellFromPosition(_player.transform.position);
        if (hoveringCell == null)
        {
            return;
        }

        _hoveringCell = hoveringCell;
    }

    private GridCell CellFromPosition(Vector3 position)
    {
        return null;
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
        _validCellsFromHover.Add(_player.playerCell);

        /*
        foreach (HexDirection direction in Enum.GetValues(typeof(HexDirection)))
        {
            HexCell cellNeighbor = _player.playerCell.GetNeighbor(direction);
            while (cellNeighbor != null)
            {
                if (cellNeighbor.GridTerrain != null && cellNeighbor.GridTerrain.IsWall)
                    break;
                _validCellsFromHover.Add(cellNeighbor);
                cellNeighbor = cellNeighbor.GetNeighbor(direction);
            }
        }
        */
    }
}
