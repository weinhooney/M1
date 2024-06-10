using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CustomTile : Tile
{
    [Space]
    [Space]
    [Header("For Designer")]
    public Define.EObjectType objectType;
    public Define.ECreatureType CreatureType;
    public int DataTemplateID;
    public string Name;
}
