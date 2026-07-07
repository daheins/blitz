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

    private bool _isAwake = false;
    
    private const float TweenDuration = .2f;
    private const float DragBodyRaiseY = .3f;
    private Vector3 BodyRaiseVector => Vector2.up * DragBodyRaiseY;
    private Queue<Vector3> _moveQueue = new Queue<Vector3>();
    private bool _isMoving = false;
    private Tween _activeTweenAnimation;


    public void SetupPlayerForLevel()
    {
        _isAwake = false;

        body.transform.localPosition = Vector3.zero;
        
        playerAsleepNode.SetActive(true);
        playerAwakeNode.SetActive(false);
    }

    private void WakeUp()
    {
        _isAwake = true;
        
        body.transform.localPosition = BodyRaiseVector;
        
        playerAsleepNode.SetActive(false);
        playerAwakeNode.SetActive(true);
    }

    private void OnMouseDown()
    {
        if (Level.IsInEditMode) return;
        
        if (_isAwake) return;
        
        WakeUp();
        Level.UpdateValidAndThreatenedCells();
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
