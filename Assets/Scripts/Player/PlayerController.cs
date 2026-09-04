using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Moves the ship sideways between the screen edges and owns the magnet
/// power-up timer. The two on-screen buttons nudge a target position and the
/// ship slides towards it.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private GameObject characterObject;
    [Tooltip("Speed factor. Sideways speed is this times movementDistance.")]
    [SerializeField]
    private float moveSpeed = 5f;
    [Tooltip("How far one button press moves the ship, in world units.")]
    [SerializeField]
    private float movementDistance = 0.8f;

    [Header("Controls")]
    [SerializeField]
    private Button buttonLeft;
    [SerializeField]
    private Button buttonRight;

    [Header("Magnet power-up")]
    [SerializeField]
    private float magnetDuration = 20f;
    [SerializeField]
    private AudioClip collisionSound;

    /// <summary>The active player, so pickups do not each have to search for it.</summary>
    public static PlayerController Instance { get; private set; }

    /// <summary>Raised when the magnet is picked up, carrying its full duration.</summary>
    public static event Action<float> MagnetActivated;

    private Camera mainCamera;
    private Transform characterTransform;
    private SpriteRenderer characterRenderer;
    private float targetPosition;
    private float stepDistance;
    private float magnetTimer;

    public bool IsMagnetActive => magnetTimer > 0f;
    public Vector3 Position => characterTransform != null ? characterTransform.position : transform.position;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;

        if (characterObject == null)
        {
            characterObject = gameObject;
        }

        characterTransform = characterObject.transform;
        characterRenderer = characterObject.GetComponent<SpriteRenderer>();
        targetPosition = characterTransform.position.x;

        // The settings scene stores this on the device.
        stepDistance = movementDistance * GameSettings.TouchSensitivity;

        if (buttonLeft != null)
        {
            buttonLeft.onClick.AddListener(MoveLeft);
        }

        if (buttonRight != null)
        {
            buttonRight.onClick.AddListener(MoveRight);
        }
    }

    private void Update()
    {
        if (characterTransform != null)
        {
            MoveTowardsTarget();
        }

        if (magnetTimer > 0f)
        {
            magnetTimer -= Time.deltaTime;
        }
    }

    private void MoveTowardsTarget()
    {
        float currentX = characterTransform.position.x;

        // Speed is movementDistance times moveSpeed, as the game was tuned.
        float step = movementDistance * moveSpeed * Time.deltaTime;
        float remaining = targetPosition - currentX;

        if (Mathf.Abs(remaining) > step)
        {
            characterTransform.Translate(Vector2.right * (Mathf.Sign(remaining) * step), Space.World);
        }
        else
        {
            characterTransform.Translate(Vector2.right * remaining, Space.World);
        }

        ClampToScreen();
    }

    private void ClampToScreen()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        float halfWidth = characterRenderer != null ? characterRenderer.bounds.extents.x : 0f;
        float leftBound = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, mainCamera.nearClipPlane)).x + halfWidth;
        float rightBound = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0f, mainCamera.nearClipPlane)).x - halfWidth;

        if (leftBound > rightBound)
        {
            return;
        }

        Vector3 position = characterTransform.position;
        position.x = Mathf.Clamp(position.x, leftBound, rightBound);
        characterTransform.position = position;

        targetPosition = Mathf.Clamp(targetPosition, leftBound, rightBound);
    }

    private void MoveLeft()
    {
        targetPosition = characterTransform.position.x - stepDistance;
        FaceDirection(-1f);
    }

    private void MoveRight()
    {
        targetPosition = characterTransform.position.x + stepDistance;
        FaceDirection(1f);
    }

    private void FaceDirection(float sign)
    {
        Vector3 scale = characterTransform.localScale;
        scale.x = Mathf.Abs(scale.x) * sign;
        characterTransform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // The player has three trigger colliders; the pickup destroys itself
        // on the first one, but the others can still fire this frame.
        if (collision == null || !collision.CompareTag("Magnet"))
        {
            return;
        }

        magnetTimer = magnetDuration;
        MagnetActivated?.Invoke(magnetDuration);

        Destroy(collision.gameObject);

        if (collisionSound != null)
        {
            AudioSource.PlayClipAtPoint(collisionSound, transform.position);
        }
    }
}
