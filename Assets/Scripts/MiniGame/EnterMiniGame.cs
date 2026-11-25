
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterMiniGame : MonoBehaviour
{
    [Header("Настройки")]
    public string miniGameSceneName;
    public GameObject player;

    [Header("Точка возврата")]
    public Transform returnPoint;

    private bool alreadyEntered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyEntered)
            return;

        if (other.gameObject != player)
            return;

        alreadyEntered = true;

        // Сохраняем безопасную позицию возврата
        if (returnPoint != null)
            MiniGameManager.returnPosition = returnPoint.position;
        else
            MiniGameManager.returnPosition = player.transform.position;

        // Сохраняем название сцены
        MiniGameManager.returnScene =
            SceneManager.GetActiveScene().name;

        MiniGameManager.shouldReturn = true;

        Debug.Log("Сохранена сцена: " + MiniGameManager.returnScene);
        Debug.Log("Точка возврата: " + MiniGameManager.returnPosition);

        SceneManager.LoadScene(miniGameSceneName);
    }
}
