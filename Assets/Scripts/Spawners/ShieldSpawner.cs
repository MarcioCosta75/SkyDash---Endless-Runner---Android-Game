using UnityEngine;

public class ShieldSpawner : MonoBehaviour
{
    public GameObject shieldPrefab;
    public float spawnInterval = 2f;
    public float fallSpeed = 5f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        InvokeRepeating("SpawnShield", 0f, spawnInterval);
    }

    private void SpawnShield()
    {
        Vector3 cameraBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 cameraTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        float randomX = Random.Range(cameraBottomLeft.x, cameraTopRight.x);
        float randomY = cameraTopRight.y; // Sempre acima da área visível da câmera
        float randomZ = transform.position.z; // Manter a posição Z do spawner

        Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);

        GameObject shield = Instantiate(shieldPrefab, spawnPosition, Quaternion.identity);
        // Certifique-se de adicionar o componente ShieldController ao shieldPrefab para lidar com a lógica de coleta e ativação do escudo

        // Se desejar, você pode definir outras propriedades ou comportamentos para o shield aqui

        Rigidbody2D shieldRigidbody = shield.GetComponent<Rigidbody2D>();
        if (shieldRigidbody != null)
        {
            shieldRigidbody.gravityScale = 0f;
            shieldRigidbody.velocity = new Vector2(0f, -fallSpeed);
        }
    }
}
