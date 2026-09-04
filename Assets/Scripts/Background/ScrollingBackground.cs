using UnityEngine;

/// <summary>
/// Recycles the background strips so the sky never runs out.
/// Each direct child is one hand-placed strip of planets. When a strip drops
/// below the camera it jumps back above the topmost strip, which makes the
/// existing artwork loop forever without spawning anything new.
/// </summary>
public class ScrollingBackground : MonoBehaviour
{
    [Tooltip("Extra distance below the camera before a strip is recycled.")]
    [SerializeField]
    private float recycleMargin = 20f;

    private Camera mainCamera;
    private Transform[] strips;
    private float loopHeight;

    private void Start()
    {
        mainCamera = Camera.main;

        strips = new Transform[transform.childCount];
        for (int i = 0; i < strips.Length; i++)
        {
            strips[i] = transform.GetChild(i);
        }

        if (strips.Length < 2)
        {
            enabled = false;
            return;
        }

        // Derive the loop height from the strips themselves, so moving or
        // adding strips in the editor keeps the loop seamless.
        float lowest = float.MaxValue;
        float highest = float.MinValue;
        for (int i = 0; i < strips.Length; i++)
        {
            float y = strips[i].position.y;
            lowest = Mathf.Min(lowest, y);
            highest = Mathf.Max(highest, y);
        }

        float spacing = (highest - lowest) / (strips.Length - 1);
        loopHeight = spacing * strips.Length;

        // Strips stacked at the same height would give a zero loop, and the
        // recycle loop below would then never end.
        if (loopHeight <= Mathf.Epsilon)
        {
            Debug.LogWarning(
                "ScrollingBackground: the strips are not spread out vertically, " +
                "so the background cannot loop. Disabling.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        float cameraBottom = mainCamera.transform.position.y - mainCamera.orthographicSize;
        float recycleBelow = cameraBottom - recycleMargin;

        for (int i = 0; i < strips.Length; i++)
        {
            Transform strip = strips[i];
            if (strip == null)
            {
                continue;
            }

            // A strip can be several loops behind if the camera moved a long
            // way in one frame, so keep lifting it until it is back in view.
            while (strip.position.y < recycleBelow)
            {
                strip.position += new Vector3(0f, loopHeight, 0f);
            }
        }
    }
}
