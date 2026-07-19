using UnityEngine;
using Photon.Pun;

/// <summary>
/// WormMovement — Worm ke head ka movement control karta hai
/// Joystick se direction leke smooth movement karta hai
/// Photon PUN 2 ke saath multiplayer sync bhi karta hai
/// </summary>
public class WormMovement : MonoBehaviourPun, IPunObservable
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;               // Normal speed
    public float boostSpeed = 9f;              // Speed boost speed
    public float rotationSpeed = 180f;         // Kitni tezi se turn kare (degrees/sec)
    public float boostShrinkRate = 0.5f;       // Boost ke time body shrink hogi

    [Header("References")]
    public JoystickController joystick;        // Joystick assign karo
    public WormBody wormBody;                  // WormBody component
    public WormSkinManager skinManager;        // Skin manager

    [Header("Boost Button")]
    public bool isBoosting = false;            // Speed boost active hai?

    // Network sync ke liye
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float networkSpeed;

    // Private variables
    private float currentSpeed;
    private Rigidbody2D rb;
    private bool isAlive = true;

    public bool IsAlive => isAlive;
    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = moveSpeed;
    }

    private void Start()
    {
        // Sirf apna worm control karo (multiplayer mein)
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            // Joystick dhundho scene mein
            if (joystick == null)
                joystick = FindObjectOfType<JoystickController>();
        }
    }

    private void Update()
    {
        if (!isAlive) return;

        // Sirf apna worm move karo
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            HandleMovement();
            HandleBoost();
        }
        else
        {
            // Doosre players ke liye smooth interpolation
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
        }
    }

    private void HandleMovement()
    {
        Vector2 direction = Vector2.zero;

        // Joystick input
        if (joystick != null && joystick.IsPressed)
        {
            direction = joystick.Direction;
        }
        else
        {
            // Keyboard fallback (PC testing ke liye)
            direction.x = Input.GetAxis("Horizontal");
            direction.y = Input.GetAxis("Vertical");
        }

        // Agar koi input hai toh rotate karo
        if (direction.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle - 90f);

            // Smooth rotation
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Forward move karo (worm hamesha aage badhta hai)
        rb.linearVelocity = transform.up * currentSpeed;
    }

    private void HandleBoost()
    {
        // Boost active hai aur body shrink kar sakti hai
        if (isBoosting && wormBody != null && wormBody.SegmentCount > 5)
        {
            currentSpeed = boostSpeed;

            // Boost se body thodi shrink hogi
            wormBody.ShrinkBody(boostShrinkRate * Time.deltaTime);
        }
        else
        {
            currentSpeed = moveSpeed;
            isBoosting = false;
        }
    }

    /// <summary>
    /// Speed boost button se call karo
    /// </summary>
    public void StartBoost()
    {
        if (wormBody != null && wormBody.SegmentCount > 5)
            isBoosting = true;
    }

    /// <summary>
    /// Boost button release
    /// </summary>
    public void StopBoost()
    {
        isBoosting = false;
    }

    /// <summary>
    /// Worm mar gaya
    /// </summary>
    public void Die()
    {
        isAlive = false;
        rb.linearVelocity = Vector2.zero;

        // Body ke baaki segments food mein convert karo
        if (wormBody != null)
            wormBody.ConvertToFood();

        // Game Over UI show karo
        GameManager.Instance?.OnPlayerDied();

        // Photon se destroy karo (0.5 sec delay se)
        if (photonView.IsMine)
            Invoke(nameof(DestroyWorm), 0.5f);
    }

    private void DestroyWorm()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Destroy(gameObject);
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Speed permanently increase karo (power-up ke liye)
    /// </summary>
    public void ApplySpeedBoost(float amount, float duration)
    {
        moveSpeed += amount;
        Invoke(nameof(ResetSpeed), duration);
    }

    private void ResetSpeed()
    {
        moveSpeed = 5f; // Default speed pe wapas
    }

    // ====== Photon Network Sync ======
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Apna data send karo
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(currentSpeed);
            stream.SendNext(isBoosting);
        }
        else
        {
            // Doosron ka data receive karo
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkSpeed = (float)stream.ReceiveNext();
            isBoosting = (bool)stream.ReceiveNext();
        }
    }
}
