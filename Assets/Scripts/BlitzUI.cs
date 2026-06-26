using UnityEngine;

public class BlitzUI : MonoBehaviour
{
    public GameObject victoryNode;
    public LevelSelector levelSelector;

    public void DisplayPlayerVictory()
    {
        victoryNode.SetActive(true);
    }
    
    public void DidTapNextLevel()
    {
        victoryNode.SetActive(false);

        levelSelector.PlayNextLevel();
    }
}
