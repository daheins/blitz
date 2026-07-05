using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerScript : MonoBehaviour
{
    public GridLevel Level { private get; set; }
    public GameObject body;
    public GridCell playerCell;
    public GridPiece playerPiece;
    public GameObject playerAwakeNode;
    public GameObject playerAsleepNode;

    private const float TweenDuration = .2f;
    private const float DragBodyRaiseY = .3f;
    private Vector3 BodyRaiseVector => Vector2.up * DragBodyRaiseY;
    private Queue<Vector3> _moveQueue = new Queue<Vector3>();
    private bool _isMoving = false;
    private Tween _activeTweenAnimation;


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

        body.transform.localPosition = BodyRaiseVector;

        Level.PlayerLiftedUp();
    }

    public void CancelAllAnimations()
    {
        _moveQueue.Clear();
        _activeTweenAnimation.Kill();
        body.transform.localPosition = BodyRaiseVector;
    }

    public void AnimateToCell(GridCell endCell)
    {
        _moveQueue.Enqueue(endCell.transform.position + BodyRaiseVector);
        
        if (!_isMoving)
            ProcessNextMove();
    }
    
    private void ProcessNextMove()
    {
        if (_moveQueue.Count == 0)
        {
            _isMoving = false;
            return;
        }

        _isMoving = true;
        Vector3 target = _moveQueue.Dequeue();
        _activeTweenAnimation = body.transform.DOMove(target, TweenDuration).OnComplete(ProcessNextMove);
    }
}
