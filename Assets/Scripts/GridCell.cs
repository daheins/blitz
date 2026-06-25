using System;
using UnityEngine;

public enum HoverState { None, Valid, Current, Invalid }

public class GridCell : MonoBehaviour
{
    public GridTerrain GridTerrain { get; set; }
    public GridItem GridItem { get; set; }

    public GameObject terrainAnchor;
    public GameObject itemAnchor;
    public GameObject hoverIndicatorValid;
    public GameObject hoverIndicatorInvalid;
    public GameObject hoverIndicatorCurrent;

    public int gridX = -1;
    public int gridY = -1;

    private HoverState _hoverState; 

    public Vector2Int GridCoordinates => new Vector2Int(gridX, gridY);

    public void RemoveCellItem()
    {
        foreach (Transform child in itemAnchor.transform) {
            Destroy(child.gameObject);
        }
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
        hoverIndicatorValid.SetActive(false);
        hoverIndicatorCurrent.SetActive(false);
        
        foreach (Transform child in terrainAnchor.transform) {
            Destroy(child.gameObject);
        }
        
        foreach (Transform child in itemAnchor.transform) {
            Destroy(child.gameObject);
        }
    }

    private void OnMouseDown()
    {
        LevelEditor levelEditor = LevelEditor.Editor;
        if (!levelEditor.gridLevel.IsInEditMode) return;
        
        levelEditor.DidTapEditCell(this);
    }

    // private void OnMouseEnter()
    // {
    //     LevelEditor levelEditor = LevelEditor.Editor;
    //     if (!levelEditor.gridLevel.IsInEditMode) return;
    //     
    //     levelEditor.DidDragIntoCell(this);
    // }
}
