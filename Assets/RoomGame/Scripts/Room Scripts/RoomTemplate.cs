using System;
using System.Collections.Generic;
using UnityEngine;
public class GameObjectRoomTemplate:MonoBehaviour
{
    [SerializeField] GameObject roomGameObject;
    [SerializeField] List<ConnectionTemplate> connectors;
    [SerializeField] List<GameObject> colliderPoints;
    public GameObject RoomGameObject => roomGameObject;
    public List<ConnectionTemplate> Connectors => connectors;
    public List<GameObject> ColliderPoints => colliderPoints;
    public List<GameObject> extractConnectorObjects()
    {
        List<GameObject> connectorObjects = new List<GameObject>();
        foreach(ConnectionTemplate tp  in connectors)
        {
            connectorObjects.Add(tp.GameObjectPoint);
        }
        return connectorObjects;
    }
}