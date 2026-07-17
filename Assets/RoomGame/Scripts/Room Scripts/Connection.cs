using UnityEngine;
using System.Collections.Generic;
using System;
public class Connection
{
    public int ConnectionIndex { get; set; }
    Vector3Int position;
    Room startRoom;
    Room endRoom;
    int floor;
    Direction direction;
    ConnectionType cntType;
    public ConnectionType ConnectionType => cntType;
    public bool Created { get; set; }
    public GameObject CreatedObject { get; set; }
    public Connection(ConnectionTemplate template, Room owningRoom)
    {
        StartRoom = owningRoom;
        Position = template.getLocalIntegerPosition() + owningRoom.Position;
        Direction = template.Direction;
        cntType = template.Type;
        Open = false;
        Created = false;
    }
    public int Floor => floor;
    public bool Open { get; set; }
    public Room StartRoom { get => startRoom; set => startRoom = value; }
    public Room EndRoom { get => endRoom; set => EndRoom = value; }
    public Vector3Int Position { get { return position; } set { position = value; } }
    public Direction Direction { get { return direction; } set { direction = value; } }
    public static Direction getOpposingDirection(Direction dr)
    {
        Dictionary<Direction, Direction> opposingDoorDict = new Dictionary<Direction, Direction>
        {
            { Direction.North, Direction.South },
            { Direction.South, Direction.North },
            { Direction.West, Direction.East },
            { Direction.East, Direction.West },
            { Direction.Up, Direction.Down },
            { Direction.Down, Direction.Up }
        };
        return opposingDoorDict[dr];
    }
    public static int getConnectionPrefabRotation(Direction dr)
    {
        Dictionary<Direction, int> angleDoorDict = new Dictionary<Direction, int>
        {
            { Direction.North, 180 },
            { Direction.South, 0 },
            { Direction.West, 90 },
            { Direction.East, 270 },
            { Direction.Up, 0 },
            { Direction.Down, 0 },
        };
        return angleDoorDict[dr];
    }
}
[Serializable]
public enum Direction
{
    South,
    North,
    West,
    East,
    Up,
    Down
}
[Serializable]
public enum ConnectionType
{
    Door,
    Ladder
}
[Serializable]
public class ConnectionTemplate
{
    [SerializeField] GameObject point;
    [SerializeField] Direction direction;
    [SerializeField] ConnectionType type;
    public Direction Direction { get { return direction; } set { direction = value; } }
    public ConnectionType Type { get { return type; } set { type = value; } }
    public GameObject GameObjectPoint { get { return point; } set { point = value; } }
    public Vector3Int getLocalIntegerPosition()
    {
        Vector3 pos = point.gameObject.transform.localPosition;
        return new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
    }
}