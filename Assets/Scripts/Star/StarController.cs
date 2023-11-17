using UnityEngine;

public class StarController : MonoBehaviour
{
    private ScoreManager scoreManager;
    private StarSpawner starSpawner;

    private void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        starSpawner = FindObjectOfType<StarSpawner>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CollectStar();
        }
        else if (collision.CompareTag("Border"))
        {
            DestroyStar();
        }
    }

    private void CollectStar()
    {
        // Increment the star counter in ScoreManager
        if (scoreManager != null)
        {
            scoreManager.IncrementTotalStarCounter();
            scoreManager.IncrementStarCounter();
        }

        // Destroy the GameObject "Star"
        Destroy(gameObject);

        // Play the star collection sound in StarSpawner
        if (starSpawner != null)
        {
            starSpawner.PlayCollectSound();
        }
    }

    private void DestroyStar()
    {
        // Destroy the GameObject "Star"
        Destroy(gameObject);
    }
}
