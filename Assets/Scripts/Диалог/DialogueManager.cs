using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Settings")]

    public GameObject dialoguePrefab;
    public Canvas canvas;

    private DialogueUI dialogueUI;

    private DialogueLine[] lines;

    private int currentLine;
    private bool isActive = false;

    void Update()
    {
        if (!isActive)
            return;

        // Следующая реплика
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0))
        {
            NextLine();
        }
    }

    public void StartDialogue(DialogueLine[] dialogueLines)
    {
        // Проверка, не запущен ли уже диалог
        if (isActive)
            return;

        if (dialogueLines == null || dialogueLines.Length == 0)
            return;

        lines = dialogueLines;
        currentLine = 0;

        // Создаём окно
        GameObject window = Instantiate(
            dialoguePrefab,
            canvas.transform,
            false
        );

        dialogueUI = window.GetComponent<DialogueUI>();

        isActive = true;

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        dialogueUI.ShowLine(lines[currentLine]);
    }

    void NextLine()
    {
        currentLine++;

        // Проверяем, закончился ли диалог
        if (currentLine >= lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    void EndDialogue()
    {
        isActive = false;

        Destroy(dialogueUI.gameObject);

        dialogueUI = null;
        lines = null;
    }
}