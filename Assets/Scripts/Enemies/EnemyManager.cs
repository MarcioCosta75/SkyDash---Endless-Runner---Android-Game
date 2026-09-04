using UnityEngine;

/// <summary>
/// Decides when the alien shows up.
///
/// It arrives on reaching a new level rather than on a bare timer, so it lands
/// with the level banner and reads as a milestone instead of an interruption.
/// Each visit is a wave, and the wave number makes the alien tougher, so the
/// fourth fight is not the first fight again.
///
/// While the alien is on screen the obstacle spawner is off, which makes the
/// fight its own phase rather than a second thing to dodge.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject alienEnemy;
    [Tooltip("Obstacle spawner that runs while no alien is on screen.")]
    [SerializeField]
    private GameObject spawnPoint;

    [Header("When it appears")]
    [Tooltip("First level that summons an alien.")]
    [SerializeField]
    private int firstBossLevel = 2;
    [Tooltip("Levels between one alien and the next.")]
    [SerializeField]
    private int levelsBetweenBosses = 3;

    private int wavesSeen;
    private bool alienActive;

    private void OnEnable()
    {
        ScoreManager.LevelChanged += OnLevelChanged;
    }

    private void OnDisable()
    {
        ScoreManager.LevelChanged -= OnLevelChanged;
    }

    private void Start()
    {
        SetSpawnPointActive(true);

        if (alienEnemy != null)
        {
            alienEnemy.SetActive(false);
        }
    }

    private void Update()
    {
        if (!alienActive || alienEnemy == null)
        {
            return;
        }

        // The alien switches itself off when it dies or retreats.
        if (!alienEnemy.activeInHierarchy)
        {
            alienActive = false;
            SetSpawnPointActive(true);
        }
    }

    private void OnLevelChanged(int level)
    {
        if (alienEnemy == null || alienActive || level < firstBossLevel)
        {
            return;
        }

        int step = Mathf.Max(1, levelsBetweenBosses);
        if ((level - firstBossLevel) % step != 0)
        {
            return;
        }

        Summon();
    }

    private void Summon()
    {
        wavesSeen++;

        EnemyHealth health = alienEnemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.ConfigureForWave(wavesSeen);
        }

        MissileSpawner spawner = alienEnemy.GetComponent<MissileSpawner>();
        if (spawner != null)
        {
            spawner.ConfigureForWave(wavesSeen);
        }

        alienActive = true;
        SetSpawnPointActive(false);
        alienEnemy.SetActive(true);
    }

    /// <summary>Stops any further aliens, called on game over.</summary>
    public void StopCycle()
    {
        ScoreManager.LevelChanged -= OnLevelChanged;
        alienActive = false;
    }

    private void SetSpawnPointActive(bool active)
    {
        if (spawnPoint != null && spawnPoint.activeSelf != active)
        {
            spawnPoint.SetActive(active);
        }
    }
}
