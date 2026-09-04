using System;
using UnityEngine;

/// <summary>
/// The shield pickup. Wraps a magnetic field around the player for a set
/// time and announces that time so the UI bar matches exactly.
/// </summary>
public class ShieldPowerUp : MonoBehaviour
{
    [SerializeField]
    private GameObject magneticFieldPrefab;
    [SerializeField]
    private float magneticFieldDuration = 10f;
    [SerializeField]
    private AudioClip collisionSound;

    /// <summary>Raised on pickup, carrying how long the shield lasts.</summary>
    public static event Action<float> ShieldActivated;

    private bool used;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (used || !collision.CompareTag("Player"))
        {
            return;
        }

        used = true;
        ActivateShieldPowerUp(collision.gameObject);

        if (collisionSound != null)
        {
            SoundPlayer.Play(collisionSound, 0.8f);
        }

        Destroy(gameObject);
    }

    private void ActivateShieldPowerUp(GameObject player)
    {
        if (magneticFieldPrefab == null)
        {
            return;
        }

        float duration = magneticFieldDuration + PlayerUpgrades.ExtraShieldSeconds;

        GameObject magneticField = Instantiate(magneticFieldPrefab, player.transform.position, Quaternion.identity);
        magneticField.transform.SetParent(player.transform);

        // Tell the blink effect when to warn the player it is running out.
        BlinkEffect blink = magneticField.GetComponentInChildren<BlinkEffect>();
        if (blink != null)
        {
            blink.BeginFor(duration);
        }

        Destroy(magneticField, duration);
        ShieldActivated?.Invoke(duration);
    }
}
