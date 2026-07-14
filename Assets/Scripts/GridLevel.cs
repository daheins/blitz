using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GridLevel : MonoBehaviour, IGridCellDelegate
{
    public static Dictionary<string, GridPiece> PiecePrefabByIdentifier;
    public CommandSystem GridCommandSystem;
    public EnemyPatternSystem EnemyPatternSystem;
    
    private LevelData _levelData;
    public bool IsPortalLevel { get; private set; }
    
    public GridCell cellPrefab;
    public GridPiece playerPrefab;
    public GridPiece goalPrefab;
    public GridPiece portalPrefab;
    public List<GridPiece> gridItems;
    public List<GridPiece> gridTerrains;
    public List<GridPiece> gridEnemies;
    
    public Transform gridObjectParent;

    public Dictionary<ItemType, int> ItemInventory = new Dictionary<ItemType, int>();
    
    public PlayerScript Player { get; private set; }
    public GridCell[,] Cells { get; private set; }
    public PortalGoal PortalGoal { get; private set; }
    
    private List<GridCell> _validCellsFromHover;
    // Map from each cell in the valid hover, to all the cells that it passes through
    private Dictionary<GridCell, List<GridCell>> _playerMoveTravelMap;
    // Map from each cell in the valid hover, to the item that it uses to pass through that cell
    private Dictionary<GridCell, ItemType> _cellItemPassThroughMap;
    // Map from each cell in the valid hover, to the item that it uses to move into that cell
    private Dictionary<GridCell, ItemType> _cellItemMoveToMap;

    private GridCell _mouseDownGridCell;
    
    private HashSet<GridCell> _threatenedEnemyCells;
    private bool _isPlayerDamaged = false;
    
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
        EnemyPatternSystem = new EnemyPatternSystem(this);
        
        PiecePrefabByIdentifier = new Dictionary<string, GridPiece>();
        PiecePrefabByIdentifier[playerPrefab.identifier] = playerPrefab;
        PiecePrefabByIdentifier[goalPrefab.identifier] = goalPrefab;

        foreach (GridPiece gridPiece in gridItems.Concat(gridTerrains).Concat(gridEnemies))
        {
            PiecePrefabByIdentifier[gridPiece.identifier] = gridPiece;
        }
    }

    public void SetupGridForLevel(LevelData data, bool isPortalLevel = false)
    {
        Debug.Log($"Loading Level: {data.levelIdentifier}");
        
        _levelData = data;
        IsPortalLevel = isPortalLevel;
        _isPlayerDamaged = false;
        _validCellsFromHover = new List<GridCell>();
        
        Cells = new GridCell[data.width,data.height];
        
        foreach (GridPiece itemPiece in gridItems)
        {
            ItemInventory[itemPiece.itemType] = 0;
        }

        BuildLevelGridCells();
        
        if (Player != null)
            Player.SetupPlayerForLevel();

        FitCameraToGrid();
        
        GridCommandSystem.ClearHistory();
        
        MoveCounter = 0;
        
        UpdateThreatenedCells();
        
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
        // Horrible hack I will maybe fix later
        if (gridPiecePrefab == goalPrefab && IsPortalLevel)
            gridPiecePrefab = portalPrefab;
        
        GridPiece gridPiece = Instantiate(gridPiecePrefab, cell.transform.position, Quaternion.identity,
            cell.pieceAnchor.transform);
        cell.AddCellPiece(gridPiece);
        
        // handle special pieces
        switch (gridPiece.pieceType)
        {
            case PieceType.Player:
                SetupPlayer(gridPiece.GetComponent<PlayerScript>(), cell, gridPiece);
                break;
            case PieceType.Goal:
                if (IsPortalLevel) PortalGoal = gridPiece.GetComponent<PortalGoal>();
                break;
            case PieceType.Item:
                gridPiece.sprite.AddComponent<FloatingEffect>();
                break;
            case PieceType.Enemy:
                gridPiece.sprite.AddComponent<FloatingEffect>();
                break;
            case PieceType.Terrain:
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
        SetupGridForLevel(_levelData, IsPortalLevel);
    }

    public void MouseDownInGridCell(GridCell gridCell)
    {
        _mouseDownGridCell = gridCell;
    }

    public void MouseUpInGridCell(GridCell gridCell)
    {
        if (gridCell != _mouseDownGridCell)
            return;
        
        if (_playerMoveTravelMap == null || !_playerMoveTravelMap.ContainsKey(gridCell))
            return;
        
        var cellsTraveled = _playerMoveTravelMap[gridCell];

        Dictionary<GridCell, ItemType> itemsUsed = new();
        if (_cellItemMoveToMap.ContainsKey(gridCell))
            itemsUsed[gridCell] = _cellItemMoveToMap[gridCell];

        foreach (GridCell cellTraveled in cellsTraveled)
        {
            if (_cellItemPassThroughMap.ContainsKey(cellTraveled))
                itemsUsed[cellTraveled] = _cellItemPassThroughMap[cellTraveled];
        }

        MovePlayerToCell(gridCell, cellsTraveled, itemsUsed);
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
        return IsInBounds(gridCoordinate.x, gridCoordinate.y);
    }

    public bool IsInBounds(int x, int y)
    {
        if (x < 0 || x >= _levelData.width) return false;
        if (y < 0 || y >= _levelData.height) return false;

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

        GridPiece pieceRemovedAtEndCell = null;
        if (itemsUsedInMove != null && itemsUsedInMove.Keys.Contains(endCell))
        {
            ItemType itemUsed = itemsUsedInMove[endCell];
            if (MoveCommand.IsPieceRemovedByItemOnMove(itemUsed, endCell.TerrainPiece))
            {
                pieceRemovedAtEndCell = endCell.TerrainPiece;
            }
        }
        
        MoveCommand moveCommand = new MoveCommand(this, Player.playerCell, endCell,
            itemsUsedInMove, gridItemsRemoved, pieceRemovedAtEndCell);
        GridCommandSystem.Execute(moveCommand);
        
        BlitzUI.Instance.UpdateMoveCounter();
        
        UpdateValidAndThreatenedCells();

        CheckForVictoryAndDefeat();
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

    private void CheckForVictoryAndDefeat()
    {
        if (Player.playerCell.GoalPiece != null)
        {
            UpdateMoveTarget();

            bool isPerfect = MoveCounter == _levelData.moveTarget;
            SaveStateManager.Instance.SetLevelState(_levelData.levelIdentifier, true, MoveCounter, isPerfect);
            
            BlitzUI.Instance.DisplayPlayerVictory();
        } else if (IsPortalLevel && MoveCounter >= _levelData.moveTarget)
        {
            BlitzUI.Instance.DisplayPlayerDefeat(BlitzUI.DefeatReasonPortal);
        } else if (_isPlayerDamaged)
        {
            BlitzUI.Instance.DisplayPlayerDefeat();
        }
        else
        {
            BlitzUI.Instance.HideVictoryAndDefeatNodes();
        }
    }

    public void MarkPlayerDamage()
    {
        _isPlayerDamaged = true;
    }

    public void ClearPlayerDamage()
    {
        _isPlayerDamaged = false;
        
        BlitzUI.Instance.HideVictoryAndDefeatNodes();
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

    public void UpdateValidAndThreatenedCells()
    {
        UpdateThreatenedCells();
        UpdateValidCells();
    }

    private void UpdateValidCells()
    {
        _validCellsFromHover.ForEach(cell => cell.SetMoveState(false));
        
        _validCellsFromHover = new List<GridCell> { Player.playerCell };
        
        _playerMoveTravelMap = new Dictionary<GridCell, List<GridCell>>();

        _cellItemMoveToMap = new();
        _cellItemPassThroughMap = new();

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int current = Player.playerCell.GridCoordinates + dir;
            
            Dictionary<ItemType, int> availableItems = new Dictionary<ItemType, int>(ItemInventory);
            
            List<GridCell> cellsInDirection = new List<GridCell>();
            
            while (IsInBounds(current))
            {
                GridCell cell = CellAtCoordinate(current);

                bool isThreatenedCell = _threatenedEnemyCells.Contains(cell);
                bool canMoveToCell = cell.CanPlayerMoveToCell(availableItems, out ItemType itemUsedMoveTo, isThreatenedCell);
                if (canMoveToCell)
                {
                    _validCellsFromHover.Add(cell);
                    _playerMoveTravelMap[cell] = new List<GridCell>(cellsInDirection);
                    
                    if (itemUsedMoveTo != ItemType.None)
                    {
                        _cellItemMoveToMap[cell] = itemUsedMoveTo;
                    }
                    
                    cell.SetMoveState(true);
                }
                
                bool canPassThroughCell = cell.CanPlayerPassThroughCell(availableItems, out ItemType itemUsedPassThrough);
                if (!canPassThroughCell)
                    break;

                if (itemUsedPassThrough != ItemType.None)
                {
                    _cellItemPassThroughMap[cell] = itemUsedPassThrough;
                    availableItems[itemUsedPassThrough]--;
                }

                cellsInDirection.Add(cell);
                
                current += dir;
            }
        }
    }

    public void UpdateThreatenedCells()
    {
        _threatenedEnemyCells = new();

        foreach (GridCell cell in Cells)
        {
            if (cell.TerrainPiece != null && cell.TerrainPiece.terrainType == TerrainType.Spikes)
            {
                _threatenedEnemyCells.Add(cell);
            }
            
            if (cell.EnemyPiece != null)
            {
                List<GridCell> threatenedCells = EnemyPatternSystem.GetThreatenedCellsForEnemy(cell.EnemyPiece);
                _threatenedEnemyCells.UnionWith(threatenedCells);
            }
        }

        foreach (GridCell cell in _threatenedEnemyCells)
        {
            cell.SetThreatenedState(true);
        }
    }
}
