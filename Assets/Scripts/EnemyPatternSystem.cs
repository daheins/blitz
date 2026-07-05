using System;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackPattern
{
    List<GridCell> GetThreatenedCells(GridCell enemyCell, GridLevel level);
}

public class RadiusAttackPattern : IAttackPattern
{
    private readonly int _radius;
    
    public RadiusAttackPattern(int radius)
    {
        _radius = radius;
    }
    
    public List<GridCell> GetThreatenedCells(GridCell enemyCell, GridLevel level)
    {
        List<GridCell> cells = new List<GridCell>();
    
        for (int x = -_radius; x <= _radius; x++)
        {
            for (int y = -_radius; y <= _radius; y++)
            {
                if (Mathf.Abs(x) == _radius || Mathf.Abs(y) == _radius)
                {
                    GridCell cell = level.Cells[enemyCell.gridX + x, enemyCell.gridY + y];
                    if (cell != null)
                        cells.Add(cell);
                }
            }
        }
    
        return cells;
    }
}

public class EnemyPatternSystem
{
    private readonly GridLevel _gridLevel;

    private Dictionary<EnemyType, IAttackPattern> _attackPatterns;

    public EnemyPatternSystem(GridLevel level)
    {
        _gridLevel = level;
        
        CreateAttackPatterns();
    }

    public void CreateAttackPatterns()
    {
        _attackPatterns = new();
        
        foreach (EnemyType enemyType in Enum.GetValues(typeof(EnemyType)))
        {
            switch (enemyType)
            {
                case EnemyType.GoombaSmall:
                    _attackPatterns[enemyType] = new RadiusAttackPattern(1);
                    break;
                case EnemyType.GoombaBig:
                    _attackPatterns[enemyType] = new RadiusAttackPattern(2);
                    break;
                case EnemyType.None:
                    break;
            }
        }
    }

    public List<GridCell> GetThreatenedCellsForEnemy(GridPiece enemyPiece)
    {
        IAttackPattern attackPattern = _attackPatterns[enemyPiece.enemyType];

        return attackPattern.GetThreatenedCells(enemyPiece.Cell, _gridLevel);
    }
}
