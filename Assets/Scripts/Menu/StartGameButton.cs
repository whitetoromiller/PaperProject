using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameButton : MonoBehaviour
{
    [Tooltip("Имя игровой сцены (должна быть в Build Settings)")]
    public string gameSceneName = "GameScene";

    public void StartGame()
    {
        Debug.Log($"Старт игры → {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }
}
