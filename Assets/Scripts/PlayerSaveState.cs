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
    public Dictionary<int, LevelState> LevelProgressStates = new();
}
