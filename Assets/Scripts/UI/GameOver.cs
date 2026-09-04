using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows the game over panel and shuts the run down.
/// It reacts to the player's death event rather than polling health, and it
/// clears the paused state so the next run starts clean.
/// </summary>
public class GameOver : MonoBehaviour
{
    [Header("Scene objects")]
    [SerializeField]
    private GameObject gameoverpanel;
    [SerializeField]
    private SpawnObstacles spawnObstacles;
    [SerializeField]
    private ParticleSystem starsParticleSystem;
    [SerializeField]
    private AudioSource deathSoundEffect;
    [SerializeField]
    private GameObject objectToDeactivate1;
    [SerializeField]
    private GameObject objectToDeactivate2;

    private bool gameOver;
    private Health playerHealth;
    private ScoreManager scoreManager;

    private void Start()
    {
        scoreManager = FindAnyObjectByType<ScoreManager>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
        }

        if (playerHealth != null)
        {
            playerHealth.Died += GameOverSequence;
        }

        BackgroundMusic.PlayForScene(SceneNames.Game);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= GameOverSequence;
        }
    }

    public void Restart()
    {
        ResumeTime();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void ResumeTime()
    {
        Time.timeScale = 1f;
        PauseMenu.ResetPauseState();
    }

    private void GameOverSequence()
    {
        if (gameOver)
        {
            return;
        }

        gameOver = true;

        if (gameoverpanel != null)
        {
            gameoverpanel.SetActive(true);
        }

        if (spawnObstacles != null)
        {
            spawnObstacles.enabled = false;
        }

        if (scoreManager != null)
        {
            scoreManager.StopScoring();
        }

        // The pause button is not inside the group that gets hidden, and the
        // Android back button is always listening, so pausing has to be shut
        // off explicitly or the two panels stack.
        PauseMenu pauseMenu = FindAnyObjectByType<PauseMenu>();
        if (pauseMenu != null)
        {
            pauseMenu.DisablePausing();
        }

        // A root object, so it is not switched off with GameCapsule.
        EnemyManager enemyManager = FindAnyObjectByType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.StopCycle();
        }

        BackgroundMusic.StopMusic();

        if (starsParticleSystem != null)
        {
            starsParticleSystem.Pause();
        }

        if (deathSoundEffect != null)
        {
            deathSoundEffect.Play();
        }

        if (objectToDeactivate1 != null)
        {
            objectToDeactivate1.SetActive(false);
        }

        if (objectToDeactivate2 != null)
        {
            objectToDeactivate2.SetActive(false);
        }
    }
}
