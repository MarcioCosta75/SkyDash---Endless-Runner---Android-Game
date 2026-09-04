using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Reapplies the Android player settings the game ships with.
/// The values are also stored in ProjectSettings.asset. This menu item exists
/// so they can be restored in one click if the editor ever writes over them,
/// and so the build configuration is readable in source control.
/// </summary>
public static class AndroidBuildSettings
{
    private const string CompanyName = "The Scaling Studio";
    private const string ProductName = "SkyDash";
    private const string BundleIdentifier = "com.thescalingstudio.skydash";
    private const int MinimumSdk = 25;

    [MenuItem("SkyDash/Apply Android Build Settings")]
    public static void Apply()
    {
        PlayerSettings.companyName = CompanyName;
        PlayerSettings.productName = ProductName;

        NamedBuildTarget android = NamedBuildTarget.Android;
        PlayerSettings.SetApplicationIdentifier(android, BundleIdentifier);

        // 64-bit is required by Google Play, and needs IL2CPP.
        PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

        PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MinimumSdk;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        // The game is drawn for a 1080x1920 portrait canvas only.
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        AssetDatabase.SaveAssets();
        Debug.Log("SkyDash: Android build settings applied.");
    }

    [MenuItem("SkyDash/Report Android Build Settings")]
    public static void Report()
    {
        NamedBuildTarget android = NamedBuildTarget.Android;
        Debug.Log(string.Format(
            "SkyDash Android settings\n" +
            "  company / product : {0} / {1}\n" +
            "  bundle id         : {2}\n" +
            "  scripting backend : {3}\n" +
            "  architectures     : {4}\n" +
            "  min / target SDK  : {5} / {6}\n" +
            "  orientation       : {7}",
            PlayerSettings.companyName,
            PlayerSettings.productName,
            PlayerSettings.GetApplicationIdentifier(android),
            PlayerSettings.GetScriptingBackend(android),
            PlayerSettings.Android.targetArchitectures,
            PlayerSettings.Android.minSdkVersion,
            PlayerSettings.Android.targetSdkVersion,
            PlayerSettings.defaultInterfaceOrientation));
    }
}
