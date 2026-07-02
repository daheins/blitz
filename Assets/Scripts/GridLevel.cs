using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GridLevel : MonoBehaviour
{
    public static Dictionary<string, GridPiece> PiecePrefabByIdentifier;
    public static CommandSystem GridCommandSystem;
    
    private LevelData _levelData;
    
    public GridCell cellPrefab;
    public GridPiece playerPrefab;
    public GridPiece goalPrefab;
    public List<GridPiece> gridItems;
    public List<GridPiece> gridTerrains;
    public List<GridPiece> gridEnemies;
    
    public Transform gridObjectParent;
    public BlitzUI blitzUI;

    public Dictionary<ItemType, int> ItemInventory = new Dictionary<ItemType, int>();
    
    public PlayerScript Player { get; private set; }
    public GridCell[,] Cells { get; private set; }

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
        GridCommandSystem = new CommandSystem();
        
        PiecePrefabByIdentifier = new Dictionary<string, GridPiece>();
        PiecePrefabByIdentifier[playerPrefab.identifier] = playerPrefab;
        PiecePrefabByIdentifier[goalPrefab.identifier] = goalPrefab;

        foreach (GridPiece gridPiece in gridItems.Concat(gridTerrains).Concat(gridEnemies))
        {
            PiecePrefabByIdentifier[gridPiece.identifier] = gridPiece;
        }

        foreach (GridPiece itemPiece in gridItems)
        {
            ItemInventory[itemPiece.itemType] = 0;
        }
    }

    public void SetupGridForLevel(LevelData data)
    {
        _levelData = data;
        Debug.Log($"setting up level: {data.levelName} ({data.levelIndex})");
        
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
        
        GridCommandSystem.ClearHistory();
        
        MoveCounter = 0;
        blitzUI.UpdateMoveCounter(this);
    }
    
    private void FitCameraToGrid()
    {
        gridObjectParent.position = new Vector2(-(_levelData.width - 1f) / 2, -(_levelData.height - 1f) / 2);
        
        float screenAspect = (float)Screen.width / Screen.height;

        float sizeByHeight = _levelData.height / 2f;
        float sizeByWidth = _levelData.width / (2f * screenAspect);

        // Use whichever is larger to guarantee everything fits
        Camera.main!.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth) * 1.2f;
    }

    public void AddPieceToCell(GridCell cell, GridPiece gridPiecePrefab)
    {
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
        Player = player;
        player.playerCell = cell;
        player.playerPiece = playerPiece;
        player.Level = this;
        
        player.transform.SetParent(cell.pieceAnchor.transform, false);
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
        if (_hoveringCell != Player.playerCell)
        {
            bool isValidMove = _validCellsFromHover.Contains(_hoveringCell);

            if (isValidMove)
            {
                var cellsTraveled = _hoverCellTravelMap[_hoveringCell];
                var itemsUsed = _hoverCellItemUsageMap[_hoveringCell];
                
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
        Vector2 localPos = gridObjectParent.InverseTransformPoint(Camera.main!.ScreenToWorldPoint(Input.mousePosition));

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
        Dictionary<GridCell, ItemType> gridItemsRemoved = new Dictionary<GridCell, ItemType>();
        if (passThroughCells != null)
        {
            foreach (GridCell cell in passThroughCells.Append(endCell))
            {
                if (cell.ItemPiece == null) continue;

                gridItemsRemoved[cell] = cell.ItemPiece.itemType;
            }
        }
        
        MoveCommand moveCommand = new MoveCommand(this, Player.playerCell, _hoveringCell,
            itemsUsedInMove, gridItemsRemoved);
        GridCommandSystem.Execute(moveCommand);
        
        blitzUI.UpdateMoveCounter(this);
    }

    public void IncrementMoveCounter()
    {
        MoveCounter++;
    }
    
    public void DecrementMoveCounter()
    {
        MoveCounter--;
    }

    public void PickupItemInCell(GridCell cell)
    {
        if (cell.ItemPiece != null)
        {
            EarnItem(cell.ItemPiece.itemType);

            cell.RemoveCellPiece(cell.ItemPiece);
        }
    }

    public void SpendItem(ItemType itemType)
    {
        int itemCount = ItemInventory[itemType];
        if (itemCount == 0)
            return;

        ItemInventory[itemType]--;
        blitzUI.RemoveInventoryItemIcon(itemType);
    }

    public void EarnItem(ItemType itemType)
    {
        ItemInventory[itemType]++;

        GridPiece itemPiece = gridItems.Find(piece => piece.itemType == itemType); // yuck
        blitzUI.AddInventoryItemIcon(itemPiece);
    }
    
    public void TransferPieceToCell(GridPiece piece, GridCell cell)
    {
        piece.transform.SetParent(cell.pieceAnchor.transform, false);
    }

    private void CheckForVictory()
    {
        if (Player.playerCell.GoalPiece != null)
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
        _validCellsFromHover = new List<GridCell> { Player.playerCell };
        
        _hoverCellTravelMap = new Dictionary<GridCell, List<GridCell>>();
        _hoverCellTravelMap[Player.playerCell] = new List<GridCell>();

        _hoverCellItemUsageMap = new Dictionary<GridCell, List<ItemType>>();
        _hoverCellItemUsageMap[Player.playerCell] = new List<ItemType>();

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int current = Player.playerCell.GridCoordinates + dir;
            
            Dictionary<ItemType, int> availableItems = new Dictionary<ItemType, int>(ItemInventory);
            
            List<GridCell> cellsInDirection = new List<GridCell>();
            List<ItemType> itemsUsedInDirection = new List<ItemType>();
            
            while (IsInBounds(current))
            {
                GridCell cell = CellAtCoordinate(current);
                GridPiece terrain = cell.TerrainPiece;
                if (terrain == null)
                {
                    _validCellsFromHover.Add(cell);
                    cell.SetHoverState(HoverState.Valid);
                }
                else
                {
                    bool shouldStopMovement = false;
                    
                    switch (terrain.terrainType)
                    {
                        case TerrainType.Wall:
                            if (availableItems[ItemType.Spring] > 0)
                            {
                                availableItems[ItemType.Spring]--;
                                itemsUsedInDirection.Add(ItemType.Spring);
                            }
                            else
                            {
                                shouldStopMovement = true;
                            }
                            break;
                        case TerrainType.Spikes:
                        case TerrainType.Mud:
                        case TerrainType.None:
                            break;

                    }

                    if (shouldStopMovement)
                        break;
                }

                _hoverCellItemUsageMap[cell] = new List<ItemType>(itemsUsedInDirection);
                _hoverCellTravelMap[cell] = new List<GridCell>(cellsInDirection);
                cellsInDirection.Add(cell);
                
                current += dir;
            }
        }
    }
}
