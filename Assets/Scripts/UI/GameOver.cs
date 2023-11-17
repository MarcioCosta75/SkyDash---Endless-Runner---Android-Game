using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject gameoverpanel;
    public SpawnObstacles spawnObstacles;
    public AudioSource backgroundMusic;
    public ParticleSystem starsParticleSystem;
    public AudioSource deathSoundEffect;
    public GameObject objectToDeactivate1;
    public GameObject objectToDeactivate2;

    private bool gameOver = false;

    private Health playerHealth;
    private BackgroundMusic backgroundMusicScript;

    private void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        backgroundMusicScript = FindObjectOfType<BackgroundMusic>();

        if (backgroundMusic != null && backgroundMusicScript != null && !backgroundMusicScript.IsPlaying())
        {
            backgroundMusic.Play();
        }
    }

    private void Update()
    {
        if (!gameOver && playerHealth.health <= 0)
        {
            GameOverSequence();
        }
    }

    public void Restart()
    {
        StopBackgroundMusic();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (backgroundMusic != null)
        {
            backgroundMusic.Play();
        }
    }

    public void MainMenu()
    {
        StopBackgroundMusic();

        SceneManager.LoadScene("MainMenu");
    }

    private void GameOverSequence()
    {
        gameOver = true;
        gameoverpanel.SetActive(true);

        spawnObstacles.enabled = false;

        StopBackgroundMusic();

        starsParticleSystem.Pause();

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

    private void StopBackgroundMusic()
    {
        if (backgroundMusic != null && backgroundMusicScript != null && backgroundMusicScript.IsPlaying())
        {
            backgroundMusic.Stop();
        }
    }
}
