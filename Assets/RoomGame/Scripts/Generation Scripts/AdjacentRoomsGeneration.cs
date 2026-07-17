using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class AdjacentRoomsGeneration : MonoBehaviour
{
    [SerializeField] Material windowMaterial;
    [SerializeField] Material wallMaterial;
    static Material staticWindowMaterial; 
    static Material staticWallMaterial; 
    public static Material WindowMaterial => staticWindowMaterial;
    public static Material WallMaterial => staticWallMaterial;
    [SerializeField] GameObject doorPrefab;
    [SerializeField] GameObject ladderPrefab;
    static GameObject staticDoorPrefab; 
    static GameObject staticLadderPrefab; 
    public static GameObject DoorPrefab => staticDoorPrefab;
    public static GameObject LadderPrefab => staticLadderPrefab;
    public static Dictionary<ConnectionType, GameObject> connectorObjects;
    public static Dictionary<ConnectionType, GameObject> ConnectorObjects => connectorObjects;
    void Awake()
    {
        staticWallMaterial = wallMaterial;
        staticWindowMaterial = windowMaterial;
        connectorObjects = new Dictionary<ConnectionType, GameObject>
        {
            {ConnectionType.Door, doorPrefab },
            {ConnectionType.Ladder, ladderPrefab }
        };
    }
    public void createLevel(Level level)
    {
        List<RoomManager> roomManagerList = new List<RoomManager>();
        foreach (Room room in level.Rooms)
        {
            GameObject roomObject = Instantiate(room.RoomObject, room.Position, Quaternion.identity);
            RoomManager rm = roomObject.GetComponent<RoomManager>();
            rm.RoomPosition = room.Position;
            rm.RoomTypes = room.RoomTypes;
            rm.triggerConnectors(room.Connections);
            roomManagerList.Add(rm);
        }
        foreach(RoomManager rm in roomManagerList)
        {
            rm.triggerWindows();
        }
        level.RoomManagers = roomManagerList;
    }
}
