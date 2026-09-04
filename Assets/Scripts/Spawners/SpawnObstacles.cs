using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Places obstacles in rows above the screen, always leaving one gap the ship
/// can fit through.
///
/// The old version scattered each obstacle by up to 3.4 units vertically. That
/// broke the order they arrived in: two obstacles spawned a second apart could
/// reach the player at the same moment, side by side, with no way through.
/// Rows now arrive in the order they are made, and the gap is reserved before
/// any obstacle is placed, so a run is always survivable.
/// </summary>
public class SpawnObstacles : MonoBehaviour
{
    [SerializeField]
    private GameObject[] obstacles;

    [Header("Row placement")]
    [Tooltip("Left edge of the spawn area, relative to this object.")]
    [SerializeField]
    private float minX = -2.4f;
    [Tooltip("Right edge of the spawn area, relative to this object.")]
    [SerializeField]
    private float maxX = 2.4f;
    [Tooltip("Small vertical scatter inside a row, so rows do not look like a grid.")]
    [SerializeField]
    private float verticalJitter = 0.25f;

    [Header("Timing")]
    [Tooltip("Seconds between rows at level 1.")]
    [SerializeField]
    private float minTimeBetweenSpawn = 0.7f;
    [SerializeField]
    private float maxTimeBetweenSpawn = 1f;
    [Tooltip("Rows come this much closer together at full difficulty. 0.6 means 40 percent quicker.")]
    [SerializeField]
    private float intervalAtMaxDifficulty = 0.62f;

    [Header("Fairness")]
    [Tooltip("Width of the guaranteed gap, in world units. The ship is about 0.8 wide.")]
    [SerializeField]
    private float gapWidth = 1.8f;
    [Tooltip("Space each obstacle needs, in world units.")]
    [SerializeField]
    private float obstacleWidth = 1f;
    [Tooltip("How far the gap may move between rows, in world units.")]
    [SerializeField]
    private float maxGapShift = 2.6f;

    [Header("Difficulty")]
    [Tooltip("Obstacles in a row at level 1.")]
    [SerializeField]
    private int minObstaclesPerRow = 1;
    [Tooltip("Obstacles in a row once the game is at full difficulty. Two is the most that fits beside a fair gap.")]
    [SerializeField]
    private int maxObstaclesPerRow = 2;
    [Tooltip("Level at which rows reach the maximum count.")]
    [SerializeField]
    private int levelAtMaxDensity = 8;

    [Header("Look")]
    [Tooltip("Obstacles spawn at a random angle. Turn off for upright sprites.")]
    [SerializeField]
    private bool randomiseRotation = true;

    [Tooltip("Assumed ship speed, used to keep rows far enough apart to be fair.")]
    [SerializeField]
    private float shipSpeedEstimate = 7f;

    private ScoreManager scoreManager;
    private float spawnTime;
    private float gapCentre;
    private bool gapInitialised;
    private readonly List<float> takenX = new List<float>();

    private void Start()
    {
        gapCentre = Random.Range(minX + gapWidth * 0.5f, maxX - gapWidth * 0.5f);
        gapInitialised = true;
        SetNextSpawnTime();
    }

    private void Update()
    {
        if (Time.time >= spawnTime)
        {
            SpawnRow();
            SetNextSpawnTime();
        }
    }

    private void SetNextSpawnTime()
    {
        float low = Mathf.Min(minTimeBetweenSpawn, maxTimeBetweenSpawn);
        float high = Mathf.Max(minTimeBetweenSpawn, maxTimeBetweenSpawn);
        float scale = Mathf.Lerp(1f, intervalAtMaxDifficulty, DifficultyFraction());

        float interval = Random.Range(low, high) * scale;

        // Keep enough time between rows for the ship to cross the gap shift.
        float floorInterval = maxGapShift / Mathf.Max(1f, shipSpeedEstimate) / 0.8f;
        spawnTime = Time.time + Mathf.Max(floorInterval, interval);
    }

