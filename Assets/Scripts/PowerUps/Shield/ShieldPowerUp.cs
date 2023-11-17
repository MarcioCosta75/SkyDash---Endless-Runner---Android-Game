using UnityEngine;

public class ShieldPowerUp : MonoBehaviour
{
    public GameObject magneticFieldPrefab;
    public float magneticFieldDuration = 10f;
    public AudioClip collisionSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ActivateShieldPowerUp(collision.gameObject);
            Destroy(gameObject);

            if (collisionSound != null)
            {
                AudioSource.PlayClipAtPoint(collisionSound, transform.position);
            }
        }
    }

    private void ActivateShieldPowerUp(GameObject player)
    {
        // Crie o campo magnético em torno do jogador
        GameObject magneticField = Instantiate(magneticFieldPrefab, player.transform.position, Quaternion.identity);
        magneticField.transform.SetParent(player.transform);

        // Ative o campo magnético por um determinado tempo
        Destroy(magneticField, magneticFieldDuration);
    }
}