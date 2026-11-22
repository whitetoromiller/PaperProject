using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue Elements")]

    public Image characterSprite;
    public Button characterNameButton;
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;
    public GameObject indicator;

    public void ShowLine(DialogueLine line)
    {
        // Имя персонажа
        characterNameText.text = line.characterName;

        // Текст реплики
        dialogueText.text = line.dialogueText;

        // Спрайт персонажа
        characterSprite.sprite = line.characterSprite;

        // Если спрайта нет, скрываем Image
        characterSprite.enabled = line.characterSprite != null;

        // Показываем индикатор
        indicator.SetActive(true);
    }
}