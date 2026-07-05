using DG.Tweening;
using UnityEngine;

public class ItemDoober : MonoBehaviour
{
    public SpriteRenderer dooberSprite;
    
    private const float RaiseDuration = 0.6f;
    private const float RaiseAmount = 1f;
    private const float PauseDuration = 0.2f;
    private const float FadeDuration = 0.3f;

    public void Doober(GridPiece itemPiece)
    {
        dooberSprite.sprite = itemPiece.sprite.sprite;
        
        DOTween.Sequence()
            .Append(transform.DOBlendableMoveBy(new Vector3(0, RaiseAmount, 0), RaiseDuration))
            .AppendInterval(PauseDuration)
            .Append(dooberSprite.DOFade(0f, FadeDuration))
            .OnComplete(() => Destroy(gameObject));
    }
}
