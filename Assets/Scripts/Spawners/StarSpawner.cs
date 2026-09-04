using UnityEngine;

public class StarSpawner : MonoBehaviour
{
    public GameObject starPrefab;
    public float spawnInterval = 2f;
    public float fallSpeed = 5f;
    public AudioClip collectSound; // Som de coleta da estrela

    private Camera mainCamera;
    private AudioSource audioSource; // Componente para reproduzir o som

    private void Start()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>(); // Obt�m o componente AudioSource

        InvokeRepeating("SpawnStar", 0f, spawnInterval);
    }

    private void SpawnStar()
    {
        Vector3 cameraBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 cameraTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        float randomX = Random.Range(cameraBottomLeft.x, cameraTopRight.x);
        float randomY = cameraTopRight.y; // Sempre acima da �rea vis�vel da c�mera
        float randomZ = transform.position.z; // Manter a posi��o Z do spawner

        Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);

        GameObject star = Instantiate(starPrefab, spawnPosition, Quaternion.identity);
        // Certifique-se de adicionar o componente StarController ao starPrefab para lidar com a l�gica de coleta e destrui��o

        // Se desejar, voc� pode definir outras propriedades ou comportamentos para a estrela aqui

        Rigidbody2D starRigidbody = star.GetComponent<Rigidbody2D>();
        if (starRigidbody != null)
        {
            starRigidbody.gravityScale = 0f;
            starRigidbody.linearVelocity = new Vector2(0f, -fallSpeed);
        }
    }

    // M�todo para reproduzir o som de coleta da estrela
    public void PlayCollectSound()
    {
        audioSource.PlayOneShot(collectSound);
    }
}
