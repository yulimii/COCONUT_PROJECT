using UnityEngine;
using UnityEngine.UI;

public class DialogueView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text npcNameText;
    [SerializeField] private Text dialogueText;
    [SerializeField] private Button continueButton;

    [Header("Debug Info")]
    [SerializeField] private bool isDialogueActive = false;

    private void Awake()
    {
        // UI 컴포넌트 자동 찾기 (Inspector에서 할당하지 않은 경우)
        if (dialoguePanel == null)
        {
            dialoguePanel = transform.Find("DialoguePanel")?.gameObject;
        }

        if (npcNameText == null)
        {
            npcNameText = transform.Find("DialoguePanel/NpcNameText")?.GetComponent<Text>();
        }

        if (dialogueText == null)
        {
            dialogueText = transform.Find("DialoguePanel/DialogueText")?.GetComponent<Text>();
        }

        if (continueButton == null)
        {
            continueButton = transform.Find("DialoguePanel/ContinueButton")?.GetComponent<Button>();
        }

        // 시작 시 대화창 숨기기
        HideDialogue();
    }

    // === View 역할: UI 표시만 담당 (비즈니스 로직 없음) ===

    public void ShowDialogue(string npcName, string text)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            isDialogueActive = true;

            // 텍스트 업데이트
            if (npcNameText != null)
                npcNameText.text = npcName;

            if (dialogueText != null)
                dialogueText.text = text;

            Debug.Log($"Dialogue shown: {npcName} - {text}");
        }
        else
        {
            Debug.LogError("Dialogue Panel is not assigned!");
        }
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            isDialogueActive = false;
            Debug.Log("Dialogue hidden");
        }
    }

    // === 상태 확인 ===
    public bool IsDialogueActive => isDialogueActive;

    // === UI 이벤트 (버튼 클릭 등) ===
    public void OnContinueButtonClicked()
    {
        // Presenter(DialogueManager)에게 알림
        HideDialogue();
    }

    // === Inspector 디버그 메서드 ===
    [ContextMenu("Test Show Dialogue")]
    private void TestShowDialogue()
    {
        ShowDialogue("테스트 NPC", "이것은 테스트 대화입니다!");
    }

    [ContextMenu("Test Hide Dialogue")]
    private void TestHideDialogue()
    {
        HideDialogue();
    }
}