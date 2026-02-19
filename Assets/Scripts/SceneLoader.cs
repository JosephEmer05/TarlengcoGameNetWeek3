using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadStartScene() => SceneManager.LoadScene("Start");
    public void LoadPlayerCustomsScene() => SceneManager.LoadScene("PlayerCustoms");
    public void QuitGame() => Application.Quit();
}