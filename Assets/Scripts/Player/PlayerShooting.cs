using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab; // Prefab do proj�til
    public float projectileSpeed = 10f; // Velocidade do proj�til

    public int maxProjectiles = 15; // N�mero m�ximo de proj�teis permitidos
    public int currentProjectiles = 15; // N�mero atual de proj�teis dispon�veis

    public TextMeshProUGUI bulletsText; // Refer�ncia ao componente TextMeshProUGUI para exibir o n�mero de balas
    public Button shootButton; // Refer�ncia ao bot�o de disparo

    public GameObject alienEnemy; // Refer�ncia ao objeto AlienEnemy

    private bool canShoot = true; // Indica se o jogador pode disparar

    private void Start()
    {
        currentProjectiles = maxProjectiles;
        UpdateBulletsText();
        // Resto da inicializa��o...

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
            // Cria o proj�til na posi��o atual do jogador
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            // Obt�m a dire��o do proj�til em rela��o ao AlienEnemy
            Vector3 direction = (alienEnemy.transform.position - projectile.transform.position).normalized;

            // Obt�m o componente Rigidbody2D do proj�til
            Rigidbody2D projectileRigidbody = projectile.GetComponent<Rigidbody2D>();

            // Aplica uma for�a para mover o proj�til na dire��o calculada
            projectileRigidbody.linearVelocity = direction * projectileSpeed;
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
