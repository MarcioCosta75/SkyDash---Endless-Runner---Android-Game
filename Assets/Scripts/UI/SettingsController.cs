using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the touch sensitivity slider in the settings scene.
/// The value is saved on the device, and the player reads it when a run
/// starts, so this scene does not need a reference to the player.
/// </summary>
public class SettingsController : MonoBehaviour
{
    [SerializeField]
    private Slider sensitivitySlider;
    [Tooltip("Optional. Shows the value as a multiplier, for example 1.2x.")]
    [SerializeField]
    private TMPro.TextMeshProUGUI valueLabel;

    private void Start()
    {
        if (sensitivitySlider == null)
        {
            enabled = false;
            return;
        }

        sensitivitySlider.minValue = GameSettings.SliderMinimum;
        sensitivitySlider.maxValue = GameSettings.SliderMaximum;
        sensitivitySlider.wholeNumbers = true;
        sensitivitySlider.SetValueWithoutNotify(GameSettings.SliderValue);

        UpdateLabel(sensitivitySlider.value);
        sensitivitySlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDestroy()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveListener(OnSliderChanged);
        }
    }

    private void OnSliderChanged(float value)
    {
        GameSettings.SliderValue = value;
        UpdateLabel(value);
    }

    private void UpdateLabel(float value)
    {
        if (valueLabel != null)
        {
            valueLabel.text = GameSettings.SliderToMultiplier(value).ToString("0.0") + "x";
        }
    }
}
