using UnityEngine;

public class HeartSpawner : MonoBehaviour
{
    public GameObject heartPrefab;
    public float spawnInterval = 2f;
    public float fallSpeed = 5f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        InvokeRepeating("SpawnHeart", 0f, spawnInterval);
    }

    private void SpawnHeart()
    {
        Vector3 cameraBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 cameraTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        float randomX = Random.Range(cameraBottomLeft.x, cameraTopRight.x);
        float randomY = cameraTopRight.y; // Sempre acima da �rea vis�vel da c�mera
        float randomZ = transform.position.z; // Manter a posi��o Z do spawner

        Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);

        GameObject heart = Instantiate(heartPrefab, spawnPosition, Quaternion.identity);
        // Certifique-se de adicionar o componente HeartController ao heartPrefab para lidar com a l�gica de coleta e restaura��o de vida

        // Se desejar, voc� pode definir outras propriedades ou comportamentos para o heart aqui

        Rigidbody2D heartRigidbody = heart.GetComponent<Rigidbody2D>();
        if (heartRigidbody != null)
        {
            heartRigidbody.gravityScale = 0f;
            heartRigidbody.linearVelocity = new Vector2(0f, -fallSpeed);
        }
    }
}
