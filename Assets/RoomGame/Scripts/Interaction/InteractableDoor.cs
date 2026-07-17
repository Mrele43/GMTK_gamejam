using UnityEngine;
using UnityEngine.InputSystem;
public class InteractableDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float animationSpeed = 3f;
    [SerializeField] private string promptText = "Press E to open/close";
    [Header("References")]
    [SerializeField] private Transform doorBody;
    [SerializeField] private BoxCollider doorCollider;
    private bool isOpen = false;
    private bool playerInRange = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;
    private void Awake()
    {
        if (doorBody == null)
        {
            Transform body = transform.Find("DoorBody");
            if (body != null) doorBody = body;
        }
        if (doorCollider == null && doorBody != null)
        {
            doorCollider = doorBody.GetComponentInChildren<BoxCollider>();
            if (doorCollider != null) doorCollider.isTrigger = false;
        }
        closedRotation = doorBody != null ? doorBody.localRotation : Quaternion.identity;
        openRotation = Quaternion.Euler(0, openAngle, 0);
        targetRotation = closedRotation;
        EnsureTriggerCollider();
    }
    private void EnsureTriggerCollider()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (var c in colliders)
        {
            if (c.isTrigger) return; 
        }
        BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(3f, 3f, 4f);
    }
    private void Update()
    {
        if (doorBody != null)
        {
            doorBody.localRotation = Quaternion.Slerp(
                doorBody.localRotation, targetRotation,
                Time.deltaTime * animationSpeed
            );
        }
        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleDoor();
        }
    }
    private void ToggleDoor()
    {
        isOpen = !isOpen;
        targetRotation = isOpen ? openRotation : closedRotation;
        Debug.Log($"Door {(isOpen ? "opened" : "closed")}");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
    private void OnGUI()
    {
        if (playerInRange)
        {
            float labelWidth = 200;
            float labelHeight = 30;
            Rect rect = new Rect(
                Screen.width / 2f - labelWidth / 2f,
                Screen.height / 2f + 30,
                labelWidth,
                labelHeight
            );
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 16;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;
            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(rect, promptText, style);
        }
    }
}