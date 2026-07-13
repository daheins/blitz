using System;
using UnityEngine;

public class DevelopmentTools : MonoBehaviour
{
    public static DevelopmentTools Instance;
    
    public bool showDebugUI;
    public bool startAtLastLevel;
    public bool updateMoveTarget;
    public bool canPlayLockedLevels;

    private void Awake()
    {
        Instance = this;
    }
}
