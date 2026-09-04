using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Places obstacles in rows above the screen, always leaving one gap the
/// astronaut can fit through.
///
/// The gap is what sets the difficulty. It starts wide enough to be forgiving
/// and tightens as the run goes on, while more obstacles join each row, so a
/// late row demands a precise line instead of a rough flick. It never closes
/// below a passable width, and never moves further between rows than the
/// astronaut can travel in the time available.
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
    [Tooltip("Rows come this much closer together at full difficulty.")]
    [SerializeField]
    private float intervalAtMaxDifficulty = 0.58f;

    [Header("The gap")]
    [Tooltip("Gap width at level 1, in world units. The astronaut is about 0.8 wide.")]
    [SerializeField]
    private float gapWidthAtStart = 1.62f;
    [Tooltip("Gap width at full difficulty. Below about 1.05 it stops being passable.")]
    [SerializeField]
    private float gapWidthAtMax = 1.14f;
    [Tooltip("Space each obstacle needs beside its neighbour, in world units.")]
    [SerializeField]
    private float obstacleSpacing = 0.9f;
    [Tooltip("How far the gap may move between rows, in world units.")]
    [SerializeField]
    private float maxGapShift = 2.3f;

    [Header("Difficulty")]
    [Tooltip("Obstacles in a row at level 1.")]
    [SerializeField]
    private int minObstaclesPerRow = 1;
    [Tooltip("Obstacles in a row at full difficulty.")]
    [SerializeField]
    private int maxObstaclesPerRow = 3;
    [Tooltip("Level at which the gap and the row count reach their hardest.")]
    [SerializeField]
    private int levelAtMaxDensity = 6;

    [Header("Look")]
    [Tooltip("Obstacles spawn at a random angle. Turn off for upright sprites.")]
    [SerializeField]
    private bool randomiseRotation = true;

    [Tooltip("Assumed astronaut speed, used to keep rows far enough apart to be fair.")]
    [SerializeField]
    private float astronautSpeedEstimate = 7f;

    /// <summary>Narrowest gap the astronaut can still pass through.</summary>
    private const float MinimumPassableGap = 1.05f;

    private ScoreManager scoreManager;
    private float spawnTime;
    private float gapCentre;
    private float gapWidth;
    private readonly List<float> takenX = new List<float>();

    private void Start()
    {
        gapWidth = gapWidthAtStart;
        gapCentre = Random.Range(minX + gapWidth * 0.5f, maxX - gapWidth * 0.5f);
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

        // Keep enough time between rows for the astronaut to cross the gap shift.
        float floorInterval = maxGapShift / Mathf.Max(1f, astronautSpeedEstimate) / 0.8f;
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

        float difficulty = DifficultyFraction();

        gapWidth = Mathf.Max(MinimumPassableGap,
                             Mathf.Lerp(gapWidthAtStart, gapWidthAtMax, difficulty));

        MoveGap();

        takenX.Clear();
        PlaceRow(ObstaclesThisRow(difficulty));

        for (int i = 0; i < takenX.Count; i++)
        {
            SpawnOne(takenX[i]);
        }
    }

    /// <summary>
    /// Fills the space either side of the gap, spreading the obstacles evenly
    /// rather than dropping them at random. Random placement kept failing to
    /// find room for the last obstacle, so late rows never reached their
    /// intended density.
    /// </summary>
    private void PlaceRow(int wanted)
    {
        float clearance = obstacleSpacing * 0.5f;
        float leftEnd = gapCentre - gapWidth * 0.5f - clearance;
        float rightStart = gapCentre + gapWidth * 0.5f + clearance;

        float leftRoom = Mathf.Max(0f, leftEnd - minX);
        float rightRoom = Mathf.Max(0f, maxX - rightStart);

        int leftCapacity = Mathf.FloorToInt(leftRoom / obstacleSpacing);
        int rightCapacity = Mathf.FloorToInt(rightRoom / obstacleSpacing);
        int capacity = leftCapacity + rightCapacity;

        if (capacity <= 0)
        {
            return;
        }

        int count = Mathf.Min(wanted, capacity);

        // Split between the two sides in proportion to the room each has,
        // then hand any leftover to whichever side can still take it.
        int onLeft = capacity > 0 ? Mathf.RoundToInt(count * (leftCapacity / (float)capacity)) : 0;
        onLeft = Mathf.Clamp(onLeft, 0, leftCapacity);
        int onRight = count - onLeft;

        if (onRight > rightCapacity)
        {
            onLeft += onRight - rightCapacity;
            onRight = rightCapacity;
            onLeft = Mathf.Min(onLeft, leftCapacity);
        }

        AddEvenlySpaced(minX, leftEnd, onLeft);
        AddEvenlySpaced(rightStart, maxX, onRight);
    }

    /// <summary>Spreads count obstacles across a span, with a little scatter.</summary>
    private void AddEvenlySpaced(float from, float to, int count)
    {
        if (count <= 0 || to <= from)
        {
            return;
        }

        float step = (to - from) / count;
        float wobble = Mathf.Max(0f, (step - obstacleSpacing) * 0.5f);

        for (int i = 0; i < count; i++)
        {
            float centre = from + step * (i + 0.5f);
            takenX.Add(Mathf.Clamp(centre + Random.Range(-wobble, wobble), from, to));
        }
    }

    /// <summary>
    /// Nudges the gap, never further than the astronaut can travel between rows.
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

        float target = Random.Range(low, high);
        gapCentre = Mathf.Clamp(target, gapCentre - maxGapShift, gapCentre + maxGapShift);
        gapCentre = Mathf.Clamp(gapCentre, low, high);
    }

    private int ObstaclesThisRow(float difficulty)
    {
        int high = Mathf.RoundToInt(Mathf.Lerp(minObstaclesPerRow, maxObstaclesPerRow, difficulty));
        return Random.Range(minObstaclesPerRow, Mathf.Max(minObstaclesPerRow, high) + 1);
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
        float half = (gapWidth > 0f ? gapWidth : gapWidthAtStart) * 0.5f;
        Gizmos.DrawLine(gap + Vector3.left * half, gap + Vector3.right * half);
    }
}
