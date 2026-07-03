#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class DebugTools
{
    [MenuItem("Dev/Reset Game State")]
    public static void ResetGameState()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Player Prefs cleared");
    }
}
#endif