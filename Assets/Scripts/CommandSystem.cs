using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ICommand
{
    void Execute();
    void Undo();
}

public class CommandSystem
{
    public static CommandSystem Instance;
    
    private Stack<ICommand> _history = new Stack<ICommand>();
    
    public void Execute(ICommand command)
    {
        command.Execute();
        _history.Push(command);
    }

    public void Undo()
    {
        if (_history.Count == 0) return;
        _history.Pop().Undo();
    }
    
    public void ClearHistory()
    {
        _history.Clear();
    }
}

public class MoveCommand : ICommand
{
    private GridLevel _level;
    private GridCell _startCell;
    private GridCell _endCell;
    private Dictionary<GridCell, ItemType> _itemsUsed;
    private Dictionary<GridCell, ItemType> _gridItemsRemoved;
    private GridPiece _pieceRemovedAtEndCell;
    private string _pieceRemovedAtEndCellIdentifier;
    
    public MoveCommand(GridLevel level, GridCell startCell, GridCell endCell,
        Dictionary<GridCell, ItemType> itemsUsed = null,
        Dictionary<GridCell, ItemType> gridItemsRemoved = null,
        GridPiece pieceRemovedAtEndCell = null)
    {
        _level = level;
        _startCell = startCell;
        _endCell = endCell;
        _itemsUsed = itemsUsed ?? new();
        _gridItemsRemoved = gridItemsRemoved ?? new Dictionary<GridCell, ItemType>();
        _pieceRemovedAtEndCell = pieceRemovedAtEndCell;
    }

    public void Execute()
    {
        foreach (GridCell cell in _gridItemsRemoved.Keys)
        {
            _level.PickupItemInCell(cell);
        }
        
        foreach (var pair in _itemsUsed)
        {
            _level.SpendItem(pair.Value);

            GridPiece itemPiece = _level.gridItems.Find(piece => piece.itemType == pair.Value);
            DooberManager.Instance.SpawnDoober(pair.Key, itemPiece);
        }
        
        _level.Player.playerCell = _endCell;
        _level.TransferPlayerToCell(_endCell, false);

        if (_pieceRemovedAtEndCell != null)
        {
            _pieceRemovedAtEndCellIdentifier = _pieceRemovedAtEndCell.identifier;
            _endCell.RemoveCellPiece(_pieceRemovedAtEndCell);
        }
        
        if (_endCell.IsThreatenedCell && !_itemsUsed.Values.Contains(ItemType.Shield))
        {
            _level.MarkPlayerDamage();
        }

        _level.IncrementMoveCounter();
    }

    public void Undo()
    {
        foreach (var pair in _gridItemsRemoved)
        {
            GridPiece itemPrefab = _level.gridItems.First(item => item.itemType == pair.Value);
            _level.AddPieceToCell(pair.Key, itemPrefab);
            
            _level.SpendItem(pair.Value);
        }

        foreach (ItemType item in _itemsUsed.Values)
        {
            _level.EarnItem(item);
        }
        
        Debug.Log("undoing");
        if (_pieceRemovedAtEndCellIdentifier != null)
        {
            GridPiece piecePrefab = GridLevel.PiecePrefabByIdentifier[_pieceRemovedAtEndCellIdentifier];
            _level.AddPieceToCell(_endCell, piecePrefab);
        }
        
        _level.Player.playerCell = _startCell;
        _level.TransferPlayerToCell(_startCell);
        
        _level.DecrementMoveCounter();
        
        _level.ClearPlayerDamage();
        
        BlitzUI.Instance.UpdateMoveCounter();
        
        _level.UpdateValidAndThreatenedCells();
    }
    
    public static bool IsPieceRemovedByItemOnMove(ItemType itemType, GridPiece gridPiece)
    {
        switch (itemType)
        {
            case ItemType.Key:
                return gridPiece.terrainType == TerrainType.Lock;
        }

        return false;
    }
}


public class RestartCommand : ICommand
{
    public void Execute()
    {
        
    }

    public void Undo()
    {
        
    }
}
