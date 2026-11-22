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
    public string questId;
    public int progressAmount = 1;
    public bool oneTime = true;
    public bool activated = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneTime && activated) return;
        activated = true;
        switch (action) {
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

    }


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
