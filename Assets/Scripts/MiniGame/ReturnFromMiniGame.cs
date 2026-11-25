using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnFromMiniGame : MonoBehaviour
{
    public void ReturnToGame()
    {
        // Проверяем, сохранена ли сцена
        if (string.IsNullOrEmpty(MiniGameManager.returnScene))
        {
            Debug.LogError("Не сохранено название предыдущей сцены!");
            return;
        }

        Debug.Log("Возвращаемся в сцену: " + MiniGameManager.returnScene);

        SceneManager.LoadScene(MiniGameManager.returnScene);
    }
}