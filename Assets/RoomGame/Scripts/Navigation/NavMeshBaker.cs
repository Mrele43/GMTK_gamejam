using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;
public class NavMeshBaker : MonoBehaviour
{
    [Header("NavMeshLink Settings")]
    [SerializeField] private int linkWidth = 2;
    [SerializeField] private float linkHorizontalLength = 2f;
    [SerializeField] private float floorHeight = 4f;
    public void BakeAndInstallLinks(Level level)
    {
        if (level?.RoomManagers == null || level.RoomManagers.Count == 0)
        {
            Debug.LogError("NavMeshBaker: No room managers found");
            return;
        }
        AddDoorLadderInteraction(level);
        foreach (RoomManager rm in level.RoomManagers)
        {
            BakeRoomSurface(rm.gameObject);
        }
        Debug.Log($"NavMeshBaker: Baked {level.RoomManagers.Count} room NavMeshes");
        InstallConnectorLinks(level);
    }
    private void BakeRoomSurface(GameObject roomGO)
    {
        NavMeshSurface surface = roomGO.GetComponentInChildren<NavMeshSurface>();
        if (surface == null)
        {
            Debug.LogWarning($"NavMeshBaker: No NavMeshSurface on {roomGO.name}");
            return;
        }
        surface.navMeshData = new NavMeshData();
        surface.BuildNavMesh();
    }
    private void AddDoorLadderInteraction(Level level)
    {
        if (level?.Rooms == null) return;
        int doorCount = 0, ladderCount = 0;
        HashSet<GameObject> processed = new HashSet<GameObject>();
        foreach (Room room in level.Rooms)
        {
            if (room.Connections == null) continue;
            foreach (Connection conn in room.Connections)
            {
                if (!conn.Open || conn.CreatedObject == null) continue;
                if (processed.Contains(conn.CreatedObject)) continue;
                processed.Add(conn.CreatedObject);
                GameObject obj = conn.CreatedObject;
                if (conn.ConnectionType == ConnectionType.Door && obj.GetComponent<InteractableDoor>() == null)
                {
                    obj.AddComponent<InteractableDoor>();
                    doorCount++;
                }
                else if (conn.ConnectionType == ConnectionType.Ladder && obj.GetComponent<InteractableLadder>() == null)
                {
                    obj.AddComponent<InteractableLadder>();
                    ladderCount++;
                }
            }
        }
        if (doorCount > 0 || ladderCount > 0)
            Debug.Log($"NavMeshBaker: Added interaction to {doorCount} doors + {ladderCount} ladders");
    }
    private void InstallConnectorLinks(Level level)
    {
        if (level.Rooms == null) return;
        int doorCount = 0, verticalDoorCount = 0, ladderCount = 0;
        HashSet<Connection> processed = new HashSet<Connection>();
        foreach (Room room in level.Rooms)
        {
            if (room.Connections == null) continue;
            foreach (Connection conn in room.Connections)
            {
                if (processed.Contains(conn) || !conn.Open) continue;
                processed.Add(conn);
                if (conn.ConnectionType == ConnectionType.Ladder)
                {
                    CreateLadderLink(conn);
                    ladderCount++;
                }
                else if (conn.ConnectionType == ConnectionType.Door)
                {
                    if (conn.Direction == Direction.Up || conn.Direction == Direction.Down)
                    {
                        CreateVerticalDoorLink(conn);
                        verticalDoorCount++;
                    }
                    else
                    {
                        CreateHorizontalLink(conn);
                        doorCount++;
                    }
                }
            }
        }
        Debug.Log($"NavMeshBaker: Links created �?{doorCount} horizontal doors + {verticalDoorCount} stair/ladder doors + {ladderCount} ladders");
    }
    private void CreateHorizontalLink(Connection conn)
    {
        Vector3 pos = conn.Position;
        Vector3 dir = GetDirectionVector(conn.Direction);
        GameObject go = new GameObject($"NavMeshLink_Door_{pos}");
        go.transform.position = pos;
        go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        NavMeshLink link = go.AddComponent<NavMeshLink>();
        link.startPoint = -dir * linkHorizontalLength;
        link.endPoint = dir * linkHorizontalLength;
        link.width = linkWidth;
        link.costModifier = 1;
        link.autoUpdate = true;
        link.bidirectional = true;
        if (conn.CreatedObject != null)
            go.transform.SetParent(conn.CreatedObject.transform);
        link.UpdateLink();
    }
    private void CreateVerticalDoorLink(Connection conn)
    {
        Vector3 pos = conn.Position;
        GameObject go = new GameObject($"NavMeshLink_StairDoor_{pos}");
        go.transform.position = pos;
        NavMeshLink link = go.AddComponent<NavMeshLink>();
        link.startPoint = Vector3.zero;
        if (conn.Direction == Direction.Up)
            link.endPoint = new Vector3(0, floorHeight, 0);
        else
            link.endPoint = new Vector3(0, -floorHeight, 0);
        link.width = linkWidth;
        link.costModifier = 1;
        link.autoUpdate = true;
        link.bidirectional = true;
        if (conn.CreatedObject != null)
            go.transform.SetParent(conn.CreatedObject.transform);
        link.UpdateLink();
    }
    private void CreateLadderLink(Connection conn)
    {
        Vector3 pos = conn.Position;
        GameObject go = new GameObject($"NavMeshLink_Ladder_{pos}");
        go.transform.position = pos;
        NavMeshLink link = go.AddComponent<NavMeshLink>();
        link.startPoint = Vector3.zero;
        if (conn.Direction == Direction.Up)
            link.endPoint = new Vector3(0, floorHeight, 0);
        else
            link.endPoint = new Vector3(0, -floorHeight, 0);
        link.width = linkWidth;
        link.costModifier = 2;
        link.autoUpdate = true;
        link.bidirectional = true;
        if (conn.CreatedObject != null)
            go.transform.SetParent(conn.CreatedObject.transform);
        link.UpdateLink();
    }
    private static Vector3 GetDirectionVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Vector3.forward;
            case Direction.South: return Vector3.back;
            case Direction.West: return Vector3.left;
            case Direction.East: return Vector3.right;
            default: return Vector3.forward;
        }
    }
}