using System;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public GridTerrain GridTerrain { get; set; }
    public GridItem GridItem { get; set; }

    public GameObject terrainAnchor;
    public GameObject itemAnchor;

    public int gridX = -1;
    public int gridY = -1;

    public void RemoveCellItem()
    {
        foreach (Transform child in itemAnchor.transform) {
            Destroy(child.gameObject);
        }
    }
    
    public void ResetCell()
    {
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
