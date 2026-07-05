using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GridLevel : MonoBehaviour, IGridCellDelegate
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

    public Dictionary<ItemType, int> ItemInventory = new Dictionary<ItemType, int>();
    
    public PlayerScript Player { get; private set; }
    public GridCell[,] Cells { get; private set; }
    
    private List<GridCell> _validCellsFromHover;
    // Map from each cell in the valid hover, to all the cells that it passes through
    private Dictionary<GridCell, List<GridCell>> _hoverCellTravelMap;
    // Map from each cell in the valid hover, to the items that it uses to get there
    private Dictionary<GridCell, Dictionary<GridCell, ItemType>> _hoverCellItemUsageMap;
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
    }

    public void SetupGridForLevel(LevelData data)
    {
        _levelData = data;
        Debug.Log($"Loading Level: {data.levelIdentifier}");
        
        Cells = new GridCell[data.width,data.height];
        
        _validCellsFromHover = new List<GridCell>();

        foreach (GridPiece itemPiece in gridItems)
        {
            ItemInventory[itemPiece.itemType] = 0;
        }

        BuildLevelGridCells();
        
        Player.SetupPlayerForLevel();

        FitCameraToGrid();
        
        GridCommandSystem.ClearHistory();
        
        MoveCounter = 0;
        
        BlitzUI.Instance.StartGridLevel();
    }

    private void BuildLevelGridCells()
    {
        foreach (Transform child in gridObjectParent) {
            Destroy(child.gameObject);
        }

        for (int y = 0; y < _levelData.height; y++)
        {
            for (int x = 0; x < _levelData.width; x++)
            {
                GridCell cell = CreateEmptyCell(x, y);
                cell.Delegate = this;
                cell.ResetCell();
                
                List<string> pieces = _levelData.GetPieceIds(x, y);

                foreach (string pieceId in pieces)
                {
                    GridPiece gridPiecePrefab = PiecePrefabByIdentifier[pieceId];
                    AddPieceToCell(cell, gridPiecePrefab);
                }
            }
        }
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
                break;
            case PieceType.Item:
                gridPiece.sprite.AddComponent<FloatingEffect>();
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
        Player = player;
        player.playerCell = cell;
        player.playerPiece = playerPiece;
        player.Level = this;
        
        player.transform.SetParent(cell.pieceAnchor.transform, false);
    }

    public void RestartLevel()
    {
        SetupGridForLevel(_levelData);
    }

    public void PlayerLiftedUp()
    {
        Player.WakeUp();
        UpdateValidCells();
    }

    public void DidTapValidGridCell(GridCell gridCell)
    {
        var cellsTraveled = _hoverCellTravelMap[gridCell];
        var itemsUsed = _hoverCellItemUsageMap[gridCell];

        MovePlayerToCell(gridCell, cellsTraveled, itemsUsed);
        
        UpdateValidCells();

        CheckForVictory();
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
        Dictionary<GridCell, ItemType> itemsUsedInMove = null)
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
        
        MoveCommand moveCommand = new MoveCommand(this, Player.playerCell, endCell,
            itemsUsedInMove, gridItemsRemoved);
        GridCommandSystem.Execute(moveCommand);
        
        BlitzUI.Instance.UpdateMoveCounter();
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
        BlitzUI.Instance.RemoveInventoryItemIcon(itemType);
    }

    public void EarnItem(ItemType itemType)
    {
        ItemInventory[itemType]++;

        GridPiece itemPiece = gridItems.Find(piece => piece.itemType == itemType); // yuck
        BlitzUI.Instance.AddInventoryItemIcon(itemPiece);
    }
    
    public void TransferPlayerToCell(GridCell cell, bool isInstant = true)
    {
        if (isInstant)
        {
            Player.CancelAllAnimations();
            Player.playerPiece.transform.SetParent(cell.pieceAnchor.transform, false);
            return;
        }
        
        Player.AnimateToCell(cell);
    }

    private void CheckForVictory()
    {
        if (Player.playerCell.GoalPiece != null)
        {
            UpdateMoveTarget();

            bool isPerfect = MoveCounter == _levelData.moveTarget;
            SaveStateManager.Instance.SetLevelState(_levelData.levelIdentifier, true, MoveCounter, isPerfect);
            
            BlitzUI.Instance.DisplayPlayerVictory();
        }
    }

    private void UpdateMoveTarget()
    {
        if (LevelEditor.Instance == null || SaveStateManager.Instance == null)
            return;
        
        if (DevelopmentTools.Instance.updateMoveTarget && MoveCounter < _levelData.moveTarget)
        {
            _levelData.moveTarget = MoveCounter;
            SaveStateManager.Instance.SaveLevel(_levelData);
        }
    }

    public void UpdateValidCells()
    {
        _validCellsFromHover.ForEach(cell => cell.SetHoverState(HoverState.None));
        
        _validCellsFromHover = new List<GridCell> { Player.playerCell };
        
        _hoverCellTravelMap = new Dictionary<GridCell, List<GridCell>>();
        _hoverCellTravelMap[Player.playerCell] = new List<GridCell>();

        _hoverCellItemUsageMap = new();
        _hoverCellItemUsageMap[Player.playerCell] = new ();

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int current = Player.playerCell.GridCoordinates + dir;
            
            Dictionary<ItemType, int> availableItems = new Dictionary<ItemType, int>(ItemInventory);
            
            List<GridCell> cellsInDirection = new List<GridCell>();
            Dictionary<GridCell, ItemType> itemsUsedInTravelCells = new();
            
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
                                itemsUsedInTravelCells[cell] = ItemType.Spring;
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

                _hoverCellItemUsageMap[cell] = new Dictionary<GridCell, ItemType>(itemsUsedInTravelCells);
                _hoverCellTravelMap[cell] = new List<GridCell>(cellsInDirection);
                cellsInDirection.Add(cell);
                
                current += dir;
            }
        }
    }
}
