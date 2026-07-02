using TMPro;
using UnityEngine;

public class LevelButton : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public GameObject levelCompleteHighlight;
    public GameObject levelPerfectHighlight;

    private LevelData _levelData;

    public void LoadWithLevelData(LevelData levelData)
    {
        _levelData = levelData;

        levelText.text = _levelData.levelIndex.ToString();

        UpdateState(false, false);
    }

    public void UpdateState(bool isComplete, bool isPerfect)
    {
        levelCompleteHighlight.SetActive(isComplete);
        levelPerfectHighlight.SetActive(isPerfect);
    }
    
    
    public void DidTapLevelButton()
    {
        BlitzUI.Instance.ToggleLevels();
        SaveStateManager.Instance.gridLevel.SetupGridForLevel(_levelData);
    }
}
