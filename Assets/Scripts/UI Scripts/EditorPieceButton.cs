using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IItemButtonDelegate
{
    public void DidTapItemButton(EditorPieceButton editorPieceButton);
}

public class EditorPieceButton : MonoBehaviour
{
    public TextMeshProUGUI itemButtonLabel;
    public Image itemButtonImage;
    public Image itemButtonBG;
    public Image highlight;
    
    public IItemButtonDelegate Delegate;

    public GridPiece GridPiece { get; private set; }

    public void LoadPiece(GridPiece gridPiece)
    {
        itemButtonLabel.text = gridPiece.identifier;
        itemButtonImage.sprite = gridPiece.sprite.sprite;
        
        highlight.gameObject.SetActive(false);

        Color bgColor = Color.grey;
        switch (gridPiece.pieceType)
        {
            case PieceType.Goal:
            case PieceType.Player:
                bgColor = Color.softYellow;
                break;
            case PieceType.Item:
                bgColor = Color.softBlue;
                break;
            case PieceType.Terrain:
                bgColor = Color.rosyBrown;
                break;
            case PieceType.Enemy:
                bgColor = Color.softRed;
                break;
        }

        itemButtonBG.color = bgColor;

        GridPiece = gridPiece;
    }

    public void DidTapItemButton()
    {
        Delegate.DidTapItemButton(this);
    }
}
