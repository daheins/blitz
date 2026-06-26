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
    public PieceType[] pieces;

    public LevelData()
    {
        width = 14;
        height = 10;
        pieces = new PieceType[width * height];
    }
    
    public PieceType GetPiece(int x, int y)
    {
        return pieces[y * width + x];
    }

    public void SetPiece(PieceType piece, int x, int y)
    {
        pieces[y * width + x] = piece;
    }
}