// ThirdPersonCamera.cs - TOUCHPAD FRIENDLY VERSION
// Replace your current camera script with this version
// Features: Auto-follow, keyboard controls, smooth movement, touchpad safe

using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -5);
    
    [Header("Camera Behavior")]
    [SerializeField] private bool autoFollowPlayer = true;
    [SerializeField] private float autoFollowSpeed = 2f;
    [SerializeField] private float manualRotationSpeed = 100f;
    
    [Header("Mouse Settings (Touchpad)")]
    [SerializeField] private bool enableMouseLook = false; // OFF by default for touchpad
    [SerializeField] private float mouseSensitivity = 0.5f; // Much lower sensitivity
    [SerializeField] private float mouseSmoothing = 10f; // Smooth out touchpad jitter
    
    [Header("Gamepad Settings")]
    [SerializeField] private float gamepadSensitivity = 100f;
    
    [Header("Rotation Limits")]
    [SerializeField] private float minVerticalAngle = -40f;
    [SerializeField] private float maxVerticalAngle = 70f;
    
    [Header("Zoom")]
    [SerializeField] private float currentZoomDistance = 5f;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 10f;
    
    [Header("Collision")]
    [SerializeField] private float collisionRadius = 0.3f;
    [SerializeField] private LayerMask collisionLayers;
    
    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.2f;
    [SerializeField] private float rotationSmoothTime = 0.1f;
    
    private float yaw;
    private float pitch = 20f; // Start at a nice angle
    private Vector3 currentVelocity;
    private float currentYawVelocity;
    private float currentPitchVelocity;
    
    // Input values
    private Vector2 smoothedMouseInput;
    private Vector2 rawMouseInput;
    private float keyboardRotationInput;
    private float keyboardZoomInput;
    
    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                //Debug.Log("Camera found player target!");
            }
            else
            {
                //Debug.LogError("Camera: Player not found! Tag your player as 'Player'");
                return;
            }
        }
        
        // Initialize camera behind player
        Vector3 targetRotation = target.eulerAngles;
        yaw = targetRotation.y;
        
        ////Debug.Log("ThirdPersonCamera initialized - Touchpad friendly mode!");
        ////Debug.Log("Controls: [ and ] to rotate camera, I and O to zoom");
        ////Debug.Log("Auto-follow is ON - camera follows player direction");
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        HandleKeyboardInput();
        HandleCameraRotation();
        HandleZoom();
        HandlePosition();
    }
    
    void HandleKeyboardInput()
    {
        // Keyboard rotation with [ and ] keys
        keyboardRotationInput = 0f;
        
        // Check for [ and ] using Keyboard class from Input System
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.leftBracketKey.isPressed)
            {
                keyboardRotationInput = -1f;
            }
            if (keyboard.rightBracketKey.isPressed)
            {
                keyboardRotationInput = 1f;
            }
            
            // Keyboard zoom with I and O keys
            //if (keyboard.iKey.isPressed)
            //{
            //    keyboardZoomInput = 1f; // Zoom in
            //}
            //if (keyboard.oKey.isPressed)
            //{
            //    keyboardZoomInput = -1f; // Zoom out
            //}
        }
    }
    
    void HandleCameraRotation()
    {
        float targetYaw = yaw;
        float targetPitch = pitch;
        
        // Auto-follow player direction
        if (autoFollowPlayer && Mathf.Approximately(keyboardRotationInput, 0f) && 
            smoothedMouseInput.magnitude < 0.1f)
        {
            targetYaw = target.eulerAngles.y;
        }
        
        // Keyboard rotation (overrides auto-follow)
        if (Mathf.Abs(keyboardRotationInput) > 0.01f)
        {
            targetYaw += keyboardRotationInput * manualRotationSpeed * Time.deltaTime;
        }
        
        // Mouse/touchpad input (optional, smoothed heavily)
        if (enableMouseLook && smoothedMouseInput.magnitude > 0.01f)
        {
            targetYaw += smoothedMouseInput.x * mouseSensitivity;
            targetPitch -= smoothedMouseInput.y * mouseSensitivity;
            targetPitch = Mathf.Clamp(targetPitch, minVerticalAngle, maxVerticalAngle);
        }
        
        // Smooth rotation
        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref currentYawVelocity, rotationSmoothTime);
        pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref currentPitchVelocity, rotationSmoothTime);
    }
    
    void HandleZoom()
    {
        // Keyboard zoom
        if (Mathf.Abs(keyboardZoomInput) > 0.01f)
        {
            currentZoomDistance -= keyboardZoomInput * zoomSpeed * Time.deltaTime * 5f;
            currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoom, maxZoom);
        }
    }
    
    void HandlePosition()
    {
        // Calculate desired position
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = target.position - (rotation * Vector3.forward * currentZoomDistance);
        desiredPosition += Vector3.up * offset.y;
        
        // Check for collisions
        Vector3 direction = desiredPosition - target.position;
        RaycastHit hit;
        
        if (Physics.SphereCast(target.position, collisionRadius, direction.normalized, 
            out hit, direction.magnitude, collisionLayers))
        {
            desiredPosition = target.position + direction.normalized * (hit.distance - collisionRadius);
        }
        
        // Smooth camera movement (prevents bouncing)
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, 
            ref currentVelocity, positionSmoothTime);
        
        // Look at target
        Vector3 lookPosition = target.position + Vector3.up * offset.y;
        transform.LookAt(lookPosition);
    }
    
    // Called by CameraInputRelay from mouse/gamepad
    public void ReceiveLookInput(Vector2 input)
    {
        rawMouseInput = input;
        
        // Smooth out mouse input (helps with touchpad jitter)
        smoothedMouseInput = Vector2.Lerp(smoothedMouseInput, rawMouseInput, 
            Time.deltaTime * mouseSmoothing);
    }
    
    // Called by CameraInputRelay for scroll wheel zoom
    public void ReceiveZoomInput(float input)
    {
        if (Mathf.Abs(input) > 0.01f)
        {
            currentZoomDistance -= input * zoomSpeed;
            currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoom, maxZoom);
        }
    }
    
    // Public method to toggle mouse look on/off
    public void SetMouseLookEnabled(bool enabled)
    {
        enableMouseLook = enabled;
        //Debug.Log($"Camera mouse look: {(enabled ? "ENABLED" : "DISABLED")}");
    }
    
    // Public method to toggle auto-follow
    public void SetAutoFollow(bool enabled)
    {
        autoFollowPlayer = enabled;
        //Debug.Log($"Camera auto-follow: {(enabled ? "ENABLED" : "DISABLED")}");
    }
    
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, collisionRadius);
            
            Gizmos.color = Color.cyan;
            Vector3 direction = transform.position - target.position;
            Gizmos.DrawLine(target.position, target.position + direction);
        }
    }
}