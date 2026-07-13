using TMPro;
using UnityEngine;

public class LevelButton : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public GameObject levelCompleteHighlight;
    public GameObject levelLockedNode;

    public LevelData LevelData { get; private set; }

    public void LoadWithLevelData(LevelData levelData)
    {
        LevelData = levelData;

        levelText.text = $"{SaveStateManager.Instance.AllLevelDatas.IndexOf(levelData) + 1}";

        UpdateState(false, false);
    }

    public void UpdateState(bool isComplete, bool isLocked)
    {
        levelCompleteHighlight.SetActive(isComplete);
        levelLockedNode.SetActive(isLocked);
    }
    
    public void DidTapLevelButton()
    {
        if (levelLockedNode.activeSelf && !DevelopmentTools.Instance.canPlayLockedLevels) return;
        
        MenuViewManager.Instance.DisplayGridLevel(LevelData);
    }
}
