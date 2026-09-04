using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Fades to black between scenes.
///
/// Every scene change was a hard cut: pressing Play, dying and restarting, or
/// going back to the menu all snapped from one screen to the next. A short
/// fade reads as one game rather than three separate screens, and it hides
/// the frame or two a scene load costs.
///
/// It builds its own canvas above everything and survives scene loads, so
/// nothing has to be placed in any scene.
/// </summary>
public class SceneFader : MonoBehaviour
{
    private const float FadeOutSeconds = 0.22f;
    private const float FadeInSeconds = 0.3f;

    private static SceneFader instance;
    private static bool loading;

    private Image curtain;

    /// <summary>Fades out, loads the scene by build index, then fades back in.</summary>
    public static void LoadScene(int buildIndex)
    {
        Load(() => SceneManager.LoadScene(buildIndex));
    }

    /// <summary>Fades out, loads the scene by name, then fades back in.</summary>
    public static void LoadScene(string sceneName)
    {
        Load(() => SceneManager.LoadScene(sceneName));
    }

    private static void Load(System.Action load)
    {
        // A second request during a fade would load twice.
        if (loading)
        {
            return;
        }

        SceneFader fader = Ensure();
        if (fader == null)
        {
            load();
            return;
        }

        loading = true;
        fader.StartCoroutine(fader.Run(load));
    }

    private IEnumerator Run(System.Action load)
    {
        yield return Fade(0f, 1f, FadeOutSeconds);

        load();

        // One frame for the new scene to wake up before it is revealed.
        yield return null;

        loading = false;
        yield return Fade(1f, 0f, FadeInSeconds);
    }

    private IEnumerator Fade(float from, float to, float seconds)
    {
        // Unscaled: the pause menu sets timeScale to zero, and its buttons
        // change scene.
        float elapsed = 0f;
        SetAlpha(from);

        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / seconds)));
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        if (curtain == null)
        {
            return;
        }

        curtain.color = new Color(0f, 0f, 0f, alpha);

        // Only swallow touches while it is actually covering something.
        curtain.raycastTarget = alpha > 0.01f;
    }

    private static SceneFader Ensure()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject host = new GameObject("SceneFader");
        DontDestroyOnLoad(host);

        instance = host.AddComponent<SceneFader>();
        instance.Build();
        return instance;
    }

    private void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Above the HUD and above the damage flash, which uses 100.
        canvas.sortingOrder = 200;

        GameObject imageObject = new GameObject("Curtain");
        imageObject.transform.SetParent(transform, false);

        curtain = imageObject.AddComponent<Image>();
        curtain.color = new Color(0f, 0f, 0f, 0f);
        curtain.raycastTarget = false;

        RectTransform rect = curtain.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            loading = false;
        }
    }
}
