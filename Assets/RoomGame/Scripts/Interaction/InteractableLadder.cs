using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
public class InteractableLadder : MonoBehaviour
{
    [Header("Ladder Settings")]
    [SerializeField] private float floorHeight = 4f;
    [SerializeField] private float climbOffset = 0.5f; 
    [SerializeField] private string promptText = "Press E to climb";
    private bool playerInRange = false;
    private void Awake()
    {
        Collider[] colliders = GetComponents<Collider>();
        bool hasTrigger = false;
        foreach (var c in colliders)
        {
            if (c.isTrigger) { hasTrigger = true; break; }
        }
        if (!hasTrigger)
        {
            BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(4f, floorHeight + 2f, 4f);
            trigger.center = new Vector3(0, floorHeight / 2f, 0);
        }
    }
    private void Update()
    {
        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Climb();
        }
    }
    private void Climb()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        Vector3 playerPos = player.transform.position;
        Vector3 ladderPos = transform.position;
        float ladderMidY = ladderPos.y + floorHeight / 2f;
        bool goingUp = playerPos.y < ladderMidY;
        Vector3 destination;
        if (goingUp)
        {
            destination = new Vector3(ladderPos.x, ladderPos.y + floorHeight + climbOffset, ladderPos.z);
            Debug.Log("Ladder: Climbing UP");
        }
        else
        {
            destination = new Vector3(ladderPos.x, ladderPos.y + climbOffset, ladderPos.z);
            Debug.Log("Ladder: Climbing DOWN");
        }
        MovePlayer(player, destination);
        ShowClimbMessage(goingUp);
    }
    private void MovePlayer(GameObject player, Vector3 destination)
    {
        NavMeshAgentController controller = player.GetComponent<NavMeshAgentController>();
        if (controller != null)
        {
            controller.SetMode(NavMeshAgentController.MoveMode.Manual);
        }
        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }
        player.transform.position = destination;
        if (cc != null) cc.enabled = true;
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(destination);
        }
    }
    private void ShowClimbMessage(bool goingUp)
    {
        Debug.Log($"Player climbed {(goingUp ? "up" : "down")} the ladder");
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
            float labelWidth = 180;
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