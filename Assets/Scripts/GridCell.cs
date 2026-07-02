using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

public enum HoverState { None, Valid, Current, Invalid }

public interface IGridCellDelegate
{
    public void DidTapValidGridCell(GridCell gridCell);
}

public class GridCell : MonoBehaviour
{
    public GameObject terrainAnchor;
    public GameObject pieceAnchor;
    public GameObject hoverIndicatorValid;
    public GameObject hoverIndicatorInvalid;
    public GameObject hoverIndicatorCurrent;

    public IGridCellDelegate Delegate;

    public int gridX = -1;
    public int gridY = -1;
    
    private List<GridPiece> _gridPieces = new List<GridPiece>();
    
    public GridPiece TerrainPiece { get; private set; }
    public GridPiece ItemPiece { get; private set; }
    public GridPiece GoalPiece { get; private set; }

    private HoverState _hoverState; 

    public Vector2Int GridCoordinates => new Vector2Int(gridX, gridY);

    public void AddCellPiece(GridPiece gridPiece)
    {
        _gridPieces.Add(gridPiece);

        if (gridPiece.pieceType == PieceType.Terrain)
            TerrainPiece = gridPiece;
        
        if (gridPiece.pieceType == PieceType.Item)
            ItemPiece = gridPiece;
        
        if (gridPiece.pieceType == PieceType.Goal)
            GoalPiece = gridPiece;
    }
    
    public void RemoveCellPiece(GridPiece gridPiece)
    {
        if (gridPiece == ItemPiece)
            ItemPiece = null;

        if (gridPiece == TerrainPiece)
            TerrainPiece = null;
        
        if (gridPiece == GoalPiece)
            GoalPiece = null;
        
        Destroy(gridPiece.gameObject);
        _gridPieces.Remove(gridPiece);
    }
    
    public bool CanAddPieceToCell(GridPiece gridPiecePrefab)
    {
        return _gridPieces.All(piece => piece.identifier != gridPiecePrefab.identifier);
    }
    
    public void SetHoverState(HoverState hoverState)
    {
        if (hoverState == _hoverState) return;
        
        _hoverState = hoverState;

        hoverIndicatorValid.SetActive(false);
        hoverIndicatorInvalid.SetActive(false);
        hoverIndicatorCurrent.SetActive(false);
        
        switch (hoverState)
        {
            case HoverState.Current:
                hoverIndicatorCurrent.SetActive(true);
                break;
            case HoverState.Valid:
                hoverIndicatorValid.SetActive(true);
                break;
            case HoverState.Invalid:
                hoverIndicatorInvalid.SetActive(true);
                break;
            case HoverState.None:
                break;
        }
    }
    
    public void ResetCell()
    {
        foreach (GridPiece gridPiece in _gridPieces)
        {
            Destroy(gridPiece.gameObject);
        }
        
        hoverIndicatorValid.SetActive(false);
        hoverIndicatorCurrent.SetActive(false);
        
        _gridPieces = new List<GridPiece>();
        TerrainPiece = null;
        ItemPiece = null;
        GoalPiece = null;
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        
        LevelEditor levelEditor = LevelEditor.Instance;
        if (levelEditor != null && levelEditor.gridLevel.IsInEditMode)
        {
            levelEditor.DidTapEditCell(this);
            return;
        }

        if (_hoverState == HoverState.Valid)
        {
            Delegate.DidTapValidGridCell(this);
        }
    }
}
