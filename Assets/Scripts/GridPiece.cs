using UnityEngine;

public enum PieceType { None, Player, Wall, Goal, Item }

public class GridPiece : MonoBehaviour
{
    public PieceType pieceType;

    public GridItem GridItem { get; set; }
}