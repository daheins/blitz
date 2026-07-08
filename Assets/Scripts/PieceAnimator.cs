using UnityEngine;

public class PieceAnimator : MonoBehaviour
{
    [SerializeField] private GameObject frameParent;
    [SerializeField] private float secondsPerFrame = 0.1f;

    private SpriteRenderer[] _frames;
    private float _timer;
    private int _currentFrame;

    void Start()
    {
        _frames = frameParent.GetComponentsInChildren<SpriteRenderer>();
        
        SetFrame(0);
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= secondsPerFrame)
        {
            _timer = 0f;
            _currentFrame = (_currentFrame + 1) % _frames.Length;
            SetFrame(_currentFrame);
        }
    }

    private void SetFrame(int frame)
    {
        for (int i = 0; i < _frames.Length; i++)
        {
            _frames[i].gameObject.SetActive(i == frame);
        }
    }
}
