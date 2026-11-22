using UnityEditorInternal;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [System.Serializable] public class QuestInfo
    {
        public string id = "";
        public string title = "";
        public int maxProgress = 1;
        [HideInInspector] public int progress = 0;
        [HideInInspector] public bool completed = false;
        [HideInInspector] public bool active = false;

    }

    public List<QuestInfo> quests = new List<QuestInfo>();
    public TMP_Text questText;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        UpdateQuestText();
    }

    public void AddQuest(string id)
    {
        QuestInfo quest = FindQuest(id);
        if (quest == null || quest.completed) return;
        quest.active = true;
        UpdateQuestText();
    }

    public void ProgressQuest(string id, int amount)
    {
        QuestInfo quest = FindQuest(id);
        if (quest == null || quest.completed) return;
        quest.progress += amount;
        quest.progress = Mathf.Clamp(quest.progress, 0, quest.maxProgress);
        if (quest.progress >= quest.maxProgress) CompleteQuest(id);
        UpdateQuestText();
    }

    public void CompleteQuest(string id)
    {
        QuestInfo quest = FindQuest(id);
        if (quest == null || quest.completed) return;
        quest.completed = true;
        quest.active = false;
        UpdateQuestText();
    }

    QuestInfo FindQuest(string id)
    {
        return quests.Find(q => q.id == id);
    }

    void UpdateQuestText()
    {
        string text = "<b> ТЕКУЩИЕ ЗАДАНИЯ <b>\n";
        foreach (QuestInfo quest in quests)
        {
            if (quest.active && !quest.completed)
            {
                text += "* " + quest.title;
                if (quest.maxProgress > 1) text += " (" + quest.progress.ToString() + "/" + quest.maxProgress.ToString() + ")";
                text += '\n';
            }
        }
        foreach (QuestInfo quest in quests)
        {
            if (quest.completed) text += "<color=green>v " + quest.title + "</color>\n";
        }
        questText.text = text;
    }

}
