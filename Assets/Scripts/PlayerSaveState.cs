using System;
using System.Collections.Generic;

[Serializable]
public class LevelState
{
    public bool isComplete = false;
    public int highScore = -1;
}

public class PlayerSaveState
{
    public Dictionary<string, LevelState> LevelProgressStates = new();
    
    public bool FeatureUnlockHighScores = false;
    public const int KLevelsToUnlockHighScores = 4;
    public bool FeatureUnlockUndoAndRestart = false;
    public const int KLevelsToUnlockUndoAndRestart = 2;
}
