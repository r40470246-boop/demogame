using UnityEngine;

/// <summary>
/// CameraFollow — Camera worm ke head ke peeche smoothly follow karta hai
/// Main Camera pe attach karo
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;              // Worm head assign karo
    public float smoothSpeed = 8f;       // Camera follow speed

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 0, -10f);  // Camera ka offset
    public float minZoom = 5f;           // Minimum zoom
    public float maxZoom = 12f;          // Maximum zoom (worm bade hone pe zoom out)
    public float zoomSpeed = 2f;

    [Header("Map Bounds")]
    public bool clampToMap = true;
    public float mapMinX = -50f;
    public float mapMaxX = 50f;
    public float mapMinY = -50f;
    public float mapMaxY = 50f;

    private Camera cam;
    private float targetZoom;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        targetZoom = (minZoom + maxZoom) / 2f;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Smooth follow
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Map bounds clamp karo
        if (clampToMap)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, mapMinX, mapMaxX);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, mapMinY, mapMaxY);
        }

        smoothedPosition.z = offset.z;
        transform.position = smoothedPosition;

        // Smooth zoom
        if (cam != null)
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Worm ka size dekhke zoom adjust karo
    /// WormBody.cs se call hoga
    /// </summary>
    public void SetZoomForSize(int segments)
    {
        // Jitna bada worm, utna zoom out
        float t = Mathf.InverseLerp(5, 100, segments);
        targetZoom = Mathf.Lerp(minZoom, maxZoom, t);
    }

    /// <summary>
    /// Naya target set karo (multiplayer spawn ke baad)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
