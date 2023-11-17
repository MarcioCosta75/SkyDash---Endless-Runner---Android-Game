using UnityEngine;

public class MagneticField : MonoBehaviour
{
    public AudioClip collisionSound; // Som de colisão com o obstáculo
    public GameObject particleEffect; // Prefab da animação de partículas
    public SpriteRenderer fieldSprite; // Referência ao componente SpriteRenderer do campo magnético
    public float blinkInterval = 0.5f; // Intervalo de tempo para piscar (em segundos)
    public float blinkDuration = 3f; // Duração total do piscar (em segundos)

    private float blinkTimer; // Temporizador para controlar o piscar
    private bool isBlinking; // Flag para indicar se o campo está piscando

    private void Start()
    {
        blinkTimer = blinkDuration; // Inicia o temporizador com a duração total do piscar
        isBlinking = false; // Inicia o campo sem piscar
    }

    private void Update()
    {
        if (isBlinking)
        {
            blinkTimer -= Time.deltaTime; // Atualiza o temporizador

            if (blinkTimer <= 0f) // Se o temporizador chegar a zero
            {
                // Desativa o campo magnético
                fieldSprite.enabled = false;
                isBlinking = false;
            }
            else
            {
                // Calcula se o campo deve estar visível ou não com base no intervalo de piscar
                bool isVisible = (blinkTimer % blinkInterval) > blinkInterval / 2f;

                // Define a visibilidade do campo magnético
                fieldSprite.enabled = isVisible;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            // Reproduzir o som de colisão
            AudioSource.PlayClipAtPoint(collisionSound, collision.transform.position);

            // Instanciar a animação de partículas
            Instantiate(particleEffect, collision.transform.position, Quaternion.identity);

            // Destruir o obstáculo
            Destroy(collision.gameObject);
        }
    }

    private void StartBlinking()
    {
        fieldSprite.enabled = true; // Ativa o campo magnético
        isBlinking = true; // Marca o campo como piscando
        blinkTimer = blinkDuration; // Reinicia o temporizador
    }
}
