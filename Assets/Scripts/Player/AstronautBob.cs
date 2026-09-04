using UnityEngine;

/// <summary>
/// Floats the astronaut gently up and down.
///
/// The fly animation used to do this by keying the whole local position, which
/// also pinned x to zero. Unity applies the animator after Update, so that
/// clip overwrote the sideways movement every frame, and the only reason the
/// game worked was that Apply Root Motion happened to be ticked, which turns
/// the clip's net-zero loop into root motion and leaves x alone. Untick that
/// box and the astronaut pins itself off screen.
///
/// Driving the bob from here instead means the animation only swaps sprites,
/// nothing fights over the position, and the checkbox stops mattering.
/// </summary>
public class AstronautBob : MonoBehaviour
{
    [Tooltip("How far it drifts from its resting height, in world units.")]
    [SerializeField]
    private float amplitude = 0.055f;
    [Tooltip("Seconds for one full up and down.")]
    [SerializeField]
    private float period = 1f;

    private float restingY;
    private float phase;

    private void Start()
    {
        restingY = transform.localPosition.y;

        // A random start means a restarted run does not look identical.
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void LateUpdate()
    {
        if (period <= 0f)
        {
            return;
        }

        // LateUpdate, so this settles after the animator and after the
        // sideways movement, and neither can undo it.
        float offset = Mathf.Sin(phase + Time.time * (Mathf.PI * 2f / period)) * amplitude;

        Vector3 position = transform.localPosition;
        position.y = restingY + offset;
        transform.localPosition = position;
    }
}
