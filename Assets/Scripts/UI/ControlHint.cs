using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Tells the player how to steer, once, at the start of a run.
///
/// The game used to show two arrow buttons, which said "you move sideways"
/// without a word. They were retired in favour of dragging, and nothing was
/// left to say so: a new player sees an astronaut and no controls at all.
///
/// The hint fades away as soon as the player moves, so anyone who already
/// knows never reads it twice.
/// </summary>
public class ControlHint : MonoBehaviour
{
    private const string Message = "Drag anywhere to move";
    private const string SeenKey = "seenControlHint";

    [Tooltip("Seconds it stays if the player does not move at all.")]
    [SerializeField]
    private float timeout = 6f;
    [Tooltip("Seconds the fade out takes.")]
    [SerializeField]
    private float fadeSeconds = 0.5f;
    [Tooltip("Show it every run, rather than only until it has been read once.")]
    [SerializeField]
    private bool alwaysShow;

    private TextMeshProUGUI label;

    private void Start()
    {
        // After a few runs the hint is just clutter, so remember it was read.
        if (!alwaysShow && PlayerPrefs.GetInt(SeenKey, 0) >= 3)
        {
            enabled = false;
            return;
        }

        label = Build();
        StartCoroutine(ShowUntilMoved());
    }

    private TextMeshProUGUI Build()
    {
        GameObject root = new GameObject("ControlHintCanvas");
        root.transform.SetParent(transform, false);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // Under the fader at 200 and the damage flash at 100.
        canvas.sortingOrder = 50;

        UnityEngine.UI.CanvasScaler scaler = root.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0f;

        GameObject textObject = new GameObject("Hint");
        textObject.transform.SetParent(root.transform, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = Message;
        text.fontSize = 52f;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        text.color = new Color(1f, 1f, 1f, 0.85f);

        // Low on the screen, above the fire button, clear of the play area.
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(900f, 90f);
        rect.anchoredPosition = new Vector2(0f, 420f);

        return text;
    }

    private IEnumerator ShowUntilMoved()
    {
        float startX = StartingX();
        float elapsed = 0f;

        // Wait for a real move, or give up after the timeout.
        while (elapsed < timeout)
        {
            elapsed += Time.deltaTime;

            PlayerController player = PlayerController.Instance;
            if (player != null && Mathf.Abs(player.Position.x - startX) > 0.3f)
            {
                PlayerPrefs.SetInt(SeenKey, PlayerPrefs.GetInt(SeenKey, 0) + 1);
                break;
            }

            yield return null;
        }

        yield return Fade();
        Destroy(label.canvas.gameObject);
    }

    private static float StartingX()
    {
        PlayerController player = PlayerController.Instance;
        return player != null ? player.Position.x : 0f;
    }

    private IEnumerator Fade()
    {
        Color start = label.color;
        float elapsed = 0f;

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / fadeSeconds);
            label.color = new Color(start.r, start.g, start.b, start.a * t);
            yield return null;
        }
    }
}
