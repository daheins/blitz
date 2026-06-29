using TMPro;
using UnityEngine;

public class LevelButton : MonoBehaviour
{
    public TextMeshProUGUI levelText;

    private LevelData _levelData;

    public void LoadWithLevelData(LevelData levelData)
    {
        _levelData = levelData;

        levelText.text = _levelData.levelIndex.ToString();
    }
    
    public void DidTapLevelButton()
    {
        LevelLoader.Instance.gridLevel.SetupGridForLevel(_levelData);
    }
}
