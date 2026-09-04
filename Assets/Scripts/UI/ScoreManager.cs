using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Distance score, star counters and the difficulty ramp.
/// Score is the distance actually travelled, so flying faster scores faster
/// and the "m" unit on screen is true. Speed rises from one formula instead
/// of a ladder of hand-written ranges, so it never stops climbing.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI highscoreText;
    [SerializeField]
    private TextMeshProUGUI starCounterText;
    [SerializeField]
    private TextMeshProUGUI starTotalCounterText;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private TextMeshProUGUI speedText;

    [Header("Difficulty")]
    [SerializeField]
    private CameraMovement cameraMovement;
    [Tooltip("Speed at level 1, in world units per second.")]
    [SerializeField]
    private float baseSpeed = 2.5f;
    [Tooltip("Fraction of the base speed added by each level.")]
    [SerializeField]
    private float speedStepPerLevel = 0.2f;
    [Tooltip("Distance in metres needed to reach the next level.")]
    [SerializeField]
    private float metresPerLevel = 250f;
    [Tooltip("Highest speed multiplier, so the game stays playable.")]
    [SerializeField]
    private float maxSpeedMultiplier = 4f;

    /// <summary>Raised when the run reaches a new level.</summary>
    public static event Action<int> LevelChanged;

    private const string HighscoreKey = "highscore_metres";
    private const string TotalStarCounterKey = "totalStarCounter";

    private float score;
    private float highscore;
    private int totalStarCounter;
    private int starCounter;
    private int currentLevel = -1;
    private bool running = true;

    public int Level => currentLevel;
    public float Score => score;
    public int StarsThisRun => starCounter;

    private void Start()
    {
        highscore = PlayerPrefs.GetFloat(HighscoreKey, 0f);
        totalStarCounter = PlayerPrefs.GetInt(TotalStarCounterKey, 0);

        UpdateHighscoreText();
        UpdateStarTexts();
        ApplyLevel(1);
    }

    private void Update()
    {
        if (!running)
        {
            return;
        }

        float speed = cameraMovement != null ? cameraMovement.CameraSpeed : baseSpeed;
        score += speed * Time.deltaTime;

        if (scoreText != null)
        {
            scoreText.text = FormatScore(score);
        }

        if (score > highscore)
        {
            highscore = score;
            UpdateHighscoreText();
        }

        ApplyLevel(Mathf.FloorToInt(score / metresPerLevel) + 1);
    }

    private void ApplyLevel(int level)
    {
        if (level == currentLevel)
        {
            return;
        }

        currentLevel = level;

        float multiplier = Mathf.Min(1f + speedStepPerLevel * (level - 1), maxSpeedMultiplier);

        if (cameraMovement != null)
        {
            cameraMovement.CameraSpeed = baseSpeed * multiplier;
        }

        if (levelText != null)
        {
            levelText.text = "Level " + level;
        }

        if (speedText != null)
        {
            speedText.text = "Speed x" + multiplier.ToString("0.0");
        }

        LevelChanged?.Invoke(level);
    }

    /// <summary>Stops the score climbing, called on game over.</summary>
    public void StopScoring()
    {
        running = false;
        SaveProgress();
    }

    public void AddStar()
    {
        starCounter++;
        totalStarCounter++;
        UpdateStarTexts();
    }

    /// <summary>Writes the record and star total to disk in one go.</summary>
    public void SaveProgress()
    {
        PlayerPrefs.SetFloat(HighscoreKey, highscore);
        PlayerPrefs.SetInt(TotalStarCounterKey, totalStarCounter);
        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveProgress();
        }
    }

    private void UpdateHighscoreText()
    {
        if (highscoreText != null)
        {
            highscoreText.text = "Highscore: " + FormatScore(highscore);
        }
    }

    private void UpdateStarTexts()
    {
        if (starCounterText != null)
        {
            starCounterText.text = "x " + starCounter;
        }

        if (starTotalCounterText != null)
        {
            starTotalCounterText.text = "Total: " + totalStarCounter;
        }
    }

    private static string FormatScore(float value)
    {
        return Mathf.FloorToInt(value) + "m";
    }
}
