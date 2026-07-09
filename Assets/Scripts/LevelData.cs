using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class CellData
{
    public List<string> pieceIds = new List<string>();
}

[Serializable]
public class LevelData
{
    public string levelIdentifier;
    public string levelName;
    public int width;
    public int height;
    public int moveTarget;
    
    [NonSerialized] 
    public string Filename;
    
    public Dictionary<string, CellData> CellMap = new();
    
    private string Key(int x, int y) => $"{x},{y}";
    
    public CellData GetCell(int x, int y)
    {
        CellMap.TryGetValue(Key(x, y), out CellData cell);
        return cell;
    }

    public void SetCell(int x, int y, CellData cell)
    {
        CellMap[Key(x, y)] = cell;
    }
    
    public LevelData()
    {
        width = 14;
        height = 10;
        CellMap = new();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                SetCell(x, y, new CellData());
        
        moveTarget = 100;
    }
    
    public List<string> GetPieceIds(int x, int y)
    {
        return GetCell(x, y).pieceIds;
    }

    public void AddPiece(int x, int y, string pieceId)
    {
        CellData cellData = GetCell(x, y);
        cellData.pieceIds.Add(pieceId);
    }
    
    public void RemovePiece(int x, int y, string pieceId)
    {
        CellData cellData = GetCell(x, y);
        cellData.pieceIds.Remove(pieceId);
    }
    
    public void RemoveAllPieces(int x, int y)
    {
        CellData cellData = GetCell(x, y);
        cellData.pieceIds.Clear();
    }
    
    public void AddColumn()
    {
        width++;
        for (int y = 0; y < height; y++)
            SetCell(width - 1, y, new CellData());
    }

    public void RemoveColumn()
    {
        if (width <= 1) return;
        for (int y = 0; y < height; y++)
            CellMap.Remove(Key(width - 1, y));
        width--;
    }

    public void AddRow()
    {
        height++;
        for (int x = 0; x < width; x++)
            SetCell(x, height - 1, new CellData());
    }

    public void RemoveRow()
    {
        if (height <= 1) return;
        for (int x = 0; x < width; x++)
            CellMap.Remove(Key(x, height - 1));
        height--;
    }
}