using TMPro;
using UnityEngine;

public class PortalGoal : MonoBehaviour
{
    [SerializeField] private TextMeshPro portalCounter;

    public void UpdatePortal(GridLevel gridLevel)
    {
        portalCounter.text = $"{gridLevel.GetLevelData().moveTarget - gridLevel.MoveCounter}";
    }
}
