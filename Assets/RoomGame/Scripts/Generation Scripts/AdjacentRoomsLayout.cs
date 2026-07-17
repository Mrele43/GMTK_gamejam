using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Random = System.Random;
public class AdjacentRoomsLayout : MonoBehaviour
{
    [SerializeField] LayoutConfiguration layoutConfiguration;
    List<Connection> availableConnectors;
    List<Room> rooms;
    Random random;
    public Level generateLevel()
    {
        Level level = new Level();
        random = SharedLevelData.Instance.RandomGen;
        availableConnectors = new List<Connection>();
        rooms = new List<Room>();
        placeInitialRoom();
        placeRooms();
        assignRoomTypes(level);
        level.Rooms = rooms;
        return level;
    }
    public void assignRoomTypes(Level level)
    {
        List<Room> edgeRooms = determineOptionalRooms();
        determineStartEndRooms(edgeRooms, level);
    }
    public List<Room> determineOptionalRooms()
    {
        List<Room> edgeRooms = new List<Room>();
        foreach (Room room in rooms)
        {
            if (room.AdjacentRooms.Count == 1)
            {
                room.RoomTypes.Add(RoomTypes.Optional);
                edgeRooms.Add(room);
            }
            else
            {
                room.RoomTypes.Add(RoomTypes.Default);
            }
        }
        return edgeRooms;
    }
    public void determineStartEndRooms(List<Room> edgeRooms, Level level)
    {
        Room room1 = edgeRooms[0];
        Room room2 = edgeRooms[1];
        int roomDistance = -1;
        for (int i = 0; i < edgeRooms.Count - 1; i++)
        {
            for (int j = i + 1; j < edgeRooms.Count; j++)
            {
                int roomsBetween = edgeRooms[i].distanceFromRoom(edgeRooms[j]);
                if (roomsBetween > roomDistance)
                {
                    roomDistance = roomsBetween;
                    room1 = edgeRooms[i];
                    room2 = edgeRooms[j];
                }
            }
        }
        room1.RoomTypes.Add(RoomTypes.Start);
        room2.RoomTypes.Add(RoomTypes.End);
        /*
         * Must remove Optional typing from start and end rooms, 
         * as they are now necessary from determining the start and end of exploration
        */
        room1.RoomTypes.Remove(RoomTypes.Optional);
        room2.RoomTypes.Remove(RoomTypes.Optional);
        level.StartRoom = room1;
        level.EndRoom = room2;
    }
    /*
     * Create initial room in the center of designated layout area, 
     * establishing initial doors and other connectors for following rooms to branch from.
    */
    public void placeInitialRoom()
    {
        GameObjectRoomTemplate firstRoomTemplate = layoutConfiguration.Rooms[0];
        Vector3Int startPos = layoutConfiguration.getLayoutCenter();
        Room firstRoom = new Room(firstRoomTemplate, startPos);
        List<Connection> connections = firstRoom.initialiseConnectors(firstRoomTemplate.Connectors);
        availableConnectors.AddRange(connections);
        rooms.Add(firstRoom);
    }
    public void placeRooms()
    {
        int optionalRoomCount = layoutConfiguration.RoomCountMax > layoutConfiguration.RoomOptionalCount + 2
            ? layoutConfiguration.RoomOptionalCount : 0;
        while (rooms.Count < layoutConfiguration.RoomCountMax && availableConnectors.Count > 0)
        {
            /*
             * Create rooms which add their connectors, 
             * then switch to rooms which don't contribute connectors to meet option room requirement, 
             * and 2 more for start and end rooms 
            */
            bool deadEndRoom = rooms.Count < layoutConfiguration.RoomCountMax - optionalRoomCount - 2;
            createAdjacentRoom(addAdditionalDoorways: deadEndRoom);
        }
    }
    public void createAdjacentRoom(bool addAdditionalDoorways)
    {
        /*
         * Take door or other connector from existing room, 
         * and find opposing connection from potential new room
        */
        Connection chosenConnection = chooseConnection();
        GameObjectRoomTemplate chosenTemplate = chooseRoomTemplate(chosenConnection.StartRoom.Template);
        ConnectionTemplate newConnect = findOpposingConnection(chosenConnection, chosenTemplate);
        if(newConnect == null)
        {
            return;
        }
        /* 
         * Translate opposing connection to find the required position of potential new room,
         * rejecting if position is invalid
         */
        Vector3Int adjacentPos = findAdjacentPos(chosenConnection, newConnect);
        Room newRoom = new Room(chosenTemplate, adjacentPos);
        if (!checkRoomValidity(newRoom))
        {
            return;
        }
        chosenConnection.StartRoom.AdjacentRooms.Add(newRoom);
        newRoom.AdjacentRooms.Add(chosenConnection.StartRoom);
        /*
         * Assign shared connector as completed, 
         * passing it when new room initialises potential connectors 
         * so that room knows which connector is used
        */
        chosenConnection.Open = true;
        List<Connection> connections = newRoom.initialiseConnectors(chosenTemplate.Connectors, newConnect, chosenConnection);
        if (addAdditionalDoorways)
        {
            availableConnectors.AddRange(connections);
        }
        rooms.Add(newRoom);
    }
    public GameObjectRoomTemplate chooseRoomTemplate(GameObjectRoomTemplate existingRoom)
    {
        if (layoutConfiguration.ExcludeSameAdjacentTemplate)
        {
            List<GameObjectRoomTemplate> excludedTemplates = new List<GameObjectRoomTemplate>(layoutConfiguration.Rooms);
            excludedTemplates.Remove(existingRoom);
            return excludedTemplates[random.Next(0, excludedTemplates.Count)];
        }
        else
        {
            return layoutConfiguration.Rooms[random.Next(0, layoutConfiguration.Rooms.Count)];
        }
    }
    public Connection chooseConnection()
    {
        return availableConnectors.ElementAt(random.Next(0, availableConnectors.Count));
    }
    public ConnectionTemplate findOpposingConnection(Connection existingCnt, GameObjectRoomTemplate template)
    {
        Direction oppositeDirection = Connection.getOpposingDirection(existingCnt.Direction);
        List<ConnectionTemplate> opposingConnections = new List<ConnectionTemplate>();
        opposingConnections.AddRange(template.Connectors.Where(cnt => cnt.Direction == oppositeDirection && cnt.Type == existingCnt.ConnectionType).ToList());
        if (opposingConnections.Count > 0)
        {
            return opposingConnections.ElementAt(random.Next(0, opposingConnections.Count));
        }
        return null;
    }
    public Vector3Int findAdjacentPos(Connection existingCnt, ConnectionTemplate newCnt)
    {
        Vector3Int newPosition = newCnt.getLocalIntegerPosition();
        return existingCnt.Position + new Vector3Int(-newPosition.x, -newPosition.y, -newPosition.z);
    }
    public bool checkRoomValidity(Room newRoom)
    {
        if(outOfBounds(newRoom.Position) || overlapping(newRoom.RoomBoundaries))
        {
            return false;
        }
        return true;
    }
    public bool outOfBounds(Vector3Int position)
    {
        return position.x < 0 || position.x > layoutConfiguration.RoomSizeWidth ||
            position.z < 0 || position.z > layoutConfiguration.RoomSizeLength;
    }
    /*
     * Check each boundary of new room against existing rooms, 
     * preventing the new room if an intersection is found
    */
    public bool overlapping(List<Bounds> newRoomBounds)
    {
        foreach (Bounds roomBound in newRoomBounds)
        {
            foreach (Room otherRoom in rooms)
            {
                if (otherRoom.intersectsBounds(roomBound))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
