using UnityEngine;

[System.Serializable]
public class DialogueData
{
    [Header("NPC Information")]
    [SerializeField] private string npcName = "NPC";

    [Header("Dialogue Content")]
    [TextArea(2, 4)]
    [SerializeField] private string[] dialogueLines = { "안녕하세요!" };

    [Header("Dialogue State")]
    [SerializeField] private int currentLineIndex = 0;

    // === 순수 데이터 프로퍼티 (MVP 패턴의 Model) ===
    public string NpcName
    {
        get => npcName;
        set => npcName = value;
    }

    public string[] DialogueLines
    {
        get => dialogueLines;
        set => dialogueLines = value;
    }

    public int CurrentLineIndex
    {
        get => currentLineIndex;
        set => currentLineIndex = value;
    }

    // === 현재 대사 관련 프로퍼티 ===
    public string CurrentDialogueText
    {
        get
        {
            if (dialogueLines == null || dialogueLines.Length == 0)
                return "대화 내용이 없습니다.";

            if (currentLineIndex >= 0 && currentLineIndex < dialogueLines.Length)
                return dialogueLines[currentLineIndex];

            return "잘못된 대화 인덱스입니다.";
        }
    }

    public bool HasNextLine => currentLineIndex < dialogueLines.Length - 1;
    public bool IsLastLine => currentLineIndex >= dialogueLines.Length - 1;
    public int TotalLines => dialogueLines?.Length ?? 0;

    // === 대화 진행 메서드 ===
    public bool MoveToNextLine()
    {
        if (HasNextLine)
        {
            currentLineIndex++;
            return true;
        }
        return false;
    }

    public void ResetDialogue()
    {
        currentLineIndex = 0;
    }

    // === 생성자 ===
    public DialogueData()
    {
        npcName = "NPC";
        dialogueLines = new string[] { "안녕하세요!" };
        currentLineIndex = 0;
    }

    public DialogueData(string name, params string[] lines)
    {
        npcName = name;
        dialogueLines = lines ?? new string[] { "안녕하세요!" };
        currentLineIndex = 0;
    }

    // === 하위 호환성을 위한 프로퍼티 ===
    public string DialogueText
    {
        get => CurrentDialogueText;
        set
        {
            if (dialogueLines == null || dialogueLines.Length == 0)
                dialogueLines = new string[1];
            dialogueLines[0] = value;
            currentLineIndex = 0;
        }
    }

    // === 디버그용 ===
    public override string ToString()
    {
        return $"{npcName}: {CurrentDialogueText} ({currentLineIndex + 1}/{TotalLines})";
    }
}