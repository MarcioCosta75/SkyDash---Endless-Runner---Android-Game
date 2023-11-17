using UnityEngine;

public class MissileSpawner : MonoBehaviour
{
    public GameObject missilePrefab;
    public float spawnInterval;
    public float spawnSpeed;
    public float spawnOffsetY;
    public float movementSpeed = 3f; // Velocidade de movimento do spawner

    public int projectileSharpHitsToDestroy = 3; // Número de vezes que o MissileSpawner pode ser atingido antes de ser destruído

    public AudioClip activeSound; // Som a ser reproduzido quando o MissileSpawner está ativo
    public GameObject explosionEffectPrefab; // Prefab da animação de explosão

    private float direction = 1f;
    private int projectileSharpHits = 0; // Contador de vezes que o MissileSpawner foi atingido
    private Camera mainCamera;
    private AudioSource audioSource;
    private bool isActive = true; // Indica se o MissileSpawner está ativo

    public AudioClip destructionSound; // Som a ser reproduzido quando o MissileSpawner é destruído

    public AudioClip hitSound;

    private void Start()
    {
        InvokeRepeating("SpawnMissile", spawnInterval, spawnInterval);
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = activeSound;
        audioSource.Play();
    }

    private void Update()
    {
        if (isActive)
        {
            MoveSpawner();
        }
    }

    private void SpawnMissile()
    {
        if (isActive)
        {
            Vector3 spawnPosition = transform.position + new Vector3(0f, spawnOffsetY, 0f);
            Quaternion spawnRotation = Quaternion.identity;
            GameObject missile = Instantiate(missilePrefab, spawnPosition, spawnRotation);
            Rigidbody2D missileRigidbody = missile.GetComponent<Rigidbody2D>();
            missileRigidbody.velocity = Vector2.down * spawnSpeed;
        }
    }

    private void MoveSpawner()
    {
        float movement = spawnSpeed * direction * Time.deltaTime;
        transform.Translate(new Vector3(movement, 0f, 0f));

        // Movimenta o spawner no eixo X
        transform.Translate(Vector3.right * direction * movementSpeed * Time.deltaTime);

        // Converte a posição do spawner em coordenadas de viewport
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        // Limita o movimento do spawner dentro dos limites da viewport
        viewportPosition.x = Mathf.Clamp01(viewportPosition.x);

        // Converte de volta para a posição do mundo
        transform.position = mainCamera.ViewportToWorldPoint(viewportPosition);

        // Verifica se o spawner está além dos limites da tela e muda a direção
        if (viewportPosition.x <= 0f || viewportPosition.x >= 1f)
        {
            direction *= -1f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ProjectileSharp"))
        {
            projectileSharpHits++;
            if (projectileSharpHits >= projectileSharpHitsToDestroy)
            {
                DestroySpawner();
            }
            else
            {
                audioSource.PlayOneShot(hitSound);
            }
        }
    }

    private void DestroySpawner()
    {
        audioSource.Stop();
        Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        gameObject.SetActive(false); // Desativa o MissileSpawner em vez de destruí-lo
        audioSource.clip = destructionSound;
        audioSource.Play();
        isActive = false; // Define o MissileSpawner como inativo
    }
}
