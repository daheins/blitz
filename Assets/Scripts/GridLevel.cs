using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GridLevel : MonoBehaviour
{
    public static Dictionary<ItemType, GridItem> ItemPrefabMap;
    
    private LevelData _levelData;
    
    public GridCell cellPrefab;
    public GridTerrain groundPrefab;
    public PlayerScript playerPrefab;
    public GridPiece goalPrefab;
    public GridPiece wallPrefab;
    public List<GridItem> gridItems;
    
    public Transform gridObjectParent;
    public BlitzUI blitzUI;

    private PlayerScript _player;
    private List<ItemType> _itemInventory = new List<ItemType>();

    private GridCell _hoveringCell;
    private List<GridCell> _validCellsFromHover;
    // Map from each cell in the valid hover, to all the cells that it passes through
    private Dictionary<GridCell, List<GridCell>> _hoverCellTravelMap;
    // Map from each cell in the valid hover, to the items that it uses to get there
    private Dictionary<GridCell, List<ItemType>> _hoverCellItemUsageMap;
    private bool _isInEditMode;

    public int MoveCounter { get; private set; }

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

    private void Awake()
    {
        Dictionary<ItemType, GridItem> itemPrefabs = new Dictionary<ItemType, GridItem>();
        
        foreach (GridItem piece in gridItems)
        {
            itemPrefabs[piece.itemType] = piece;
        }

        ItemPrefabMap = itemPrefabs;
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
                ItemType itemType = data.GetItem(x, y);
                
                PopulateCell(cell, pieceType, itemType);
                
                // TerrainType terrainType = levelData.Terrains[x, y];
                // make all cells ground
                cell.GridTerrain = Instantiate(groundPrefab, cell.transform.position, Quaternion.identity,
                    cell.terrainAnchor.transform);
            }
        }

        gridObjectParent.position = new Vector2(-(_levelData.width - 1f) / 2, -(_levelData.height - 1f) / 2);

        MoveCounter = 0;
        blitzUI.UpdateMoveCounter(this);
    }

    public void PopulateCell(GridCell cell, PieceType pieceType, ItemType itemType)
    {
        _levelData.SetPiece(cell.gridX, cell.gridY, pieceType);
        if (itemType != ItemType.None)
            _levelData.SetItem(cell.gridX, cell.gridY, itemType);
        
        cell.RemoveCellPiece();
        
        // handle pieces
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
                GridItem itemPrefab = ItemPrefabMap[itemType];
                GridItem item = Instantiate(itemPrefab, cell.transform.position, Quaternion.identity,
                    cell.pieceAnchor.transform);
                cell.GridPiece = item.GridPiece;
                cell.GridPiece.GridItem = item;
                break;
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
        if (_hoveringCell != _player.playerCell)
        {
            bool isValidMove = _validCellsFromHover.Contains(_hoveringCell);

            List<GridCell> cellsTraveled = null;
            List<ItemType> itemsUsed = null;
        
            if (isValidMove)
            {
                cellsTraveled = _hoverCellTravelMap[_hoveringCell];
                itemsUsed = _hoverCellItemUsageMap[_hoveringCell];
            }

            MovePlayerToCell(_hoveringCell, cellsTraveled, itemsUsed);
        }

        _hoveringCell.SetHoverState(HoverState.None);
        foreach (GridCell cell in _validCellsFromHover)
        {
            cell.SetHoverState(HoverState.None);
        }
        
        _hoveringCell = null;
        _validCellsFromHover = null;
        _hoverCellTravelMap = null;
        _hoverCellItemUsageMap = null;

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
        
        if (gridX < 0 || gridY < 0 || gridX >= _levelData.width || gridY >= _levelData.height) return null;
        
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

    private void MovePlayerToCell(
        GridCell endCell,
        List<GridCell> passThroughCells = null,
        List<ItemType> itemsUsedInMove = null)
    {
        _player.playerCell = endCell;

        if (passThroughCells != null)
        {
            foreach (GridCell cell in passThroughCells)
            {
                PickupItemInCell(cell);
            }
        }

        if (itemsUsedInMove != null)
        {
            foreach (ItemType item in itemsUsedInMove)
            {
                SpendItem(item);
            }
        }
        
        PickupItemInCell(endCell);
        
        TransferPieceToCell(_player.playerPiece, endCell);

        MoveCounter++;
        blitzUI.UpdateMoveCounter(this);
    }

    private void PickupItemInCell(GridCell cell)
    {
        if (cell.GridPiece?.pieceType != PieceType.Item) 
            return;
        
        _itemInventory.Add(cell.GridPiece.GridItem.itemType);
        blitzUI.AddInventoryItemIcon(cell.GridPiece.GridItem);
                
        cell.RemoveCellPiece();
    }

    private void SpendItem(ItemType item)
    {
        if (!_itemInventory.Contains(item))
            return;

        _itemInventory.Remove(item);
        blitzUI.RemoveInventoryItemIcon(item);
    }
    
    private void TransferPieceToCell(GridPiece piece, GridCell cell)
    {
        piece.transform.SetParent(cell.pieceAnchor.transform, false);
    }

    private void CheckForVictory()
    {
        if (_player.playerCell?.GridPiece?.pieceType == PieceType.Goal)
        {
            UpdateMoveTarget();
            blitzUI.DisplayPlayerVictory();
        }
    }

    private void UpdateMoveTarget()
    {
        if (LevelEditor.Instance == null || LevelLoader.Instance == null)
            return;
        
        if (MoveCounter < _levelData.moveTarget)
        {
            _levelData.moveTarget = MoveCounter;
            LevelLoader.Instance.SaveLevel(_levelData);
        }
    }

    private void FindValidCells()
    {
        _validCellsFromHover = new List<GridCell> { _player.playerCell };
        
        _hoverCellTravelMap = new Dictionary<GridCell, List<GridCell>>();
        _hoverCellTravelMap[_player.playerCell] = new List<GridCell>();

        _hoverCellItemUsageMap = new Dictionary<GridCell, List<ItemType>>();
        _hoverCellItemUsageMap[_player.playerCell] = new List<ItemType>();

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int current = _player.playerCell.GridCoordinates + dir;
            
            List<ItemType> availableItems = new List<ItemType>(_itemInventory);
            
            List<GridCell> cellsInDirection = new List<GridCell>();
            List<ItemType> itemsUsedInDirection = new List<ItemType>();
            
            while (IsInBounds(current))
            {
                GridCell cell = CellAtCoordinate(current);
                if (cell.GridPiece?.pieceType == PieceType.Wall)
                {
                    if (!availableItems.Contains(ItemType.Spring))
                        break;

                    availableItems.Remove(ItemType.Spring);
                    itemsUsedInDirection.Add(ItemType.Spring);
                }
                else
                {
                    _validCellsFromHover.Add(cell);
                    cell.SetHoverState(HoverState.Valid);
                }

                _hoverCellItemUsageMap[cell] = new List<ItemType>(itemsUsedInDirection);
                _hoverCellTravelMap[cell] = new List<GridCell>(cellsInDirection);
                cellsInDirection.Add(cell);
                
                current += dir;
            }
        }
    }
}
