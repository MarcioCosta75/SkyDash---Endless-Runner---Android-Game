using System;
using System.Collections;
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
    [Header("Feel")]
    [Tooltip("How much the star counter grows when a star is collected.")]
    [SerializeField]
    private float pulseScale = 1.35f;
    [Tooltip("Seconds the pulse takes to settle.")]
    [SerializeField]
    private float pulseDuration = 0.18f;

    [Tooltip("Highest speed multiplier. 3 matches the fastest the game reached " +
             "before the level ladder was replaced. Raise it to keep the " +
             "difficulty climbing for longer.")]
    [SerializeField]
    private float maxSpeedMultiplier = 3f;

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
    private float bestBeforeRun;

    public int Level => currentLevel;

    private void Start()
    {
        highscore = PlayerPrefs.GetFloat(HighscoreKey, 0f);
        bestBeforeRun = highscore;
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

        if (level > 1)
        {
            Pulse(levelText);
        }
    }

    /// <summary>Stops the score climbing, called on game over.</summary>
    public void StopScoring()
    {
        running = false;
        SaveProgress();
        ShowRunSummary();
    }

    /// <summary>
    /// Replaces the highscore line with what this run achieved. The label sits
    /// on the game over panel, so this is the first thing read after dying.
    /// </summary>
    private void ShowRunSummary()
    {
        if (highscoreText == null)
        {
            return;
        }

        string firstLine = FormatScore(score) + "   Stars " + starCounter;
        string secondLine = score > bestBeforeRun
            ? "NEW BEST!"
            : "Best " + FormatScore(bestBeforeRun);

        highscoreText.text = firstLine + "\n" + secondLine;
    }

    public void AddStar()
    {
        AddStars(1);
    }

    /// <summary>Adds several stars at once, used for the alien kill bonus.</summary>
    public void AddStars(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        starCounter += amount;
        totalStarCounter += amount;
        UpdateStarTexts();
        Pulse(starCounterText);
    }

    /// <summary>Briefly grows a label, so a pickup is felt and not just read.</summary>
    private void Pulse(TextMeshProUGUI label)
    {
        if (label == null || !isActiveAndEnabled)
        {
            return;
        }

        StartCoroutine(PulseRoutine(label.transform));
    }

    private IEnumerator PulseRoutine(Transform target)
    {
        Vector3 rest = Vector3.one;
        float elapsed = 0f;

        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pulseDuration);

            // Out and back in one arc.
            float amount = Mathf.Sin(t * Mathf.PI);
            target.localScale = rest * Mathf.Lerp(1f, pulseScale, amount);
            yield return null;
        }

        target.localScale = rest;
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
