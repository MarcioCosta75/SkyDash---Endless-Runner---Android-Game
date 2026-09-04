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

    [Header("Ammo counter colours")]
    [SerializeField]
    private Color fullAmmoColour = Color.white;
    [SerializeField]
    private Color emptyAmmoColour = new Color(1f, 0.4f, 0.4f);

    private const float TargetSearchInterval = 0.15f;

    private int currentProjectiles;
    private bool buttonEnabled = true;
    private float nextTargetSearch;

    private void OnEnable()
    {
        PlayerController.Tapped += OnShootButtonClick;
    }

    private void OnDisable()
    {
        PlayerController.Tapped -= OnShootButtonClick;
    }

    private void Start()
    {
        maxProjectiles += PlayerUpgrades.ExtraAmmo;
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
        // Searching by tag is not free, and the answer changes rarely.
        if (Time.time >= nextTargetSearch)
        {
            nextTargetSearch = Time.time + TargetSearchInterval;
            RefreshButtonState();
        }
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

    /// <summary>
    /// Greyed out only when the magazine is empty, which is a state the player
    /// can fix by collecting a pickup. It stays usable the rest of the time,
    /// because a shot breaks obstacles as well as hurting the alien.
    /// </summary>
    private void RefreshButtonState()
    {
        if (shootButton == null)
        {
            return;
        }

        bool haveAmmo = currentProjectiles > 0;
        if (haveAmmo != buttonEnabled)
        {
            buttonEnabled = haveAmmo;
            shootButton.interactable = haveAmmo;
        }
    }

    private void OnShootButtonClick()
    {
        if (currentProjectiles <= 0)
        {
            return;
        }

        Shoot(FindTarget());
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

        // Up to start with when there is no alien. The projectile picks the
        // nearest obstacle ahead on its first steer, a frame later.
        Vector2 direction = target != null
            ? ((Vector2)(target.transform.position - projectile.transform.position)).normalized
            : Vector2.up;

        // The projectile owns its movement. Setting the velocity here as well
        // made the shot travel at both speeds added together.
        Projectile shot = projectile.GetComponent<Projectile>();
        if (shot != null)
        {
            shot.Launch(direction, projectileSpeed);
            return;
        }

        Rigidbody2D body = projectile.GetComponent<Rigidbody2D>();
        if (body != null)
        {
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
        if (bulletsText == null)
        {
            return;
        }

        bulletsText.text = "Bullets " + currentProjectiles;

        // With the fire button retired, the greyed out button no longer says
        // "empty", so the counter does.
        bulletsText.color = currentProjectiles > 0 ? fullAmmoColour : emptyAmmoColour;
    }
}
