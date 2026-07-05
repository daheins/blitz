using System.Collections.Generic;
using System.Linq;

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
    private List<ItemType> _itemsUsed;
    private Dictionary<GridCell, ItemType> _gridItemsRemoved;
    
    public MoveCommand(GridLevel level, GridCell startCell, GridCell endCell,
        // List<ItemType> itemsGained = null,
        List<ItemType> itemsUsed = null,
        // Dictionary<GridCell, GridPiece> piecesAdded = null,
        Dictionary<GridCell, ItemType> gridItemsRemoved = null)
    {
        _level = level;
        _startCell = startCell;
        _endCell = endCell;
        _itemsUsed = itemsUsed ?? new List<ItemType>();
        _gridItemsRemoved = gridItemsRemoved ?? new Dictionary<GridCell, ItemType>();
    }

    public void Execute()
    {
        foreach (GridCell cell in _gridItemsRemoved.Keys)
        {
            _level.PickupItemInCell(cell);
        }
        
        foreach (ItemType item in _itemsUsed)
        {
            _level.SpendItem(item);
        }
        
        _level.Player.playerCell = _endCell;
        _level.TransferPlayerToCell(_endCell, false);

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

        foreach (ItemType item in _itemsUsed)
        {
            _level.EarnItem(item);
        }
        
        _level.Player.playerCell = _startCell;
        _level.TransferPlayerToCell(_startCell);
        
        _level.DecrementMoveCounter();
        
        BlitzUI.Instance.UpdateMoveCounter();
        
        _level.UpdateValidCells();
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
