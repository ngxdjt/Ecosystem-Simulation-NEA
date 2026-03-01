using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
    public float zoomSpeed = 1f;
    public float minZoom = 2f;
    public float maxZoom = 10f;

    private Camera cam;
    private Vector2 scrollInput;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
            Debug.LogWarning("CameraZoom is designed for orthographic cameras.");
    }

    // Called automatically by the Input System when scroll input occurs
    public void OnZoom(InputAction.CallbackContext context)
    {
        scrollInput = context.ReadValue<Vector2>(); // y-axis of scroll
    }

    private void Update()
    {
        if (scrollInput.y != 0f)
        {
            cam.orthographicSize -= scrollInput.y * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }

        // Reset scroll input after applying (so it's not applied continuously)
        scrollInput = Vector2.zero;
    }
}