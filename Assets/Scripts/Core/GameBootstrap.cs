using UnityEngine;

/// <summary>
/// Applies runtime settings that must be identical in every scene.
/// Runs automatically before the first scene loads, so it needs no
/// GameObject and cannot be forgotten when a new scene is added.
/// </summary>
public static class GameBootstrap
{
    public const int TargetFrameRate = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialise()
    {
        // Mobile defaults to 30 fps unless a target is set explicitly.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;

        // The whole game is authored for a 1080x1920 portrait canvas.
        Screen.orientation = ScreenOrientation.Portrait;
    }
}
