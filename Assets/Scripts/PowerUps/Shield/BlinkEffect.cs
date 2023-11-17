using UnityEngine;

public class BlinkEffect : MonoBehaviour
{
    public SpriteRenderer targetSprite; // Refer�ncia ao componente SpriteRenderer do objeto alvo
    public float blinkDuration = 10f; // Dura��o total do efeito de piscar
    public float blinkStartTime = 7f; // Tempo restante para iniciar o efeito de piscar

    private bool isBlinking; // Flag para indicar se o efeito de piscar est� ativo

    private void Start()
    {
        isBlinking = false; // Inicia o efeito sem piscar

        Invoke("StartBlinking", blinkStartTime);
    }

    private void StartBlinking()
    {
        if (!isBlinking)
        {
            isBlinking = true; // Marca o efeito de piscar como ativo

            InvokeRepeating("ToggleSpriteVisibility", 0f, 0.5f); // Invoca repetidamente a fun��o ToggleSpriteVisibility com intervalo de 0.5 segundos

            Invoke("StopBlinking", blinkDuration - 3f); // Invoca a fun��o StopBlinking ap�s a dura��o total do efeito de piscar menos 3 segundos
        }
    }

    private void ToggleSpriteVisibility()
    {
        targetSprite.enabled = !targetSprite.enabled; // Alterna a visibilidade do objeto alvo
    }

    private void StopBlinking()
    {
        isBlinking = false; // Marca o efeito de piscar como inativo
        targetSprite.enabled = true; // Garante que o objeto alvo esteja vis�vel no final do efeito
    }
}
