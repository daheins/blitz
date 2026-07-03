using TMPro;
using UnityEngine;

public class LevelButton : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public GameObject levelCompleteHighlight;
    public GameObject levelPerfectHighlight;

    public LevelData LevelData { get; private set; }

    public void LoadWithLevelData(LevelData levelData)
    {
        LevelData = levelData;

        levelText.text = $"{SaveStateManager.Instance.AllLevelDatas.IndexOf(levelData) + 1}";

        UpdateState(false, false);
    }

    public void UpdateState(bool isComplete, bool isPerfect)
    {
        levelCompleteHighlight.SetActive(isComplete);
        levelPerfectHighlight.SetActive(isPerfect && SaveStateManager.Instance.PlayerSaveState.FeatureUnlockHighScores);
    }
    
    public void DidTapLevelButton()
    {
        BlitzUI.Instance.ToggleLevels();
        SaveStateManager.Instance.gridLevel.SetupGridForLevel(LevelData);
    }
}
