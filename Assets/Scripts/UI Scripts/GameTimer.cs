using UnityEngine;

public class GameTimer : MonoBehaviour
{
    private float _elapsedTime = 0f;
    private bool _isRunning = false;

    public float ElapsedTime => _elapsedTime;

    void Update()
    {
        if (_isRunning)
            _elapsedTime += Time.deltaTime;
    }

    public void StartTimer()
    {
        _elapsedTime = 0f;
        _isRunning = true;
    }

    public void PauseTimer()
    {
        _isRunning = false;
    }

    public void ResumeTimer()
    {
        _isRunning = true;
    }

    public string GetFormattedTime()
    {
        return $"{_elapsedTime:F2}s";
    }
}