using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Data")]
    [SerializeField] private DialogueData dialogueData;

    [Header("UI Reference")]
    [SerializeField] private DialogueView dialogueView;

    [Header("Quest System")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private bool giveQuestAfterDialogue = true;

    [Header("Interaction Settings")]
    [SerializeField] private bool showDebugMessages = true;

    private bool isPlayerNearby = false;

    // === 초기화 ===
    void Start()
    {
        // DialogueData가 없으면 기본값 생성
        if (dialogueData == null)
        {
            dialogueData = new DialogueData("NPC", "안녕하세요! 좋은 하루예요.");
        }

        // DialogueView 자동 찾기 (할당되지 않은 경우)
        if (dialogueView == null)
        {
            dialogueView = FindFirstObjectByType<DialogueView>();
            if (dialogueView == null && showDebugMessages)
            {
                Debug.LogWarning("DialogueView not found in scene!");
            }
        }

        // QuestManager 자동 찾기 (할당되지 않은 경우)
        if (questManager == null)
        {
            questManager = FindFirstObjectByType<QuestManager>();
            if (questManager == null && showDebugMessages)
            {
                Debug.LogWarning("QuestManager not found in scene!");
            }
        }

        if (showDebugMessages)
        {
            Debug.Log($"DialogueManager initialized for {dialogueData.NpcName}");
        }
    }

    // === Update에서 입력 감지 ===
    void Update()
    {
        HandleDialogueInput();
    }

    // === 대화 입력 처리 (Presenter 역할) ===
    private void HandleDialogueInput()
    {
        // 플레이어가 근처에 있고, 대화창이 열려있지 않을 때만 Z키 감지
        if (isPlayerNearby && dialogueView != null && !dialogueView.IsDialogueActive)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                StartDialogue();
            }
        }
        // 대화창이 열려있을 때 Z키로 다음 대사 또는 대화 종료
        else if (dialogueView != null && dialogueView.IsDialogueActive)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                ProcessNextDialogue();
            }
        }
    }

    private void StartDialogue()
    {
        if (dialogueData != null && dialogueView != null)
        {
            // 대화 시작 시 처음부터 시작
            dialogueData.ResetDialogue();
            ShowCurrentDialogue();

            if (showDebugMessages)
            {
                Debug.Log($"Dialogue started with {dialogueData.NpcName}");
            }
        }
    }

    private void ProcessNextDialogue()
    {
        if (dialogueData == null || dialogueView == null)
            return;

        // 다음 대사가 있으면 진행
        if (dialogueData.HasNextLine)
        {
            dialogueData.MoveToNextLine();
            ShowCurrentDialogue();

            if (showDebugMessages)
            {
                Debug.Log($"Next dialogue: {dialogueData.CurrentDialogueText} ({dialogueData.CurrentLineIndex + 1}/{dialogueData.TotalLines})");
            }
        }
        // 마지막 대사였으면 대화 종료
        else
        {
            EndDialogue();
        }
    }

    private void ShowCurrentDialogue()
    {
        if (dialogueData != null && dialogueView != null)
        {
            dialogueView.ShowDialogue(dialogueData.NpcName, dialogueData.CurrentDialogueText);
        }
    }

    private void EndDialogue()
    {
        if (dialogueView != null)
        {
            dialogueView.HideDialogue();

            if (showDebugMessages)
            {
                Debug.Log($"💬 [DIALOGUE] Dialogue ended with {dialogueData?.NpcName}");
            }
        }

        // 퀘스트 지급 처리
        if (giveQuestAfterDialogue && questManager != null)
        {
            GiveQuestToPlayer();
        }

        // 대화 종료 시 다음 번을 위해 리셋
        if (dialogueData != null)
        {
            dialogueData.ResetDialogue();
        }
    }

    // === 퀘스트 지급 처리 ===
    private void GiveQuestToPlayer()
    {
        // 이미 활성 퀘스트가 있는지 확인
        if (questManager.GetCurrentActiveQuest() != null)
        {
            if (showDebugMessages)
            {
                Debug.Log($"📋 [QUEST GIVER] 이미 진행 중인 퀘스트가 있습니다.");
            }
            return;
        }

        // 새 퀘스트 생성
        QuestData newQuest = new QuestData(
            1,
            "친밀도 높이기",
            "NPC의 호감도를 Happy로 만들어보세요!",
            LoveModel.Emotion.Happy
        );

        // QuestManager에 퀘스트 추가
        questManager.AddQuest(newQuest);

        if (showDebugMessages)
        {
            Debug.Log($"🎯 [QUEST GIVER] 새로운 퀘스트를 받았습니다: {newQuest.Title}");
        }
    }

    // === Trigger 이벤트 (별도의 작은 Collider에서 호출) ===
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;

            if (showDebugMessages)
            {
                Debug.Log($"[DialogueManager] Player can talk to {dialogueData?.NpcName ?? "NPC"}. Press Z to talk! - {gameObject.name}");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;

            // 대화 중이었다면 대화 종료
            if (dialogueView != null && dialogueView.IsDialogueActive)
            {
                EndDialogue();
            }

            if (showDebugMessages)
            {
                Debug.Log($"[DialogueManager] Player left {dialogueData?.NpcName ?? "NPC"} interaction area. - {gameObject.name}");
            }
        }
    }

    // === 공개 메서드 (다른 스크립트에서 사용 가능) ===
    public void SetDialogueData(string npcName, string dialogueText)
    {
        if (dialogueData == null)
        {
            dialogueData = new DialogueData();
        }

        dialogueData.NpcName = npcName;
        dialogueData.DialogueText = dialogueText;
    }

    public bool IsPlayerNearby => isPlayerNearby;
    public bool IsDialogueActive => dialogueView != null && dialogueView.IsDialogueActive;

    // === Inspector 디버그 메서드 ===
    [ContextMenu("Test Start Dialogue")]
    private void TestStartDialogue()
    {
        isPlayerNearby = true;
        StartDialogue();
    }

    [ContextMenu("Test End Dialogue")]
    private void TestEndDialogue()
    {
        EndDialogue();
    }
}