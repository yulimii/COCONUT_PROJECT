using UnityEngine;
using UnityEngine.UI;

public class QuestView : MonoBehaviour
{
    [Header("Quest UI Elements")]
    [SerializeField] private GameObject questPanel;
    [SerializeField] private Text questTitleText;
    [SerializeField] private Text questDescriptionText;
    [SerializeField] private Text questProgressText;

    [Header("Completion UI")]
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private Text completionText;
    [SerializeField] private float completionDisplayTime = 2f;

    [Header("UI Settings")]
    [SerializeField] private bool showDebugMessages = true;

    private bool isQuestDisplayActive = false;
    private float completionTimer = 0f;

    // === 초기화 ===
    void Start()
    {
        // UI 요소들이 할당되지 않은 경우 자동으로 찾기
        InitializeUIElements();

        // 초기 상태 설정
        if (questPanel != null)
            questPanel.SetActive(false);

        if (completionPanel != null)
            completionPanel.SetActive(false);

        if (showDebugMessages)
        {
            Debug.Log("QuestView initialized");
        }
    }

    void Update()
    {
        // 완료 메시지 타이머 처리
        HandleCompletionTimer();
    }

    // === UI 요소 자동 찾기 ===
    private void InitializeUIElements()
    {
        // questPanel이 없으면 자식에서 찾기
        if (questPanel == null)
        {
            Transform questPanelTransform = transform.Find("QuestPanel");
            if (questPanelTransform != null)
                questPanel = questPanelTransform.gameObject;
        }

        // Text 요소들 자동 찾기
        if (questPanel != null)
        {
            if (questTitleText == null)
            {
                Transform titleTransform = questPanel.transform.Find("QuestTitle");
                if (titleTransform != null)
                    questTitleText = titleTransform.GetComponent<Text>();
            }

            if (questDescriptionText == null)
            {
                Transform descTransform = questPanel.transform.Find("QuestDescription");
                if (descTransform != null)
                    questDescriptionText = descTransform.GetComponent<Text>();
            }

            if (questProgressText == null)
            {
                Transform progressTransform = questPanel.transform.Find("QuestProgress");
                if (progressTransform != null)
                    questProgressText = progressTransform.GetComponent<Text>();
            }
        }

        // 완료 패널 자동 찾기
        if (completionPanel == null)
        {
            Transform completionTransform = transform.Find("CompletionPanel");
            if (completionTransform != null)
            {
                completionPanel = completionTransform.gameObject;
                if (completionText == null)
                {
                    completionText = completionPanel.GetComponentInChildren<Text>();
                }
            }
        }
    }

    // === 퀘스트 표시 (Presenter에서 호출) ===
    public void UpdateQuestDisplay(QuestData questData, LoveModel.Emotion currentEmotion)
    {
        if (questData == null)
        {
            HideQuestDisplay();
            return;
        }

        // 퀘스트 패널 활성화
        if (questPanel != null)
            questPanel.SetActive(true);

        // 퀘스트 정보 표시
        if (questTitleText != null)
            questTitleText.text = questData.Title;

        if (questDescriptionText != null)
            questDescriptionText.text = questData.Description;

        // 진행 상황 표시
        if (questProgressText != null)
        {
            string status = questData.IsCompleted ? "[완료]" : "[진행중]";
            string progress = $"{status} 목표: {questData.TargetEmotion} | 현재: {currentEmotion}";
            questProgressText.text = progress;
        }

        isQuestDisplayActive = true;

        if (showDebugMessages)
        {
            Debug.Log($"Quest Display Updated: {questData.Title}");
        }
    }

    // === 퀘스트 표시 숨기기 ===
    public void HideQuestDisplay()
    {
        if (questPanel != null)
            questPanel.SetActive(false);

        isQuestDisplayActive = false;

        if (showDebugMessages)
        {
            Debug.Log("Quest Display Hidden");
        }
    }

    // === 퀘스트 완료 메시지 표시 ===
    public void ShowQuestCompletion(string questTitle)
    {
        if (completionPanel != null && completionText != null)
        {
            completionPanel.SetActive(true);
            completionText.text = $"퀘스트 완료!\n{questTitle}";
            completionTimer = completionDisplayTime;

            if (showDebugMessages)
            {
                Debug.Log($"Quest Completion Shown: {questTitle}");
            }
        }
    }

    // === 완료 메시지 타이머 처리 ===
    private void HandleCompletionTimer()
    {
        if (completionTimer > 0f)
        {
            completionTimer -= Time.deltaTime;

            if (completionTimer <= 0f)
            {
                if (completionPanel != null)
                    completionPanel.SetActive(false);
            }
        }
    }

    // === 공개 프로퍼티 ===
    public bool IsQuestDisplayActive => isQuestDisplayActive;

    // === Inspector 디버그 메서드 ===
    [ContextMenu("Test Show Quest")]
    private void TestShowQuest()
    {
        QuestData testQuest = new QuestData(1, "테스트 퀘스트", "이것은 테스트 퀘스트입니다.", LoveModel.Emotion.Happy);
        UpdateQuestDisplay(testQuest, LoveModel.Emotion.Idle);
    }

    [ContextMenu("Test Show Completion")]
    private void TestShowCompletion()
    {
        ShowQuestCompletion("테스트 퀘스트");
    }

    [ContextMenu("Hide Quest Display")]
    private void TestHideQuest()
    {
        HideQuestDisplay();
    }
}