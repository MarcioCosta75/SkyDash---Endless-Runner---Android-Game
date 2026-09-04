using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads scenes from UI buttons. Always restores normal time first, so a
/// button pressed while paused does not leave the next scene frozen.
/// </summary>
public class SceneLoadManager : MonoBehaviour
{
    public void LoadScene(int sceneIndex)
    {
        PauseMenu.ResetPauseState();
        SceneManager.LoadScene(sceneIndex);
    }

    public void LoadSceneByName(string sceneName)
    {
        PauseMenu.ResetPauseState();
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
