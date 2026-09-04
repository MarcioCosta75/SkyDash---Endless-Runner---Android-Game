using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Moves the ship sideways between the screen edges and owns the magnet
/// power-up timer. The two on-screen buttons nudge a target position and the
/// ship slides towards it at a constant speed.
///
/// The sideways position is applied in LateUpdate and kept in a field rather
/// than read back from the transform. The Float animation on this object
/// writes the whole local position every frame, after Update, so anything
/// written earlier is overwritten and anything read back is the animation's
/// value rather than ours.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private GameObject characterObject;
    [Tooltip("Sideways speed in world units per second.")]
    [SerializeField]
    private float moveSpeed = 4f;
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
    private float currentX;
    private float targetX;
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

        currentX = characterTransform.position.x;
        targetX = currentX;

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
        if (magnetTimer > 0f)
        {
            magnetTimer -= Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if (characterTransform == null)
        {
            return;
        }

        currentX = Mathf.MoveTowards(currentX, targetX, moveSpeed * Time.deltaTime);
        ClampToScreen();

        // Only x is ours. y and z stay as the float animation left them.
        Vector3 position = characterTransform.position;
        position.x = currentX;
        characterTransform.position = position;
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

        currentX = Mathf.Clamp(currentX, leftBound, rightBound);
        targetX = Mathf.Clamp(targetX, leftBound, rightBound);
    }

    private void MoveLeft()
    {
        targetX = currentX - stepDistance;
        FaceDirection(-1f);
    }

    private void MoveRight()
    {
        targetX = currentX + stepDistance;
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
