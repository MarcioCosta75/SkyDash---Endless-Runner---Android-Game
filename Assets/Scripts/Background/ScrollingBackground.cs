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
    [Tooltip("How much of the camera's motion the background copies. 0 keeps it "
             + "still, 1 pins it to the camera. Small values read as distance.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float parallaxFactor = 0.22f;
    [Tooltip("How far below the camera the artwork should reach at startup.")]
    [SerializeField]
    private float coverBelowCamera = 25f;

    private Camera mainCamera;
    private Transform[] strips;
    private float loopHeight;
    private float lastCameraY;
    private bool hasLastCameraY;

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
            return;
        }

        AlignToCamera();
    }

    /// <summary>
    /// Slides the whole set down so the artwork already covers the camera on
    /// the first frame. The planets were authored starting 16 units above the
    /// opening view, which left the sky empty for the first few seconds.
    /// </summary>
    private void AlignToCamera()
    {
        if (mainCamera == null)
        {
            return;
        }

        Bounds painted = default;
        bool any = false;

        for (int i = 0; i < strips.Length; i++)
        {
            Renderer[] renderers = strips[i].GetComponentsInChildren<Renderer>();
            for (int r = 0; r < renderers.Length; r++)
            {
                if (!any)
                {
                    painted = renderers[r].bounds;
                    any = true;
                }
                else
                {
                    painted.Encapsulate(renderers[r].bounds);
                }
            }
        }

        if (!any)
        {
            return;
        }

        float cameraBottom = mainCamera.transform.position.y - mainCamera.orthographicSize;
        float wantedLowest = cameraBottom - coverBelowCamera;

        if (painted.min.y > wantedLowest)
        {
            float shift = painted.min.y - wantedLowest;
            transform.position -= new Vector3(0f, shift, 0f);
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

        float cameraY = mainCamera.transform.position.y;

        // Drifting up with a fraction of the camera makes the planets pass
        // more slowly than the obstacles, which reads as depth.
        if (hasLastCameraY && parallaxFactor > 0f)
        {
            float drift = (cameraY - lastCameraY) * parallaxFactor;
            if (drift != 0f)
            {
                transform.position += new Vector3(0f, drift, 0f);
            }
        }

        lastCameraY = cameraY;
        hasLastCameraY = true;

        float cameraBottom = cameraY - mainCamera.orthographicSize;
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
