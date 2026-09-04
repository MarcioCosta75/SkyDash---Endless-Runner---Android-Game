using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Moves the ship sideways between the screen edges and owns the magnet
/// power-up timer. The two on-screen buttons nudge a target position and the
/// ship slides towards it at a constant speed.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private GameObject characterObject;
    [Tooltip("Sideways speed in world units per second.")]
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
    private float magnetTimer;
    private float stepDistance;

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
        if (characterTransform == null)
        {
            return;
        }

        MoveTowardsTarget();
        TickMagnet();
    }

    private void MoveTowardsTarget()
    {
        Vector3 position = characterTransform.position;

        position.x = Mathf.MoveTowards(position.x, targetPosition, moveSpeed * Time.deltaTime);

        float halfWidth = characterRenderer != null ? characterRenderer.bounds.extents.x : 0f;
        if (mainCamera != null)
        {
            float leftBound = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, mainCamera.nearClipPlane)).x + halfWidth;
            float rightBound = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0f, mainCamera.nearClipPlane)).x - halfWidth;

            position.x = Mathf.Clamp(position.x, leftBound, rightBound);
            targetPosition = Mathf.Clamp(targetPosition, leftBound, rightBound);
        }

        characterTransform.position = position;
    }

    private void TickMagnet()
    {
        if (magnetTimer > 0f)
        {
            magnetTimer -= Time.deltaTime;
        }
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
        if (!collision.CompareTag("Magnet"))
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
