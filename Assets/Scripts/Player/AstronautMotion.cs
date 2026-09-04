using System.Collections;
using UnityEngine;

/// <summary>
/// The astronaut's reactions, driven from code rather than from clips.
///
/// The only art is a nine frame flying loop, so new hand drawn animations are
/// not on the table. What is on the table is squash, stretch, spin and scale,
/// which is where most of the life in a 2D character comes from anyway:
///
///   picking up  a quick bounce, so a pickup is felt in the body
///   taking a hit  squashed sideways and knocked back, then settling
///   dying  a slow tumble and shrink
///   idling  a slight breathing scale, under everything else
///
/// It multiplies the resting scale rather than setting it, so the facing flip
/// in PlayerController keeps working underneath.
/// </summary>
public class AstronautMotion : MonoBehaviour
{
    [Header("Breathing")]
    [Tooltip("How much the body swells while flying. 0 turns it off.")]
    [SerializeField]
    private float breathAmount = 0.03f;
    [Tooltip("Seconds for one breath.")]
    [SerializeField]
    private float breathPeriod = 1.6f;

    [Header("Pickup bounce")]
    [SerializeField]
    private float bounceScale = 1.22f;
    [SerializeField]
    private float bounceSeconds = 0.22f;

    [Header("Hit")]
    [Tooltip("How far it squashes sideways when hit.")]
    [SerializeField]
    private float hitSquash = 0.3f;
    [SerializeField]
    private float hitSeconds = 0.3f;

    [Header("Death")]
    [SerializeField]
    private float deathSpin = 540f;
    [SerializeField]
    private float deathSeconds = 1.1f;

    private Health health;
    private Vector3 restScale;
    private float breathPhase;

    // One-off reactions multiply into these, so they layer over the breathing
    // without any of them fighting for the transform.
    private Vector3 reactionScale = Vector3.one;
    private float reactionSpin;
    private bool dying;

    private Coroutine reaction;

    private void Awake()
    {
        restScale = transform.localScale;
        breathPhase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void OnEnable()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.Hurt += OnHurt;
            health.Healed += OnPickup;
            health.Died += OnDied;
        }

        PlayerController.MagnetActivated += OnMagnet;
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Hurt -= OnHurt;
            health.Healed -= OnPickup;
            health.Died -= OnDied;
        }

        PlayerController.MagnetActivated -= OnMagnet;
    }

    private void LateUpdate()
    {
        // After the animator and after the movement, so nothing overwrites it.
        //
        // The facing sign has to be read fresh every frame, not taken from the
        // resting scale: PlayerController mirrors localScale.x to turn the
        // astronaut around, in Update, and writing an absolute scale here
        // would undo that turn before it was ever drawn.
        float facing = Mathf.Sign(transform.localScale.x);
        if (facing == 0f)
        {
            facing = 1f;
        }

        float width = Mathf.Abs(restScale.x);
        float height = restScale.y;

        if (breathAmount > 0f && breathPeriod > 0f && !dying)
        {
            float breath = Mathf.Sin(breathPhase + Time.time * (Mathf.PI * 2f / breathPeriod));
            height *= 1f + breath * breathAmount;
            width *= 1f - breath * breathAmount * 0.5f;
        }

        transform.localScale = new Vector3(width * reactionScale.x * facing,
                                           height * reactionScale.y,
                                           restScale.z);

        if (reactionSpin != 0f)
        {
            Vector3 euler = transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(euler.x, euler.y, reactionSpin);
        }
    }

    private void OnPickup()
    {
        Play(Bounce());
    }

    private void OnMagnet(float seconds)
    {
        Play(Bounce());
    }

    private void OnHurt(float graceSeconds)
    {
        if (graceSeconds > 0f)
        {
            Play(Squash());
        }
    }

    private void OnDied()
    {
        dying = true;
        Play(Tumble());
    }

    private void Play(IEnumerator routine)
    {
        if (dying && reaction != null)
        {
            // Nothing interrupts the death tumble.
            return;
        }

        if (reaction != null)
        {
            StopCoroutine(reaction);
        }

        reaction = StartCoroutine(routine);
    }

    /// <summary>A quick swell and settle, for anything collected.</summary>
    private IEnumerator Bounce()
    {
        float elapsed = 0f;
        while (elapsed < bounceSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bounceSeconds);
            float amount = Mathf.Sin(t * Mathf.PI);
            float grow = Mathf.Lerp(1f, bounceScale, amount);
            reactionScale = new Vector3(grow, grow, 1f);
            yield return null;
        }

        reactionScale = Vector3.one;
        reaction = null;
    }

    /// <summary>Squashed wide and short, then springing back.</summary>
    private IEnumerator Squash()
    {
        float elapsed = 0f;
        while (elapsed < hitSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / hitSeconds);

            // Overshoots on the way back, which is what makes it read as
            // springy rather than as a fade.
            float amount = Mathf.Sin(t * Mathf.PI) * (1f - t * 0.4f);
            reactionScale = new Vector3(1f + hitSquash * amount,
                                        1f - hitSquash * amount * 0.8f,
                                        1f);
            yield return null;
        }

        reactionScale = Vector3.one;
        reaction = null;
    }

    /// <summary>A slow tumble and shrink, so the end has a beat of its own.</summary>
    private IEnumerator Tumble()
    {
        float elapsed = 0f;
        while (elapsed < deathSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / deathSeconds);

            reactionSpin = deathSpin * t;

            float shrink = Mathf.Lerp(1f, 0.35f, t * t);
            reactionScale = new Vector3(shrink, shrink, 1f);
            yield return null;
        }

        reaction = null;
    }
}
