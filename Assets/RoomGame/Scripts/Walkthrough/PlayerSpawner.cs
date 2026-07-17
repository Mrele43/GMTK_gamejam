using UnityEngine;
using UnityEngine.AI;
using StarterAssets;
public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Prefabs (Starter Assets)")]
    [SerializeField] private GameObject playerCapsulePrefab;
    [SerializeField] private GameObject followCameraPrefab;
    [Header("Runtime Setup")]
    [SerializeField] private float walkSpeed = 4.0f;
    public NavMeshAgentController SpawnPlayer(Level level)
    {
        Vector3 spawnPos = GetSpawnPosition(level);
        Debug.Log("PlayerSpawner: Spawning at " + spawnPos);
        GameObject playerGO = CreatePlayer(spawnPos);
        SetupMainCamera(playerGO);
        if (followCameraPrefab != null)
        {
            SetupFollowCamera(playerGO);
        }
        NavMeshAgentController controller = playerGO.GetComponent<NavMeshAgentController>();
        controller.Initialize();
        return controller;
    }
    private Vector3 GetSpawnPosition(Level level)
    {
        if (level.StartRoom != null && level.RoomManagers != null && level.RoomManagers.Count > 0)
        {
            RoomManager startRoomManager = level.GetRoomManager(level.StartRoom);
            if (startRoomManager != null)
            {
                return startRoomManager.StartPosition;
            }
        }
        if (level.StartRoom != null)
        {
            return new Vector3(level.StartRoom.Position.x + 9f, 1f, level.StartRoom.Position.z + 9f);
        }
        return Vector3.zero;
    }
    private GameObject CreatePlayer(Vector3 position)
    {
        GameObject playerGO;
        if (playerCapsulePrefab != null)
        {
            playerGO = Instantiate(playerCapsulePrefab, position, Quaternion.identity);
            Debug.Log("PlayerSpawner: Spawned PlayerCapsule from Starter Assets");
        }
        else
        {
            playerGO = BuildFallbackPlayer(position);
        }
        NavMeshAgent agent = playerGO.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = playerGO.AddComponent<NavMeshAgent>();
            agent.radius = 0.5f;
            agent.height = 2.0f;
            agent.speed = walkSpeed;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.5f;
            agent.autoBraking = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }
        if (playerGO.GetComponent<NavMeshAgentController>() == null)
        {
            playerGO.AddComponent<NavMeshAgentController>();
        }
        return playerGO;
    }
    private GameObject BuildFallbackPlayer(Vector3 position)
    {
        GameObject playerGO = new GameObject("PlayerCapsule (Fallback)");
        playerGO.transform.position = position;
        playerGO.layer = 8;
        playerGO.tag = "Player";
        CharacterController cc = playerGO.AddComponent<CharacterController>();
        cc.height = 2;
        cc.radius = 0.5f;
        cc.center = new Vector3(0, 0.93f, 0);
        cc.slopeLimit = 45;
        cc.stepOffset = 0.25f;
        GameObject cameraTarget = new GameObject("PlayerCameraRoot");
        cameraTarget.transform.SetParent(playerGO.transform);
        cameraTarget.transform.localPosition = new Vector3(0, 1.375f, 0);
        cameraTarget.tag = "CinemachineTarget";
        StarterAssetsInputs input = playerGO.AddComponent<StarterAssetsInputs>();
        input.cursorLocked = true;
        input.cursorInputForLook = true;
        FirstPersonController fps = playerGO.AddComponent<FirstPersonController>();
        fps.MoveSpeed = walkSpeed;
        fps.SprintSpeed = 6.0f;
        fps.RotationSpeed = 1.0f;
        fps.CinemachineCameraTarget = cameraTarget;
        fps.GroundedOffset = -0.14f;
        fps.GroundLayers = 1;
        Debug.Log("PlayerSpawner: Built fallback player (no prefab assigned)");
        return playerGO;
    }
    private void SetupMainCamera(GameObject playerGO)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("PlayerSpawner: No Main Camera found");
            return;
        }
        Transform cameraTarget = FindCameraTarget(playerGO.transform);
        if (cameraTarget == null)
        {
            Debug.LogWarning("PlayerSpawner: No camera target found on player");
            return;
        }
        if (mainCam.transform.IsChildOf(playerGO.transform))
            return;
        mainCam.transform.SetParent(cameraTarget);
        mainCam.transform.localPosition = Vector3.zero;
        mainCam.transform.localRotation = Quaternion.identity;
        mainCam.nearClipPlane = 0.2f;
        mainCam.fieldOfView = 60f;
        mainCam.tag = "MainCamera";
        Debug.Log("PlayerSpawner: Camera attached to player camera target");
    }
    private void SetupFollowCamera(GameObject playerGO)
    {
        Transform cameraTarget = FindCameraTarget(playerGO.transform);
        if (cameraTarget == null) return;
        GameObject followCam = Instantiate(followCameraPrefab);
        var vcam = followCam.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        if (vcam != null)
        {
            vcam.Follow = cameraTarget;
            vcam.LookAt = cameraTarget;
        }
    }
    private Transform FindCameraTarget(Transform playerRoot)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("CinemachineTarget");
        foreach (GameObject t in targets)
        {
            if (t.transform.IsChildOf(playerRoot))
                return t.transform;
        }
        foreach (Transform child in playerRoot.GetComponentsInChildren<Transform>())
        {
            if (child.name.Contains("CameraRoot") || child.name.Contains("CameraTarget"))
                return child;
        }
        return playerRoot;
    }
}
