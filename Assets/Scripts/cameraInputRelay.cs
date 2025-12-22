// CameraInputRelay.cs
// Add this to the PLAYER GameObject
// This relays input from Player to Camera

using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInputRelay : MonoBehaviour
{
    private ThirdPersonCamera cameraScript;
    
    void Start()
    {
        // Updated to use FindFirstObjectByType (Unity 2023+)
        cameraScript = FindFirstObjectByType<ThirdPersonCamera>();
        
        if (cameraScript == null)
        {
            //Debug.LogError("CameraInputRelay: Could not find ThirdPersonCamera in scene!");
        }
        else
        {
            ////Debug.Log("CameraInputRelay: Successfully connected to camera!");
            ////Debug.Log("Camera Controls:");
            ////Debug.Log("  [ and ] keys - Rotate camera");
            ////Debug.Log("  I and O keys - Zoom in/out");
            ////Debug.Log("  Mouse Scroll - Also zooms");
            ////Debug.Log("  Right Stick (Gamepad) - Rotate camera");
        }
    }
    
    // Receives Look input from PlayerInput (mouse/gamepad right stick)
    public void OnLook(InputValue value)
    {
        if (cameraScript != null)
        {
            Vector2 lookInput = value.Get<Vector2>();
            cameraScript.ReceiveLookInput(lookInput);
        }
    }
    
    // Receives Zoom input from PlayerInput (scroll wheel)
    public void OnZoom(InputValue value)
    {
        if (cameraScript != null)
        {
            float zoomInput = value.Get<float>();
            cameraScript.ReceiveZoomInput(zoomInput);
        }
    }
}