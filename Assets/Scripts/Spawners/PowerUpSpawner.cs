using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public GameObject powerUpPrefab;
    public float spawnInterval = 10f; // Intervalo de spawn em segundos
    public float fallSpeed = 5f;

    private Camera mainCamera;
    private AudioSource audioSource; // Componente para reproduzir o som

    private void Start()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>(); // Obt�m o componente AudioSource

        InvokeRepeating("SpawnPowerUp", 0f, spawnInterval);
    }

    private void SpawnPowerUp()
    {
        Vector3 cameraBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 cameraTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        float randomX = Random.Range(cameraBottomLeft.x, cameraTopRight.x);
        float randomY = cameraTopRight.y; // Sempre acima da �rea vis�vel da c�mera
        float randomZ = transform.position.z; // Manter a posi��o Z do spawner

        Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);

        GameObject powerUp = Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);
        // Certifique-se de adicionar o componente MagnetController ao powerUpPrefab para lidar com a l�gica de coleta e destrui��o

        // Se desejar, voc� pode definir outras propriedades ou comportamentos para o power-up aqui

        Rigidbody2D powerUpRigidbody = powerUp.GetComponent<Rigidbody2D>();
        if (powerUpRigidbody != null)
        {
            powerUpRigidbody.gravityScale = 0f;
            powerUpRigidbody.linearVelocity = new Vector2(0f, -fallSpeed);
        }
    }
    
}
