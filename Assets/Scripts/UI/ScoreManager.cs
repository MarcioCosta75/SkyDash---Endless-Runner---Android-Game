using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;
    public TextMeshProUGUI starCounterText;
    public TextMeshProUGUI starTotalCounterText;
    public GameObject health;

    public CameraMovement cameraMovement;

    public TextMeshProUGUI levelText; // Texto do nível
    public TextMeshProUGUI speedText; // Texto da velocidade

    private float score;
    private float highscore;
    private int totalStarCounter;
    private int starCounter;

    private const string HighscoreKey = "highscore";
    private const string TotalStarCounterKey = "totalStarCounter";

    void Start()
    {
        highscore = PlayerPrefs.GetFloat(HighscoreKey, 0f);
        highscoreText.text = "Highscore: " + FormatScore(highscore);
        LoadTotalStarCounter();
        UpdateStarTotalCounterText();
        UpdateStarCounterText();
        UpdateLevelAndSpeedText("Level 1", "Speed x1"); // Configurar mensagens iniciais do nível e velocidade
    }

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            score += 1f * Time.deltaTime;
            scoreText.text = FormatScore(score);

            if (score > highscore)
            {
                highscore = score;
                highscoreText.text = "Highscore: " + FormatScore(highscore);
                PlayerPrefs.SetFloat(HighscoreKey, highscore);
            }

            // Verificar o valor do score e ajustar o cameraSpeed
            if (score >= 0 && score < 100)
            {
                cameraMovement.cameraSpeed = 2.5f;
                UpdateLevelAndSpeedText("Level 1", "Speed x1"); // Atualizar mensagens do nível e velocidade
            }
            else if (score >= 100 && score < 200)
            {
                cameraMovement.cameraSpeed = 3f;
                UpdateLevelAndSpeedText("Level 2", "Speed x2"); // Atualizar mensagens do nível e velocidade
            }
            else if (score >= 200 && score < 300)
            {
                cameraMovement.cameraSpeed = 3.5f;
                UpdateLevelAndSpeedText("Level 3", "Speed x3"); // Atualizar mensagens do nível e velocidade
            }
            else if (score >= 300 && score < 400)
            {
                cameraMovement.cameraSpeed = 4f;
                UpdateLevelAndSpeedText("Level 4", "Speed x4"); // Atualizar mensagens do nível e velocidade
            }
            else if (score >= 400 && score < 500)
            {
                cameraMovement.cameraSpeed = 4.5f;
                UpdateLevelAndSpeedText("Level 5", "Speed x5"); // Atualizar mensagens do nível e velocidade
            }
            else if (score >= 500 && score < 600)
            {
                cameraMovement.cameraSpeed = 5f;
                UpdateLevelAndSpeedText("Level 6", "Speed x6"); // Atualizar mensagens do nível e velocidade
            }
            else if (score >= 600 && score < 700)
            {
                cameraMovement.cameraSpeed = 5.5f;
                UpdateLevelAndSpeedText("Level 7", "Speed x7"); // Atualizar mensagens do nível e velocidade
            }
            else if (score >= 700 && score < 800)
            {
                cameraMovement.cameraSpeed = 6f;
                UpdateLevelAndSpeedText("Level 7", "Speed x7"); // Atualizar mensagens do nível e velocidade
            }
            else if (score >= 800 && score < 900)
            {
                cameraMovement.cameraSpeed = 6.5f;
                UpdateLevelAndSpeedText("Level 8", "Speed x8"); // Atualizar mensagens do nível e velocidade
            }
            else if (score >= 900 && score < 1000)
            {
                cameraMovement.cameraSpeed = 7f;
                UpdateLevelAndSpeedText("Level 9", "Speed x9"); // Atualizar mensagens do nível e velocidade
            }
            else if (score >= 1000 && score < 1100)
            {
                cameraMovement.cameraSpeed = 7.5f;
                UpdateLevelAndSpeedText("Level 10", "Speed x10"); // Atualizar mensagens do nível e velocidade
            }
            // Adicione mais condições conforme necessário para outros valores de score

            // Você pode adicionar lógica adicional aqui, como aumentar o cameraSpeed gradualmente com base no score
        }
    }

    public void IncrementTotalStarCounter()
    {
        totalStarCounter++;
        UpdateStarTotalCounterText();
        SaveTotalStarCounter();
    }

    private void UpdateStarTotalCounterText()
    {
        starTotalCounterText.text = "Total: " + totalStarCounter.ToString();
    }

    public void IncrementStarCounter()
    {
        starCounter++;
        UpdateStarCounterText();
    }

    private void UpdateStarCounterText()
    {
        starCounterText.text = "x " + starCounter.ToString();
    }

    private void LoadTotalStarCounter()
    {
        if (PlayerPrefs.HasKey(TotalStarCounterKey))
        {
            totalStarCounter = PlayerPrefs.GetInt(TotalStarCounterKey);
        }
        else
        {
            totalStarCounter = 0;
        }
    }

    private void SaveTotalStarCounter()
    {
        PlayerPrefs.SetInt(TotalStarCounterKey, totalStarCounter); // Salva o valor total de estrelas
    }

    private string FormatScore(float value)
    {
        return ((int)value).ToString() + "m";
    }

    private void UpdateLevelAndSpeedText(string level, string speed)
    {
        levelText.text = level;
        speedText.text = speed;
    }
}
