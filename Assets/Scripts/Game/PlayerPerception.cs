using UnityEngine;

public class PlayerPerception : MonoBehaviour
{
    public static PlayerPerception Instance { get; private set; }

    [Header("Flashlight Settings")]
    [Tooltip("手电是否开启")]
    [SerializeField] private bool _flashlightEnabled = false;

    [Tooltip("手电开关按键")]
    [SerializeField] private KeyCode _flashlightKey = KeyCode.F;

    [Tooltip("手电光源")]
    [SerializeField] private Light _flashlightLight;

    [Tooltip("手电照射角度")]
    [SerializeField] private float _flashlightAngle = 30f;

    [Tooltip("手电照射距离")]
    [SerializeField] private float _flashlightRange = 15f;

    public bool IsFlashlightOn => _flashlightEnabled;
    public Light FlashlightLight => _flashlightLight;
    public float FlashlightAngle => _flashlightAngle;
    public float FlashlightRange => _flashlightRange;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_flashlightLight == null)
        {
            _flashlightLight = GetComponentInChildren<Light>();
        }

        UpdateFlashlightState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(_flashlightKey))
        {
            _flashlightEnabled = !_flashlightEnabled;
            UpdateFlashlightState();
        }
    }

    private void UpdateFlashlightState()
    {
        if (_flashlightLight != null)
        {
            _flashlightLight.enabled = _flashlightEnabled;
            _flashlightLight.spotAngle = _flashlightAngle;
            _flashlightLight.range = _flashlightRange;
        }
    }

    public bool IsPointInFlashlightCone(Vector3 point)
    {
        if (!_flashlightEnabled || _flashlightLight == null)
            return false;

        Vector3 toPoint = point - transform.position;
        float distance = toPoint.magnitude;

        if (distance > _flashlightRange)
            return false;

        toPoint.y = 0f;
        Vector3 forward = transform.forward;
        forward.y = 0f;

        float angle = Vector3.Angle(forward, toPoint);
        return angle <= _flashlightAngle * 0.5f;
    }
}