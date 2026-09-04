using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The reactions that tell the player what just happened: the camera kicks on
/// a hit, the screen flashes, the ship blinks while it cannot be hurt, and a
/// green pulse marks a heart pickup.
///
/// It finds what it needs and builds the flash overlay itself, so it works
/// from a single component with nothing to wire up.
/// </summary>
public class GameFeel : MonoBehaviour
{
    [Header("Camera kick")]
    [Tooltip("How far the camera jumps on a hit, in world units.")]
    [SerializeField]
    private float shakeStrength = 0.35f;
    [Tooltip("Seconds the kick takes to settle.")]
    [SerializeField]
    private float shakeDuration = 0.35f;

    [Header("Screen flash")]
    [SerializeField]
    private Color hurtColour = new Color(1f, 0.15f, 0.15f, 0.45f);
    [SerializeField]
    private Color healColour = new Color(0.3f, 1f, 0.4f, 0.3f);
    [Tooltip("Seconds the flash takes to fade out.")]
    [SerializeField]
    private float flashDuration = 0.4f;

    [Header("Ship blink")]
    [Tooltip("Seconds between blinks while invulnerable.")]
    [SerializeField]
    private float blinkInterval = 0.12f;

    private Health playerHealth;
    private SpriteRenderer playerSprite;
    private Transform cameraTransform;
    private Vector3 cameraRestPosition;
    private Image flashOverlay;

    private Coroutine shakeRoutine;
    private Coroutine flashRoutine;
    private Coroutine blinkRoutine;

    private void Start()
    {
        Camera main = Camera.main;
        if (main != null)
        {
            cameraTransform = main.transform;
            cameraRestPosition = cameraTransform.localPosition;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            playerSprite = player.GetComponent<SpriteRenderer>();
        }

        if (playerHealth != null)
        {
            playerHealth.Hurt += OnHurt;
            playerHealth.Healed += OnHealed;
        }

        flashOverlay = BuildFlashOverlay();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.Hurt -= OnHurt;
            playerHealth.Healed -= OnHealed;
        }
    }

    /// <summary>
    /// Creates a full-screen image on its own canvas, above the HUD, used for
    /// the damage and heal flashes.
    /// </summary>
    private Image BuildFlashOverlay()
    {
        GameObject root = new GameObject("FlashOverlay");
        root.transform.SetParent(transform, false);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // Above the HUD, which sits on the default order of 0.
        canvas.sortingOrder = 100;

        GameObject imageObject = new GameObject("Flash");
        imageObject.transform.SetParent(root.transform, false);

        Image image = imageObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = false;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return image;
    }

    private void OnHurt(float graceSeconds)
    {
        Restart(ref shakeRoutine, Shake());
        Restart(ref flashRoutine, Flash(hurtColour));

        if (playerSprite != null)
        {
            Restart(ref blinkRoutine, Blink(graceSeconds));
        }
    }

    private void OnHealed()
    {
        Restart(ref flashRoutine, Flash(healColour));
    }

    private void Restart(ref Coroutine slot, IEnumerator routine)
    {
        if (slot != null)
        {
            StopCoroutine(slot);
        }

        slot = StartCoroutine(routine);
    }

    private IEnumerator Shake()
    {
        if (cameraTransform == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            // Fades out, so the kick is strongest at the moment of impact.
            float falloff = 1f - Mathf.Clamp01(elapsed / shakeDuration);
            Vector2 offset = Random.insideUnitCircle * (shakeStrength * falloff);

            cameraTransform.localPosition = cameraRestPosition + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }

        cameraTransform.localPosition = cameraRestPosition;
        shakeRoutine = null;
    }

    private IEnumerator Flash(Color colour)
    {
        if (flashOverlay == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / flashDuration);
            flashOverlay.color = new Color(colour.r, colour.g, colour.b, colour.a * t);
            yield return null;
        }

        flashOverlay.color = new Color(colour.r, colour.g, colour.b, 0f);
        flashRoutine = null;
    }

    private IEnumerator Blink(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            playerSprite.enabled = !playerSprite.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        playerSprite.enabled = true;
        blinkRoutine = null;
    }
}
