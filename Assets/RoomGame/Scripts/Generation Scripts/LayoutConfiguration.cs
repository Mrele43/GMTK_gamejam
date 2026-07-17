using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
[CreateAssetMenu(fileName = "LayoutConfiguration", menuName = "Custom/LevelGeneration/LayoutConfiguration")]
public class LayoutConfiguration : ScriptableObject
{
    [SerializeField] List<GameObjectRoomTemplate> rooms;
    [SerializeField] int layoutSizeWidth;
    [SerializeField] int layoutSizeLength;
    [SerializeField] int roomCountMax;
    [SerializeField] bool excludeSameAdjacentTemplate;
    [SerializeField] int roomOptionalCount;
    public int RoomSizeWidth => layoutSizeWidth;
    public int RoomSizeLength => layoutSizeLength;
    public int RoomCountMax => roomCountMax;
    public bool ExcludeSameAdjacentTemplate => excludeSameAdjacentTemplate;
    public int RoomOptionalCount => roomOptionalCount;
    public List<GameObjectRoomTemplate> Rooms => rooms;
    public Vector3Int getLayoutCenter()
    {
        return new Vector3Int((int)(layoutSizeWidth / 2), 0, (int)(layoutSizeLength / 2));
    }
}
