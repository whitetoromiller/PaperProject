using UnityEngine;
using UnityEngine.SceneManagement;
using static QuestTrigger;

public class ExitTrigger : MonoBehaviour
{
    public enum Actions
    {
        Condition,
        NoCondition
    }

    public Actions action;
    public int conditionQuestId = -1;
    public bool oneTime = true;
    public bool playerNear = false;
    public bool activated = false;
    public bool need_press;
    public string scene;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNear = true;

    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNear = false;

    }

    private void Update()
    {
        if (!playerNear) return;
        if (activated && oneTime) return;
        if (!need_press) ActivateTrigger();
        if (Input.GetKeyDown(KeyCode.Return)) ActivateTrigger();
        if (Input.GetKeyDown(KeyCode.Space)) ActivateTrigger();
        return;
    }

    private void ActivateTrigger()
    {
        if (QuestManager.Instance == null) return;
        switch (action)
        {
            case Actions.Condition:
                if (QuestManager.Instance.FindQuest(conditionQuestId).completed)
                {
                    SceneManager.LoadScene(scene);
                }
                break;
            case Actions.NoCondition:
                SceneManager.LoadScene(scene);
                break;
        }

        activated = true;
    }
}
