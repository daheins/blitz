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
    public ItemType[] items;

    public LevelData()
    {
        width = 14;
        height = 10;
        items = new ItemType[width * height];
    }
    
    public ItemType GetItem(int x, int y)
    {
        return items[y * width + x];
    }

    public void SetItem(ItemType item, int x, int y)
    {
        items[y * width + x] = item;
    }
}