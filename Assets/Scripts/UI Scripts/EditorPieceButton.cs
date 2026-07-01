using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IItemButtonDelegate
{
    public void DidTapItemButton(EditorPieceButton editorPieceButton, GridPiece gridPiece);
}

public class EditorPieceButton : MonoBehaviour
{
    public TextMeshProUGUI itemButtonLabel;
    public Image itemButtonImage;
    
    public IItemButtonDelegate Delegate;

    private GridPiece _gridPiece;

    public void LoadPiece(GridPiece gridPiece)
    {
        itemButtonLabel.text = gridPiece.identifier;
        itemButtonImage.sprite = gridPiece.sprite.sprite;

        _gridPiece = gridPiece;
    }

    public void DidTapItemButton()
    {
        Delegate.DidTapItemButton(this, _gridPiece);
    }
}
