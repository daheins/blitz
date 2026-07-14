using UnityEngine;

public enum PieceType { None, Player, Terrain, Goal, Item, Enemy }

public enum ItemType { None, Spring, Shield, Key }
public enum TerrainType { None, Wall, Mud, Spikes, Lock }
public enum EnemyType { None, GoombaSmall, GoombaBig,  }

public class GridPiece : MonoBehaviour
{
    public PieceType pieceType;
    
    public string identifier;
    public string displayName;
    public SpriteRenderer sprite;
    
    public ItemType itemType;
    public TerrainType terrainType;
    public EnemyType enemyType;
    
    public GridCell Cell { get; set; }
}