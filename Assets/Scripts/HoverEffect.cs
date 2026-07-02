using UnityEngine;

public class HoverEffect : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.05f;
    [SerializeField] private float frequency = .4f;
    
    private Vector3 _startPosition;

    void Start()
    {
        _startPosition = transform.localPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * frequency * 2f * Mathf.PI) * amplitude;
        transform.localPosition = _startPosition + new Vector3(0, offset, 0);
    }
}