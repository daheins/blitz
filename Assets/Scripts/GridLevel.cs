using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class GridLevel : MonoBehaviour
{
    public static Dictionary<string, GridPiece> PiecePrefabByIdentifier;
    
    private LevelData _levelData;
    
    public GridCell cellPrefab;
    public PlayerScript playerPrefab;
    public GridPiece goalPrefab;
    public List<GridPiece> gridItems;
    public List<GridPiece> gridTerrains;
    public List<GridPiece> gridEnemies;
    
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
        PiecePrefabByIdentifier = new Dictionary<string, GridPiece>();
        PiecePrefabByIdentifier[goalPrefab.identifier] = goalPrefab;
        PiecePrefabByIdentifier[goalPrefab.identifier] = goalPrefab;
        PiecePrefabByIdentifier[goalPrefab.identifier] = goalPrefab;

        foreach (GridPiece gridPiece in gridItems.Concat(gridTerrains).Concat(gridEnemies))
        {
            PiecePrefabByIdentifier[gridPiece.identifier] = gridPiece;
        }
    }
    
    public GridCell[,] Cells { get; private set; }

    public void SetupGridForLevel(LevelData data)
    {
        Debug.Log($"setting up level: {data.levelName} ({data.levelIndex})");
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
                
                List<string> pieces = data.GetPieceIds(x, y);

                foreach (string pieceId in pieces)
                {
                    GridPiece gridPiecePrefab = PiecePrefabByIdentifier[pieceId];
                    AddPieceToCell(cell, gridPiecePrefab);
                }
            }
        }

        FitCameraToGrid();

        gridObjectParent.position = new Vector2(-(_levelData.width - 1f) / 2, -(_levelData.height - 1f) / 2);

        MoveCounter = 0;
        blitzUI.UpdateMoveCounter(this);
    }
    
    private void FitCameraToGrid()
    {
        float screenAspect = (float)Screen.width / Screen.height;

        float sizeByHeight = _levelData.height / 2f;
        float sizeByWidth = _levelData.width / (2f * screenAspect);

        // Use whichever is larger to guarantee everything fits
        Camera.main.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth) * 1.2f;
    }

    public void AddPieceToCell(GridCell cell, GridPiece gridPiecePrefab)
    {
        _levelData.AddPiece(cell.gridX, cell.gridY, gridPiecePrefab.identifier);
        
        GridPiece gridPiece = Instantiate(gridPiecePrefab, cell.transform.position, Quaternion.identity,
            cell.pieceAnchor.transform);
        cell.AddCellPiece(gridPiece);
        
        // handle special pieces
        switch (gridPiece.pieceType)
        {
            case PieceType.Player:
                SetupPlayer(gridPiece.GetComponent<PlayerScript>(), cell, gridPiece);
                break;
            case PieceType.Terrain:
            case PieceType.Goal:
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
        if (_hoveringCell != _player.playerCell)
        {
            bool isValidMove = _validCellsFromHover.Contains(_hoveringCell);

            List<GridCell> cellsTraveled = null;
            List<ItemType> itemsUsed = null;
        
            if (isValidMove)
            {
                cellsTraveled = _hoverCellTravelMap[_hoveringCell];
                itemsUsed = _hoverCellItemUsageMap[_hoveringCell];
                
                MovePlayerToCell(_hoveringCell, cellsTraveled, itemsUsed);
            }
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
        if (cell.ItemPiece != null)
        {
            _itemInventory.Add(cell.ItemPiece.itemType);
            blitzUI.AddInventoryItemIcon(cell.ItemPiece);

            cell.RemoveCellPiece(cell.ItemPiece);
        }
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
        if (_player.playerCell.GoalPiece != null)
        {
            UpdateMoveTarget();
            blitzUI.DisplayPlayerVictory();
        }
    }

    private void UpdateMoveTarget()
    {
        if (LevelEditor.Instance == null || LevelLoader.Instance == null)
            return;
        
        if (DevelopmentTools.Instance.updateMoveTarget && MoveCounter < _levelData.moveTarget)
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
                if (cell.TerrainPiece != null)
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
