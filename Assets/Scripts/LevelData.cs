using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelData
{
    public string levelName = "tempLevel";
    public int levelIndex;
    public int width;
    public int height;
    public int moveTarget;
    
    public PieceType[] pieces;
    public ItemType[] items;
    
    public LevelData()
    {
        width = 14;
        height = 10;
        pieces = new PieceType[width * height];
        items = new ItemType[width * height];
        moveTarget = 100;
    }
    
    public PieceType GetPiece(int x, int y)
    {
        return pieces[y * width + x];
    }

    public ItemType GetItem(int x, int y)
    {
        return items[y * width + x];
    }

    public void SetPiece(int x, int y, PieceType pieceType)
    {
        pieces[y * width + x] = pieceType;
    }

    public void SetItem(int x, int y, ItemType itemType)
    {
        items[y * width + x] = itemType;
    }
}