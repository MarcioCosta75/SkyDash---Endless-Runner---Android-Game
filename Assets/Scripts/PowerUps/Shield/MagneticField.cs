using UnityEngine;

public class MagneticField : MonoBehaviour
{
    public AudioClip collisionSound; // Som de colis�o com o obst�culo
    public GameObject particleEffect; // Prefab da anima��o de part�culas
    public SpriteRenderer fieldSprite; // Refer�ncia ao componente SpriteRenderer do campo magn�tico
    public float blinkInterval = 0.5f; // Intervalo de tempo para piscar (em segundos)
    public float blinkDuration = 3f; // Dura��o total do piscar (em segundos)

    private float blinkTimer; // Temporizador para controlar o piscar
    private bool isBlinking; // Flag para indicar se o campo est� piscando

    private void Start()
    {
        blinkTimer = blinkDuration; // Inicia o temporizador com a dura��o total do piscar
        isBlinking = false; // Inicia o campo sem piscar
    }

    private void Update()
    {
        if (isBlinking)
        {
            blinkTimer -= Time.deltaTime; // Atualiza o temporizador

            if (blinkTimer <= 0f) // Se o temporizador chegar a zero
            {
                // Desativa o campo magn�tico
                fieldSprite.enabled = false;
                isBlinking = false;
            }
            else
            {
                // Calcula se o campo deve estar vis�vel ou n�o com base no intervalo de piscar
                bool isVisible = (blinkTimer % blinkInterval) > blinkInterval / 2f;

                // Define a visibilidade do campo magn�tico
                fieldSprite.enabled = isVisible;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            // Reproduzir o som de colis�o
            AudioSource.PlayClipAtPoint(collisionSound, collision.transform.position);

            // Instanciar a anima��o de part�culas
            Instantiate(particleEffect, collision.transform.position, Quaternion.identity);

            // Destruir o obst�culo
            Destroy(collision.gameObject);
        }
    }

    private void StartBlinking()
    {
        fieldSprite.enabled = true; // Ativa o campo magn�tico
        isBlinking = true; // Marca o campo como piscando
        blinkTimer = blinkDuration; // Reinicia o temporizador
    }
}
