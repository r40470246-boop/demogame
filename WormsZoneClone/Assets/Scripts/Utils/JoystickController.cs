using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Mobile Joystick Controller — worm ko move karne ke liye
/// UI mein ek circle banao aur is script ko attach karo
/// </summary>
public class JoystickController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Joystick Settings")]
    public float handleRange = 1f;        // Kitna dur tak handle move ho sakta hai
    public float deadZone = 0f;           // Minimum input threshold
    public bool hideOnRelease = false;    // Release pe hide karo?

    [Header("References")]
    public RectTransform background;      // Joystick background circle
    public RectTransform handle;          // Joystick handle (inner circle)

    // Public - WormMovement script ye padega
    public Vector2 Direction { get; private set; }
    public bool IsPressed { get; private set; }

    private Canvas canvas;
    private Camera cam;
    private Vector2 input = Vector2.zero;
    private RectTransform baseRect;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        baseRect = GetComponent<RectTransform>();

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvas.worldCamera;

        // Shuru mein center pe rakho
        if (handle != null)
            handle.anchoredPosition = Vector2.zero;

        if (hideOnRelease && background != null)
            background.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;

        if (hideOnRelease && background != null)
            background.gameObject.SetActive(true);

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
        Vector2 radius = background.sizeDelta / 2f;

        input = (eventData.position - position) / (radius * canvas.scaleFactor);

        // Dead zone check
        if (input.magnitude > deadZone)
        {
            // Circle ke andar rakho (clamp)
            if (input.magnitude > 1f)
                input = input.normalized;

            Direction = input;
        }
        else
        {
            Direction = Vector2.zero;
        }

        // Handle ko move karo visually
        if (handle != null)
        {
            Vector2 handlePos = new Vector2(
                input.x * (background.sizeDelta.x / 2f) * handleRange,
                input.y * (background.sizeDelta.y / 2f) * handleRange
            );
            handle.anchoredPosition = handlePos;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
        input = Vector2.zero;
        Direction = Vector2.zero;

        // Handle wapas center pe
        if (handle != null)
            handle.anchoredPosition = Vector2.zero;

        if (hideOnRelease && background != null)
            background.gameObject.SetActive(false);
    }

    /// <summary>
    /// Horizontal input (-1 to 1)
    /// </summary>
    public float Horizontal => Direction.x;

    /// <summary>
    /// Vertical input (-1 to 1)
    /// </summary>
    public float Vertical => Direction.y;
}
