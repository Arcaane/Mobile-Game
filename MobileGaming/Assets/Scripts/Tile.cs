using System;
using UnityEngine;

public class Tile : MonoBehaviour, IComparable<Tile>
{
    public TypeCases typeCases;
    public Vector3Int posOnTileSet;
    
    public int CompareTo(Tile other) => posOnTileSet.x.CompareTo(other.posOnTileSet.x) != 0 ? posOnTileSet.x.CompareTo(other.posOnTileSet.x) : posOnTileSet.z.CompareTo(other.posOnTileSet.z);
}
