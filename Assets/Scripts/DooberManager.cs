using System;
using UnityEngine;

public class DooberManager : MonoBehaviour
{
    public static DooberManager Instance;
    
    public ItemDoober dooberPrefab;
    public Transform dooberParent;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnDoober(GridCell cell, GridPiece itemPiece)
    {
        ItemDoober doober = Instantiate(dooberPrefab, cell.transform.position, Quaternion.identity, dooberParent);
        
        doober.Doober(itemPiece);
    }
}
