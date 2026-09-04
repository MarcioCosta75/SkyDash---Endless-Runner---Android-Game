using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Fires projectiles at the alien enemy and tracks the ammo count.
/// The fire button is only interactable when there is both ammo and a live
/// target, so shots cannot be wasted while no enemy is on screen.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private float projectileSpeed = 10f;

    [Header("Ammo")]
    [SerializeField]
    private int maxProjectiles = 10;
    [SerializeField]
    private TextMeshProUGUI bulletsText;
    [SerializeField]
    private Button shootButton;

    [Tooltip("Optional. Left empty the target is found by its AlienEnemy tag.")]
    [SerializeField]
    private GameObject alienEnemy;

    private int currentProjectiles;
    private bool buttonEnabled = true;

    public int CurrentProjectiles => currentProjectiles;

    private void Start()
    {
        currentProjectiles = maxProjectiles;
        UpdateBulletsText();

        if (shootButton != null)
        {
            shootButton.onClick.AddListener(OnShootButtonClick);
        }

        RefreshButtonState();
    }

    private void Update()
    {
        RefreshButtonState();
    }

    /// <summary>The live enemy, or null when none is on screen.</summary>
    private GameObject FindTarget()
    {
        if (alienEnemy != null && alienEnemy.activeInHierarchy)
        {
            return alienEnemy;
        }

        GameObject tagged = GameObject.FindGameObjectWithTag("AlienEnemy");
        if (tagged != null && tagged.activeInHierarchy)
        {
            alienEnemy = tagged;
            return tagged;
        }

        return null;
    }

    private void RefreshButtonState()
    {
        if (shootButton == null)
        {
            return;
        }

        bool canFire = currentProjectiles > 0 && FindTarget() != null;
        if (canFire != buttonEnabled)
        {
            buttonEnabled = canFire;
            shootButton.interactable = canFire;
        }
    }

    private void OnShootButtonClick()
    {
        GameObject target = FindTarget();
        if (currentProjectiles <= 0 || target == null)
        {
            return;
        }

        Shoot(target);
        currentProjectiles--;
        UpdateBulletsText();
        RefreshButtonState();
    }

    private void Shoot(GameObject target)
    {
        if (projectilePrefab == null)
        {
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Rigidbody2D body = projectile.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            Vector2 direction = ((Vector2)(target.transform.position - projectile.transform.position)).normalized;
            body.linearVelocity = direction * projectileSpeed;
        }
    }

    public void AcquireProjectiles(int amount)
    {
        currentProjectiles = Mathf.Clamp(currentProjectiles + amount, 0, maxProjectiles);
        UpdateBulletsText();
        RefreshButtonState();
    }

    private void UpdateBulletsText()
    {
        if (bulletsText != null)
        {
            bulletsText.text = "Bullets " + currentProjectiles;
        }
    }
}
