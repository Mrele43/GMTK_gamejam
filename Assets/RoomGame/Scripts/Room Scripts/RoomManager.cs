using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class RoomManager : MonoBehaviour
{
    [SerializeField] GameObject startPosition;
    List<GameObject> connectorObjectArray;
    [SerializeField] List<Window> windows;
    public Vector3 StartPosition => startPosition.transform.position;
    public bool PlayerWithin {  get; set; }
    public Vector3Int RoomPosition { get; set; }
    public List<RoomTypes> RoomTypes { get; set; }
    public List<GameObject> ConnectorObjects => connectorObjectArray;
    public void triggerConnectors(List<Connection> connections)
    {
        GameObjectRoomTemplate roomTemplate = GetComponent<GameObjectRoomTemplate>();
        connectorObjectArray = roomTemplate.extractConnectorObjects();
        for (int i = 0; i < connections.Count; i++) 
        {
            Connection cnt = connections[i];
            if (cnt.Open) 
            {
                connectorObjectArray[i].SetActive(false);
                createConnection(cnt);
            }
            else
            {
                try
                {
                    connectorObjectArray[i].SetActive(true);
                    connectorObjectArray[i].transform.GetChild(0).gameObject.SetActive(true);
                }
                catch (Exception e) 
                {
                    continue;
                }
            }
        }
    }
    public void createConnection(Connection cnt)
    {
        if (!cnt.Created)
        {
            int doorRotation = Connection.getConnectionPrefabRotation(cnt.Direction);
            GameObject doorPrefab = AdjacentRoomsGeneration.ConnectorObjects[cnt.ConnectionType];
            cnt.CreatedObject = Instantiate(doorPrefab, cnt.Position, Quaternion.Euler(0, doorRotation, 0));
            cnt.Created = true;
        }
    }
    public void triggerWindows()
    {
        foreach (Window window in windows)
        {
            bool excludeWindow = false;
            window.WindowObject.SetActive(true);
            Vector3 windowDirection = window.getWindowRaycastDirection();
            foreach (GameObject windowPoint in window.WindowRayCastPoint)
            {
                Vector3 windowPosition = windowPoint.transform.position;
                if (Physics.Raycast(windowPosition, windowDirection, 5f))
                {
                    excludeWindow = true;
                    break;
                }
            }
            Renderer windowRender = window.WindowObject.GetComponent<Renderer>();
            if (excludeWindow)
            {
                windowRender.material = AdjacentRoomsGeneration.WallMaterial;
            }
            else
            {
                windowRender.material = AdjacentRoomsGeneration.WindowMaterial;
            }
        }
    } 
}
