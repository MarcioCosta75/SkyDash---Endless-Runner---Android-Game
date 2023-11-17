using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject shootEffect;
    public GameObject hitEffect;
    public GameObject firingShip;
    public float moveSpeed = 10f;
    public float directionUpdateDelay = 0.1f;
    public AudioClip activationSound; // Som a ser reproduzido quando o projetil é ativado
    public AudioClip destructionSound; // Som a ser reproduzido quando o projetil é destruído

    private GameObject alienEnemy;
    private Vector3 direction;
    private float directionUpdateTimer;
    private AudioSource audioSource;

    public int damageAmount = 10; // Quantidade de dano causada pelo projétil

    private void Start()
    {
        Instantiate(shootEffect, transform.position, Quaternion.identity);
        Destroy(gameObject, 5f);

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = activationSound;
        audioSource.Play();

        UpdateProjectileDirection();
    }

    private void Update()
    {
        directionUpdateTimer -= Time.deltaTime;

        if (directionUpdateTimer <= 0f)
        {
            UpdateProjectileDirection();
            directionUpdateTimer = directionUpdateDelay;
        }

        // Move o projétil na direção definida
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    private void UpdateProjectileDirection()
    {
        if (alienEnemy == null)
        {
            alienEnemy = GameObject.FindGameObjectWithTag("AlienEnemy");
        }

        if (alienEnemy != null)
        {
            direction = (alienEnemy.transform.position - transform.position).normalized;
        }
        else
        {
            // Se o objeto AlienEnemy não estiver presente, o projétil irá simplesmente para a frente
            direction = transform.up;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "MagneticField")
        {
            // Ignora a colisão com o MagneticField
            return;
        }

        if (col.gameObject != firingShip && col.gameObject.tag != "ProjectileSharp")
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);

            audioSource.clip = destructionSound;
            audioSource.Play();

            Destroy(gameObject);
        }

        if (col.CompareTag("AlienEnemy"))
        {
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damageAmount);
            }

            // Destruir o projétil
            Destroy(gameObject);
        }
    }
}
