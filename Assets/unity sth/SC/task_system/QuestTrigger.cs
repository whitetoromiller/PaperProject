using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public enum QuestAction
    {
        Add, 
        Progress,
        Complete
    }

    public QuestAction action;
    public int questId;
    public int progressAmount = 1;
    public bool oneTime = true;
    public bool playerNear = false;
    public bool activated = false;
    public bool need_press;

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
            case QuestAction.Add:
                QuestManager.Instance.AddQuest(questId);
                break;
            case QuestAction.Progress:
                QuestManager.Instance.ProgressQuest(questId, progressAmount);
                break;
            case QuestAction.Complete:
                QuestManager.Instance.CompleteQuest(questId);
                break;
        }

        activated = true;
    }
}
