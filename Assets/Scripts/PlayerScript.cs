using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public GridLevel Level { private get; set; }
    public GameObject body;
    public GridCell playerCell;
    public GridPiece playerPiece;

    private const float DragBodyRaiseY = .3f;

    private Vector3 _mOffset;
    private float _mZCoord;
    // private bool _isDragging;

    private Vector3 GetMouseAsWorldPoint()
    {
        return Camera.main!.ScreenToWorldPoint(Input.mousePosition);
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
