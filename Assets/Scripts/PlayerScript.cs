using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public GridLevel Level { private get; set; }
    public GameObject body;
    public GridCell playerCell;
    public GridPiece playerPiece;
    public GameObject playerAwakeNode;
    public GameObject playerAsleepNode;

    private const float DragBodyRaiseY = .3f;
    private Vector3 _mOffset;
    private float _mZCoord;

    public void SetupPlayerForLevel()
    {
        playerAsleepNode.SetActive(true);
        playerAwakeNode.SetActive(false);
    }

    public void WakeUp()
    {
        playerAsleepNode.SetActive(false);
        playerAwakeNode.SetActive(true);
    }

    private void OnMouseDown()
    {
        if (Level.IsInEditMode) return;

        // Store offset = gameobject world pos - mouse world pos
        Vector3 bodyRaiseVector = Vector2.up * DragBodyRaiseY;
        body.transform.localPosition = bodyRaiseVector;
        // _mOffset = body.transform.localPosition - GetMouseAsWorldPoint() + bodyRaiseVector;
        // _isDragging = true;

        Level.PlayerLiftedUp();
    }
}
