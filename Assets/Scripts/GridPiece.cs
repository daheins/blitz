using UnityEngine;

public enum PieceType { None, Player, Terrain, Goal, Item }

public enum ItemType { None, Spring, }
public enum TerrainType { None, Wall, Mud, Spring, }
public enum EnemyType { None, Goomba, }

public class GridPiece : MonoBehaviour
{
    public PieceType pieceType;
    
    public string identifier;
    public string displayName;
    public SpriteRenderer sprite;
    
    public ItemType itemType;
    public TerrainType terrainType;
    public EnemyType enemyType;
}