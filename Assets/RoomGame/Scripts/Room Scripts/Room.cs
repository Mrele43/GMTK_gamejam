using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public enum RoomTypes
{
    Unassigned,
    Start,
    End,
    Optional,
    Default
}
public class Room
{
    List<Bounds> roomBoundaries;
    List<Connection> connections;
    List<bool> approvedDoorways;
    List<bool> approvedLadders;
    List<Room> adjacentRooms;
    List<RoomTypes> roomTypes;
    GameObjectRoomTemplate template;
    public List<RoomTypes> RoomTypes => roomTypes;
    public List<Room> AdjacentRooms => adjacentRooms;
    public Vector3Int Position { get; set; }
    public GameObject RoomObject { get; set; }
    public List<Connection> Connections => connections;
    public List<bool> ApprovedDoorways => approvedDoorways;
    public List<bool> ApprovedLadders => approvedLadders;
    public GameObjectRoomTemplate Template => template;
    public List<Bounds> RoomBoundaries => roomBoundaries;
    public Room(GameObjectRoomTemplate template, Vector3Int position)
    {
        this.template = template;
        Position = position;
        RoomObject = template.RoomGameObject;
        storeRoomBounds(template.ColliderPoints);
        adjacentRooms = new List<Room>();
        roomTypes = new List<RoomTypes>();
    }
    public List<Connection> initialiseConnectors(List<ConnectionTemplate> connectionTemplates, ConnectionTemplate excludedPoint = null, Connection existingConnect = null)
    {
        List<Connection> trueConnectors = new List<Connection>();
        connections = new List<Connection>();
        foreach (ConnectionTemplate cnt in connectionTemplates)
        {
            Connection nextConnect;
            if (excludedPoint != null && excludedPoint != null && cnt == excludedPoint)
            {
                nextConnect = existingConnect;
            }
            else
            {
                nextConnect = new Connection(cnt, this);
                trueConnectors.Add(nextConnect);
            }
            connections.Add(nextConnect);
        }
        return trueConnectors;
    }
    public void storeRoomBounds(List<GameObject> colliderPoints)
    {
        roomBoundaries = new List<Bounds>();
        for (int i = 0; i < colliderPoints.Count - 1; i++)
        {
            Vector3 colliderPos1 = Position + colliderPoints[i].transform.position;
            Vector3 colliderPos2 = Position + colliderPoints[i + 1].transform.position;
            float sizeX = Math.Abs(colliderPos1.x - colliderPos2.x);
            float sizeY = Math.Abs(colliderPos1.y - colliderPos2.y);
            float sizeZ = Math.Abs(colliderPos1.z - colliderPos2.z);
            Vector3 size = new Vector3(sizeX, sizeY, sizeZ);
            Vector3 center = (colliderPos1 + colliderPos2) / 2;
            Bounds newBound = new Bounds(center, size);
            RoomBoundaries.Add(newBound);
        }
    }
    public bool intersectsBounds(Bounds testedBound)
    {
        foreach (Bounds bound in RoomBoundaries)
        {
            if (!(testedBound.max.x <= bound.min.x || bound.max.x <= testedBound.min.x
            || testedBound.max.y <= bound.min.y || bound.max.y <= testedBound.min.y
            || testedBound.max.z <= bound.min.z || bound.max.z <= testedBound.min.z))
            {
                return true;
            }
        }
        return false;
    }
    public int distanceFromRoom(Room opposingRoom)
    {
        Queue<(Room room, int distance)> queue = new Queue<(Room room, int distance)>();
        HashSet<Room> roomHash = new HashSet<Room>();
        queue.Enqueue((this, 0));
        roomHash.Add(this);
        while (queue.Count > 0)
        {
            var (currentRoom, distance) = queue.Dequeue();
            foreach (Room room in currentRoom.adjacentRooms)
            {
                if (roomHash.Contains(room))
                {
                    continue;
                }
                else if (room == opposingRoom)
                {
                    return distance + 1;
                }
                else
                {
                    roomHash.Add(room);
                    queue.Enqueue((room, distance + 1));
                }
            }
        }
        return -1;
    }
}
[Serializable]
public class Window
{
    [SerializeField] GameObject[] windowRaycastPoint;
    [SerializeField] GameObject windowObject;
    [SerializeField] Direction windowDirection;
    public Direction WindowDirection => windowDirection;
    public GameObject WindowObject => windowObject;
    public GameObject[] WindowRayCastPoint => windowRaycastPoint;
    public Vector3 getWindowRaycastDirection()
    {
        Dictionary<Direction, Vector3> directionVectorDict = new Dictionary<Direction, Vector3>
        {
            { Direction.North, new Vector3(0,0,1) },
            { Direction.South, new Vector3(0,0,-1) },
            { Direction.West, new Vector3(-1,0,0) },
            { Direction.East, new Vector3(1,0,0) }
        };
        return directionVectorDict[windowDirection];
    }
}