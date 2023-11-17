using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab; // Prefab do projétil
    public float projectileSpeed = 10f; // Velocidade do projétil

    public int maxProjectiles = 15; // Número máximo de projéteis permitidos
    public int currentProjectiles = 15; // Número atual de projéteis disponíveis

    public TextMeshProUGUI bulletsText; // Referência ao componente TextMeshProUGUI para exibir o número de balas
    public Button shootButton; // Referência ao botão de disparo

    public GameObject alienEnemy; // Referência ao objeto AlienEnemy

    private bool canShoot = true; // Indica se o jogador pode disparar

    private void Start()
    {
        currentProjectiles = maxProjectiles;
        UpdateBulletsText();
        // Resto da inicialização...

        if (shootButton != null)
        {
            shootButton.onClick.AddListener(OnShootButtonClick);
        }
    }

    private void OnShootButtonClick()
    {
        if (currentProjectiles > 0 && canShoot)
        {
            Shoot();
            currentProjectiles--;
            if (currentProjectiles <= 0)
            {
                canShoot = false;
            }
            UpdateBulletsText();
        }
    }

    private void Shoot()
    {
        if (projectilePrefab != null && alienEnemy != null)
        {
            // Cria o projétil na posição atual do jogador
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            // Obtém a direção do projétil em relação ao AlienEnemy
            Vector3 direction = (alienEnemy.transform.position - projectile.transform.position).normalized;

            // Obtém o componente Rigidbody2D do projétil
            Rigidbody2D projectileRigidbody = projectile.GetComponent<Rigidbody2D>();

            // Aplica uma força para mover o projétil na direção calculada
            projectileRigidbody.velocity = direction * projectileSpeed;
        }
    }


    public void AcquireProjectiles(int amount)
    {
        currentProjectiles += amount;
        currentProjectiles = Mathf.Clamp(currentProjectiles, 0, maxProjectiles);

        if (!canShoot && currentProjectiles > 0)
        {
            canShoot = true;
        }
        UpdateBulletsText();
    }

    public void SetCanShoot(bool value)
    {
        canShoot = value;
    }

    private void UpdateBulletsText()
    {
        if (bulletsText != null)
        {
            bulletsText.text = "Bullets " + currentProjectiles.ToString();
        }
    }
}
