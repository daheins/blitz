using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public GridLevel Level { private get; set; }
    public GameObject body;
    public GridCell playerCell;
    public GridItem playerItem;

    private const float DragBodyRaiseY = .3f;

    private Vector3 _mOffset;
    private float _mZCoord;
    private bool _isDragging;

    private Vector3 GetMouseAsWorldPoint()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseDown()
    {
        if (Level.IsInEditMode) return;

        // Store offset = gameobject world pos - mouse world pos
        Vector3 bodyRaiseVector = Vector2.up * DragBodyRaiseY;
        _mOffset = body.transform.localPosition - GetMouseAsWorldPoint() + bodyRaiseVector;
        _isDragging = true;

        Level.PlayerLiftedUp();
    }

    private void OnMouseDrag()
    {
        if (!_isDragging) return;
        
        body.transform.localPosition = GetMouseAsWorldPoint() + _mOffset;

        Level.PlayerDragged();
    }

    private void OnMouseUp()
    {
        if (!_isDragging) return;

        _isDragging = false;
        body.transform.localPosition = Vector3.zero;

        Level.PlayerPutDown();
    }
}
