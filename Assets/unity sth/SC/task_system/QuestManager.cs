// using UnityEditorInternal;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [System.Serializable] public class QuestInfo
    {
        public int id = 1;
        public string title = "";
        public int maxProgress = 1;
        [HideInInspector] public int progress = 0;
        [HideInInspector] public bool completed = false;
        [HideInInspector] public bool active = false;
        public bool need_press = false;
        

    }
    private bool loh = false;
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

    public void AddQuest(int id)
    {
        QuestInfo quest = FindQuest(id);
        if (quest == null || quest.completed) return;
        quest.active = true;
        UpdateQuestText();
    }

    public void ProgressQuest(int id, int amount)
    {
        QuestInfo quest = FindQuest(id);
        if (quest == null || quest.completed || (!quest.active)) return;
        if (amount == 0) loh = true;
        else loh = false;
        quest.progress += amount;
        quest.progress = Mathf.Clamp(quest.progress, 0, quest.maxProgress);
        if (quest.progress >= quest.maxProgress) CompleteQuest(id);
        UpdateQuestText();
    }

    public void CompleteQuest(int id)
    {
        QuestInfo quest = FindQuest(id);
        if (quest == null || quest.completed) return;
        loh = false;
        quest.completed = true;
        quest.active = false;
        if (quest.id % 2 == 1) { 
            FindQuest(id + 1).active = true; 
        }
        UpdateQuestText();
    }

    public QuestInfo FindQuest(int id)
    {
        return quests.Find(q => q.id == id);
    }

    void UpdateQuestText()
    {
        string text = "<b> ТЕКУЩИЕ ЗАДАНИЯ <b>\n";

        foreach (QuestInfo quest in quests)
        {
            if (quest.completed) text += "<color=green>v " + quest.title + "</color>\n";
        }


        foreach (QuestInfo quest in quests)
        {
            if (quest.active && !quest.completed)
            {
                text += "* " + quest.title;
                if (quest.maxProgress > 1) text += " (" + quest.progress.ToString() + "/" + quest.maxProgress.ToString() + ")";
                text += '\n';
            }
        }

        if (loh)
        {
            text += "ХАХАХАХАХХАХА А ЯГОД НЕТУ :) \n";
        }

        if((!FindQuest(3).completed) && FindQuest(3).active){
            text += "Соберите все ягоды, чтобы выйти";
        }
        
        questText.text = text;
    }

}
