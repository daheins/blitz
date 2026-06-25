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
        Plane plane = new Plane(Vector3.up, new Vector3(0, 2, 0));
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.one;
    }

    private void OnMouseDown()
    {
        Debug.Log("drag 1");
        if (Level.IsInEditMode) return;

        // _mZCoord = Camera.main.WorldToScreenPoint(transform.position).z;

        // Store offset = gameobject world pos - mouse world pos
        _mOffset = transform.position - GetMouseAsWorldPoint();
        _isDragging = true;
        body.transform.localPosition = Vector3.up * DragBodyRaiseY;
        Debug.Log("drag 2");

        Level.PlayerLiftedUp();
    }

    private void OnMouseDrag()
    {
        Debug.Log("drag 3");
        if (!_isDragging) return;

        Debug.Log("drag 4");
        transform.position = GetMouseAsWorldPoint() + _mOffset;

        Level.PlayerDragged();
    }

    private void OnMouseUp()
    {
        Debug.Log("drag 5");
        if (!_isDragging) return;

        _isDragging = false;
        body.transform.localPosition = Vector3.zero;

        Debug.Log("drag 6");
        Level.PlayerPutDown();
    }
}
