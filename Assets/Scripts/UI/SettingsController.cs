using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the settings screen: touch sensitivity, music volume and effects
/// volume. Values are saved on the device, and the game reads them when it
/// starts, so this scene does not need a reference to anything in the game.
/// </summary>
public class SettingsController : MonoBehaviour
{
    [Header("Touch")]
    [SerializeField]
    private Slider sensitivitySlider;
    [Tooltip("Optional. Shows the value as a multiplier, for example 1.2x.")]
    [SerializeField]
    private TMPro.TextMeshProUGUI valueLabel;

    [Header("Audio")]
    [SerializeField]
    private Slider musicSlider;
    [SerializeField]
    private Slider sfxSlider;

    private void Start()
    {
        SetUpSensitivity();
        SetUpVolume(musicSlider, GameSettings.MusicVolume, OnMusicChanged);
        SetUpVolume(sfxSlider, GameSettings.SfxVolume, OnSfxChanged);
    }

    private void OnDestroy()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveListener(OnSliderChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
        }

        // The setters deliberately skip the disk write, because they run on
        // every frame of a drag. Something has to flush them, or the settings
        // are lost when the app closes.
        GameSettings.Save();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            GameSettings.Save();
        }
    }

    private void SetUpSensitivity()
    {
        if (sensitivitySlider == null)
        {
            return;
        }

        sensitivitySlider.minValue = GameSettings.SliderMinimum;
        sensitivitySlider.maxValue = GameSettings.SliderMaximum;
        sensitivitySlider.wholeNumbers = true;
        sensitivitySlider.SetValueWithoutNotify(GameSettings.SliderValue);

        UpdateLabel(sensitivitySlider.value);
        sensitivitySlider.onValueChanged.AddListener(OnSliderChanged);
    }

    /// <summary>A volume slider runs 0 to 1 and starts at the saved value.</summary>
    private static void SetUpVolume(Slider slider, float current, UnityEngine.Events.UnityAction<float> handler)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.SetValueWithoutNotify(current);
        slider.onValueChanged.AddListener(handler);
    }

    private void OnSliderChanged(float value)
    {
        GameSettings.SliderValue = value;
        UpdateLabel(value);
    }

    private void OnMusicChanged(float value)
    {
        GameSettings.MusicVolume = value;

        // Applied straight away, so the player hears what they are setting.
        BackgroundMusic.RefreshVolume();
    }

    private void OnSfxChanged(float value)
    {
        GameSettings.SfxVolume = value;
    }

    private void UpdateLabel(float value)
    {
        if (valueLabel != null)
        {
            valueLabel.text = GameSettings.SliderToMultiplier(value).ToString("0.0") + "x";
        }
    }
}
