using TMPro;
using UnityEngine;

public class LevelButton : MonoBehaviour
{
    public TextMeshProUGUI levelText;

    private LevelData _levelData;

    public void LoadWithLevelData(LevelData levelData)
    {
        _levelData = levelData;

        levelText.text = levelData.levelName;
    }
    
    public void DidTapLevelButton()
    {
        // Level.lo
    }
}
