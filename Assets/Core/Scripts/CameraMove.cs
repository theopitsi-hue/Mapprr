using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [Header("Pan Settings")]
    public float panSpeed = 20f;
    public int panMouseButton = 2; // 2 = middle mouse, 1 = right mouse

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 15f;

    private Camera cam;
    private Vector3 lastMousePosition;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (!cam.orthographic)
        {
            Debug.LogWarning("CameraPanZoom2D is designed for orthographic cameras.");
        }
    }

    void Update()
    {
        HandlePan();
        HandleZoom();
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(panMouseButton))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(panMouseButton))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            // Convert screen movement to world movement
            Vector3 move = new Vector3(-delta.x, -delta.y, 0f) * panSpeed * Time.deltaTime;

            transform.Translate(move);

            lastMousePosition = Input.mousePosition;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

}