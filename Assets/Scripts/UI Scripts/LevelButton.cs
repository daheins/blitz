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
        levelCompleteHighlight.SetActive(false);
        levelPerfectHighlight.SetActive(false);

        if (!isComplete)
        {
            return;
        }

        if (isPerfect)
        {
            levelPerfectHighlight.SetActive(true);
        }
        else
        {
            levelCompleteHighlight.SetActive(true);
        }
    }
    
    
    public void DidTapLevelButton()
    {
        SaveStateManager.Instance.ToggleLevels();
        SaveStateManager.Instance.gridLevel.SetupGridForLevel(_levelData);
    }
}
