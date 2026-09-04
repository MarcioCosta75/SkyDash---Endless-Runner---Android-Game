using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause overlay. The paused flag is static so other scripts can read it,
/// which means it also has to be cleared whenever the scene changes.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenuUI;

    public static bool GameIsPaused { get; private set; }

    private void OnDestroy()
    {
        ResetPauseState();
    }

    private void Update()
    {
        // Back button on Android maps to Escape.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }

    public void OnPauseButtonClicked()
    {
        Toggle();
    }

    private void Toggle()
    {
        if (GameIsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    private void Pause()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void LoadMenu()
    {
        ResetPauseState();
        SceneManager.LoadScene(SceneNames.Menu);
    }

    public void QuitGame()
    {
        ResetPauseState();
        Application.Quit();
    }

    /// <summary>Clears the paused flag and restores normal time.</summary>
    public static void ResetPauseState()
    {
        GameIsPaused = false;
        Time.timeScale = 1f;
    }
}
