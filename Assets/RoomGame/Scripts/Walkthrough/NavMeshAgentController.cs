using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using StarterAssets;
public class NavMeshAgentController : MonoBehaviour
{
    public enum MoveMode { Manual, Auto }
    [Header("References")]
    [SerializeField] private FirstPersonController fpsController;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private StarterAssetsInputs input;
    [SerializeField] private Transform cameraTarget;
    [Header("Mouse Look (Auto Mode)")]
    [SerializeField] private float lookSpeed = 2.0f;
    [SerializeField] private float topClamp = 89f;
    [SerializeField] private float bottomClamp = -89f;
    private MoveMode currentMode = MoveMode.Manual;
    private float verticalRotation;
    private bool cursorLocked = true;
    private bool wasEscapePressed;
    private bool wasLeftClickPressed;
    public MoveMode Mode => currentMode;
    public NavMeshAgent Agent => agent;
    private void Awake()
    {
        if (fpsController == null) fpsController = GetComponent<FirstPersonController>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (input == null) input = GetComponent<StarterAssetsInputs>();
        if (cameraTarget == null)
        {
            GameObject target = GameObject.FindGameObjectWithTag("CinemachineTarget");
            if (target != null) cameraTarget = target.transform;
        }
        agent.radius = 0.5f;
        agent.height = 2.0f;
        agent.speed = 4.0f;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.5f;
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.enabled = false;
    }
    public void Initialize()
    {
        SetMode(MoveMode.Manual);
        Cursor.lockState = CursorLockMode.Locked;
        cursorLocked = true;
    }
    private void Update()
    {
        HandleCursorLock();
        if (currentMode == MoveMode.Auto && HasMovementInput())
        {
            SwitchToManual();
        }
        if (currentMode == MoveMode.Auto)
        {
            HandleMouseLook();
        }
    }
    private bool HasMovementInput()
    {
        return input.move.sqrMagnitude > 0.01f;
    }
    private void HandleCursorLock()
    {
        if (Keyboard.current != null)
        {
            bool escapePressed = Keyboard.current.escapeKey.wasPressedThisFrame;
            if (escapePressed && !wasEscapePressed)
            {
                cursorLocked = !cursorLocked;
                Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !cursorLocked;
            }
            wasEscapePressed = escapePressed;
        }
        if (!cursorLocked && Mouse.current != null)
        {
            bool leftClick = Mouse.current.leftButton.wasPressedThisFrame;
            if (leftClick && !wasLeftClickPressed)
            {
                cursorLocked = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            wasLeftClickPressed = leftClick;
        }
    }
    private void HandleMouseLook()
    {
        if (!cursorLocked || Mouse.current == null) return;
        Vector2 delta = Mouse.current.delta.ReadValue() * (lookSpeed / 10f);
        transform.Rotate(0, delta.x, 0);
        if (cameraTarget != null)
        {
            verticalRotation -= delta.y;
            verticalRotation = Mathf.Clamp(verticalRotation, bottomClamp, topClamp);
            cameraTarget.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
    }
    public void SetMode(MoveMode mode)
    {
        if (mode == currentMode) return;
        if (mode == MoveMode.Auto) SwitchToAuto();
        else SwitchToManual();
    }
    private void SwitchToAuto()
    {
        currentMode = MoveMode.Auto;
        if (fpsController != null) fpsController.enabled = false;
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }
    }
    private void SwitchToManual()
    {
        currentMode = MoveMode.Manual;
        if (agent != null)
        {
            agent.ResetPath();
            agent.enabled = false;
        }
        if (fpsController != null) fpsController.enabled = true;
    }
}