    /// <summary>0 at level 1, 1 once the game is at full difficulty.</summary>
    private float DifficultyFraction()
    {
        if (scoreManager == null)
        {
            scoreManager = FindAnyObjectByType<ScoreManager>();
        }

        int level = scoreManager != null ? Mathf.Max(1, scoreManager.Level) : 1;
        return levelAtMaxDensity > 1
            ? Mathf.Clamp01((level - 1f) / (levelAtMaxDensity - 1f))
            : 1f;
    }

    private void SpawnRow()
    {
        if (obstacles == null || obstacles.Length == 0)
        {
            return;
        }

        MoveGap();

        int count = ObstaclesThisRow();
        takenX.Clear();

        for (int i = 0; i < count; i++)
        {
            float x;
            if (!TryFindFreeX(out x))
            {
                break;
            }

            takenX.Add(x);
            SpawnOne(x);
        }
    }

    /// <summary>
    /// Nudges the gap, never further than the ship can travel between rows.
    /// </summary>
    private void MoveGap()
    {
        float low = minX + gapWidth * 0.5f;
        float high = maxX - gapWidth * 0.5f;

        if (high <= low)
        {
            gapCentre = (minX + maxX) * 0.5f;
            return;
        }

        if (!gapInitialised)
        {
            gapCentre = Random.Range(low, high);
            gapInitialised = true;
            return;
        }

        float target = Random.Range(low, high);
        gapCentre = Mathf.Clamp(target, gapCentre - maxGapShift, gapCentre + maxGapShift);
        gapCentre = Mathf.Clamp(gapCentre, low, high);
    }

    private int ObstaclesThisRow()
    {
        int high = Mathf.RoundToInt(Mathf.Lerp(minObstaclesPerRow, maxObstaclesPerRow, DifficultyFraction()));
        return Random.Range(minObstaclesPerRow, Mathf.Max(minObstaclesPerRow, high) + 1);
    }

    /// <summary>
    /// Picks a spot outside the gap that no obstacle in this row uses yet.
    /// </summary>
    private bool TryFindFreeX(out float x)
    {
        float gapLow = gapCentre - gapWidth * 0.5f;
        float gapHigh = gapCentre + gapWidth * 0.5f;

        // Candidate slots across the row, then filter by the gap and neighbours.
        for (int attempt = 0; attempt < 24; attempt++)
        {
            float candidate = Random.Range(minX, maxX);

            if (candidate > gapLow - obstacleWidth * 0.5f && candidate < gapHigh + obstacleWidth * 0.5f)
            {
                continue;
            }

            bool clashes = false;
            for (int i = 0; i < takenX.Count; i++)
            {
                if (Mathf.Abs(candidate - takenX[i]) < obstacleWidth)
                {
                    clashes = true;
                    break;
                }
            }

            if (!clashes)
            {
                x = candidate;
                return true;
            }
        }

        x = 0f;
        return false;
    }

    private void SpawnOne(float x)
    {
        GameObject prefab = obstacles[Random.Range(0, obstacles.Length)];
        if (prefab == null)
        {
            return;
        }

        Vector3 position = transform.position
                           + new Vector3(x, Random.Range(-verticalJitter, verticalJitter), 0f);

        Quaternion rotation = randomiseRotation
            ? Quaternion.Euler(0f, 0f, Random.Range(0f, 360f))
            : Quaternion.identity;

        Instantiate(prefab, position, rotation);
    }

    private void OnDrawGizmosSelected()
    {
        // Shows the spawn band and the current gap in the editor.
        Gizmos.color = Color.yellow;
        Vector3 centre = transform.position;
        Gizmos.DrawLine(centre + Vector3.right * minX, centre + Vector3.right * maxX);

        Gizmos.color = Color.green;
        Vector3 gap = centre + Vector3.right * gapCentre;
        Gizmos.DrawLine(gap + Vector3.left * (gapWidth * 0.5f), gap + Vector3.right * (gapWidth * 0.5f));
    }
}
