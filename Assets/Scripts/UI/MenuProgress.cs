using TMPro;
using UnityEngine;

/// <summary>
/// Shows the player's record and star total on the menu.
///
/// The menu had a title and two buttons, so nothing on it acknowledged that
/// the player had ever played before. The record is the whole point of an
/// endless runner, and the star total was being saved to disk and never shown
/// anywhere except mid-run.
///
/// It builds its own label, so the menu scene needs nothing added by hand.
/// </summary>
public class MenuProgress : MonoBehaviour
{
    private const string HighscoreKey = "highscore_metres";
    private const string TotalStarCounterKey = "totalStarCounter";

    [Tooltip("Optional. Left empty, a label is created and placed automatically.")]
    [SerializeField]
    private TextMeshProUGUI label;

    [Tooltip("Height on the screen, 0 at the bottom and 1 at the top.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float screenHeight = 0.245f;

    private void Start()
    {
        if (label == null)
        {
            label = Build();
        }

        label.text = Describe();
    }

    private string Describe()
    {
        float best = PlayerPrefs.GetFloat(HighscoreKey, 0f);
        int stars = PlayerPrefs.GetInt(TotalStarCounterKey, 0);

        // Nothing to boast about yet, so say nothing rather than showing zeroes.
        if (best <= 0f && stars <= 0)
        {
            return string.Empty;
        }

        string bestPart = "Best  " + Mathf.FloorToInt(best) + "m";
        return stars > 0 ? bestPart + "      Stars  " + stars : bestPart;
    }

    private TextMeshProUGUI Build()
    {
        GameObject root = new GameObject("ProgressCanvas");
        root.transform.SetParent(transform, false);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        UnityEngine.UI.CanvasScaler scaler = root.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0f;

        GameObject textObject = new GameObject("Progress");
        textObject.transform.SetParent(root.transform, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = 46f;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        text.color = new Color(1f, 1f, 1f, 0.75f);

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, screenHeight);
        rect.anchorMax = new Vector2(0.5f, screenHeight);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1000f, 70f);
        rect.anchoredPosition = Vector2.zero;

        return text;
    }
}
