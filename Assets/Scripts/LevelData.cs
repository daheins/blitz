using System;
using System.IO;
using UnityEngine;

public class LevelData 
{
    private static string path => Application.persistentDataPath + "/level1.json";
    
    public int width;
    public int height;
    public ItemType[] items;

    public LevelData()
    {
        width = 14;
        height = 10;
        items = new ItemType[width * height];
    }

    public static void Save(LevelData data)
    {
        string json = JsonUtility.ToJson(data, true);
        Debug.Log($"saving level. path: {path}, data: {json}");
        File.WriteAllText(path, json);
    }

    public static LevelData Load()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<LevelData>(json);
        }
        else
        {
            return new LevelData();
        }
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