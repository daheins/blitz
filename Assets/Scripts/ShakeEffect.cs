using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.05f;
    [SerializeField] private float frequency = 20f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float pauseDuration = 2f;

    private Vector3 _startPosition;
    private float _timer = 0f;
    private bool _isShaking = false;

    void Start()
    {
        _startPosition = transform.localPosition;
    }

    void Update()
    {
        _timer += Time.deltaTime;

        float shakeOffset = 0f;

        if (_isShaking)
        {
            if (_timer < shakeDuration)
            {
                shakeOffset = Mathf.Sin(Time.time * frequency * 2f * Mathf.PI) * amplitude;
            }
            else
            {
                _isShaking = false;
                _timer = 0f;
            }
        }
        else
        {
            if (_timer >= pauseDuration)
            {
                _isShaking = true;
                _timer = 0f;
            }
        }

        transform.localPosition = _startPosition + new Vector3(shakeOffset, 0, 0);
    }
}