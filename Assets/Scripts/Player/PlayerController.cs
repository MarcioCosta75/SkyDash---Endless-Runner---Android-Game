using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Steers the ship sideways and owns the magnet power-up timer.
///
/// There are two ways to steer, and both are always available:
/// dragging a finger anywhere on the play area moves the ship by the same
/// distance the finger travelled, and holding the left or right button moves
/// it at a steady speed. Holding matters: Unity's Button only fires on
/// release, so the old one-tap-one-step scheme meant tapping repeatedly to
/// cross the screen.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private GameObject characterObject;
    [Tooltip("Speed while a movement button is held, in world units per second.")]
    [SerializeField]
    private float buttonMoveSpeed = 7f;
    [Tooltip("How far the ship travels per unit of finger travel. 1 is one to one.")]
    [SerializeField]
    private float dragSensitivity = 1.25f;
    [Tooltip("Safety limit on drag speed, in world units per second.")]
    [SerializeField]
    private float maxDragSpeed = 24f;

    [Header("Feel")]
    [Tooltip("Degrees the ship tilts into a turn. 0 turns tilting off.")]
    [SerializeField]
    private float bankAngle = 10f;
    [Tooltip("How quickly the tilt follows the movement.")]
    [SerializeField]
    private float bankResponse = 10f;

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
    private HoldButton holdLeft;
    private HoldButton holdRight;

    private float sensitivity = 1f;
    private float magnetTimer;
    private float bank;

    private bool dragging;
    private int dragFingerId = -1;
    private float lastDragScreenX;

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

        // Set in the settings scene, stored on the device.
        sensitivity = GameSettings.TouchSensitivity;

        holdLeft = EnsureHoldButton(buttonLeft);
        holdRight = EnsureHoldButton(buttonRight);
    }

    /// <summary>
    /// Adds the hold reporter to a button if it does not have one, so the
    /// scene does not need to be wired up by hand for this to work.
    /// </summary>
    private static HoldButton EnsureHoldButton(Button button)
    {
        if (button == null)
        {
            return null;
        }

        HoldButton hold = button.GetComponent<HoldButton>();
        return hold != null ? hold : button.gameObject.AddComponent<HoldButton>();
    }

    private void Update()
    {
        if (characterTransform != null)
        {
            float move = ReadButtonMovement() + ReadDragMovement();
            if (move != 0f)
            {
                Move(move);
            }

            ClampToScreen();
            UpdateBank(move);
        }

        if (magnetTimer > 0f)
        {
            magnetTimer -= Time.deltaTime;
        }
    }

    /// <summary>Distance to move this frame from the on-screen buttons.</summary>
    private float ReadButtonMovement()
    {
        float direction = 0f;

        if (holdLeft != null && holdLeft.IsHeld)
        {
            direction -= 1f;
        }

        if (holdRight != null && holdRight.IsHeld)
        {
            direction += 1f;
        }

        // Keyboard, so the game can be played in the editor.
        direction += Input.GetAxisRaw("Horizontal");
        direction = Mathf.Clamp(direction, -1f, 1f);

        if (direction != 0f)
        {
            FaceDirection(direction);
        }

        return direction * buttonMoveSpeed * sensitivity * Time.deltaTime;
    }

    /// <summary>Distance to move this frame from a finger drag.</summary>
    private float ReadDragMovement()
    {
        float screenX;
        if (!TryGetDragPoint(out screenX))
        {
            dragging = false;
            dragFingerId = -1;
            return 0f;
        }

        if (!dragging)
        {
            // First frame of a drag only records where the finger started.
            dragging = true;
            lastDragScreenX = screenX;
            return 0f;
        }

        float deltaPixels = screenX - lastDragScreenX;
        lastDragScreenX = screenX;

        if (Mathf.Approximately(deltaPixels, 0f))
        {
            return 0f;
        }

        float worldPerPixel = ViewWidth() / Mathf.Max(1, Screen.width);
        float distance = deltaPixels * worldPerPixel * dragSensitivity * sensitivity;

        // Cap it so a flick cannot jump the ship straight through an obstacle.
        float limit = maxDragSpeed * Time.deltaTime;
        distance = Mathf.Clamp(distance, -limit, limit);

        FaceDirection(distance);
        return distance;
    }

    /// <summary>
    /// The horizontal screen position of the active drag, if there is one.
    /// Touches that started on the UI are ignored so the buttons keep working.
    /// </summary>
    private bool TryGetDragPoint(out float screenX)
    {
        screenX = 0f;

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (dragFingerId == touch.fingerId)
                {
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        return false;
                    }

                    screenX = touch.position.x;
                    return true;
                }

                if (dragFingerId == -1
                    && touch.phase == TouchPhase.Began
                    && !IsOverUI(touch.position, touch.fingerId))
                {
                    dragFingerId = touch.fingerId;
                    screenX = touch.position.x;
                    return true;
                }
            }

            return false;
        }

        // Mouse, for testing in the editor.
        if (Input.GetMouseButton(0))
        {
            if (!dragging && IsOverUI(Input.mousePosition, -1))
            {
                return false;
            }

            screenX = Input.mousePosition.x;
            return true;
        }

        return false;
    }

    private static bool IsOverUI(Vector2 screenPosition, int pointerId)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData data = new PointerEventData(EventSystem.current)
        {
            position = screenPosition,
            pointerId = pointerId,
        };

        System.Collections.Generic.List<RaycastResult> hits = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(data, hits);
        return hits.Count > 0;
    }

    private float ViewWidth()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        return mainCamera != null ? mainCamera.orthographicSize * 2f * mainCamera.aspect : 5.62f;
    }

    private void Move(float distance)
    {
        characterTransform.Translate(Vector2.right * distance, Space.World);
    }

    /// <summary>
    /// Leans the ship into a turn and levels it out again. Reads as weight,
    /// and shows the player that the input registered.
    /// </summary>
    private void UpdateBank(float distanceThisFrame)
    {
        if (bankAngle <= 0f)
        {
            return;
        }

        float speed = Time.deltaTime > 0f ? distanceThisFrame / Time.deltaTime : 0f;
        float target = -Mathf.Clamp(speed / Mathf.Max(0.01f, buttonMoveSpeed), -1f, 1f) * bankAngle;

        bank = Mathf.Lerp(bank, target, 1f - Mathf.Exp(-bankResponse * Time.deltaTime));

        // The sprite is mirrored when facing left, which would flip the tilt
        // with it, so the sign follows the facing.
        float facing = Mathf.Sign(characterTransform.localScale.x);
        characterTransform.localRotation = Quaternion.Euler(0f, 0f, bank * facing);
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
        float clamped = Mathf.Clamp(position.x, leftBound, rightBound);
        if (!Mathf.Approximately(clamped, position.x))
        {
            position.x = clamped;
            characterTransform.position = position;
        }
    }

    private void FaceDirection(float signedAmount)
    {
        if (signedAmount == 0f)
        {
            return;
        }

        Vector3 scale = characterTransform.localScale;
        float wanted = Mathf.Abs(scale.x) * Mathf.Sign(signedAmount);
        if (!Mathf.Approximately(scale.x, wanted))
        {
            scale.x = wanted;
            characterTransform.localScale = scale;
        }
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
