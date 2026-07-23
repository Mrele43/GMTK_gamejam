using UnityEngine;

public class RoomAreaManager : MonoBehaviour
{
    public static RoomAreaManager Instance { get; private set; }

    [System.Serializable]
    public class RoomArea
    {
        public string roomName;
        public Transform areaTransform;
        public Vector3 size;
        public Color gizmoColor = Color.blue;
    }

    [Header("Room Areas")]
    [SerializeField] private RoomArea[] _roomAreas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public string GetRoomName(Vector3 position)
    {
        foreach (var area in _roomAreas)
        {
            if (IsPointInArea(position, area))
            {
                return area.roomName;
            }
        }
        return "Unknown";
    }

    public bool IsPointInRoom(Vector3 position, string roomName)
    {
        foreach (var area in _roomAreas)
        {
            if (area.roomName == roomName && IsPointInArea(position, area))
            {
                return true;
            }
        }
        return false;
    }

    public Vector3 GetRoomCenter(string roomName)
    {
        foreach (var area in _roomAreas)
        {
            if (area.roomName == roomName)
            {
                return area.areaTransform.position;
            }
        }
        return Vector3.zero;
    }

    private bool IsPointInArea(Vector3 point, RoomArea area)
    {
        if (area.areaTransform == null)
            return false;

        Vector3 localPoint = area.areaTransform.InverseTransformPoint(point);

        return Mathf.Abs(localPoint.x) <= area.size.x * 0.5f &&
               Mathf.Abs(localPoint.y) <= area.size.y * 0.5f &&
               Mathf.Abs(localPoint.z) <= area.size.z * 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var area in _roomAreas)
        {
            if (area.areaTransform == null)
                continue;

            Gizmos.color = area.gizmoColor;
            Gizmos.DrawWireCube(area.areaTransform.position, area.size);

            GUIStyle style = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = area.gizmoColor }
            };

            Vector3 labelPos = area.areaTransform.position + Vector3.up * (area.size.y * 0.5f + 0.5f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, area.roomName, style);
#endif
        }
    }
}