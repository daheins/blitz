using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class CellData
{
    public List<string> pieceIds = new List<string>();
}

public class LevelData
{
    public string levelName = "tempLevel";
    public int levelIndex;
    public int width;
    public int height;
    public int moveTarget;
    
    public CellData[] cells;
    
    public LevelData()
    {
        width = 14;
        height = 10;
        cells = new CellData[width * height];
        for (int i = 0; i < cells.Length; i++)
            cells[i] = new CellData();
        
        moveTarget = 100;
    }
    
    public List<string> GetPieceIds(int x, int y)
    {
        return cells[y * width + x].pieceIds;
    }

    public void AddPiece(int x, int y, string pieceId)
    {
        CellData cellData = cells[y * width + x];
        cellData.pieceIds.Add(pieceId);
    }
    
    public void RemovePiece(int x, int y, string pieceId)
    {
        CellData cellData = cells[y * width + x];
        cellData.pieceIds.Remove(pieceId);
    }
}