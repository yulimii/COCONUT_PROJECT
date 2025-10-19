using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private List<QuestData> activeQuests = new List<QuestData>();
    [SerializeField] private bool showDebugMessages = true;

    [Header("UI Reference")]
    [SerializeField] private QuestView questView;

    private LoveModel.Emotion lastCheckedEmotion = LoveModel.Emotion.None;

    // === 초기화 ===
    void Start()
    {
        Debug.Log("🚀 [QUEST MANAGER] QuestManager Start() 호출됨!");

        // QuestView 자동 찾기 (할당되지 않은 경우)
        if (questView == null)
        {
            questView = FindFirstObjectByType<QuestView>();
            if (questView == null && showDebugMessages)
            {
                Debug.LogWarning("⚠️ [QUEST MANAGER] QuestView not found in scene!");
            }
            else if (questView != null)
            {
                Debug.Log("✅ [QUEST MANAGER] QuestView 자동 찾기 성공!");
            }
        }

        // PlayerDataManager 확인
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("❌ [QUEST MANAGER] PlayerDataManager.Instance가 null입니다!");
        }
        else if (PlayerDataManager.Instance.CurrentPlayer == null)
        {
            Debug.LogError("❌ [QUEST MANAGER] PlayerDataManager.Instance.CurrentPlayer가 null입니다!");
        }
        else
        {
            Debug.Log($"✅ [QUEST MANAGER] PlayerDataManager 확인 완료. 현재 감정: {PlayerDataManager.Instance.CurrentPlayer.Npc1Affection}");
        }

        // 퀘스트는 NPC와의 대화를 통해 받게 됩니다
        // 게임 시작 시 자동 퀘스트 생성하지 않음

        // 초기 UI 업데이트
        UpdateQuestView();

        if (showDebugMessages)
        {
            Debug.Log($"✅ [QUEST MANAGER] QuestManager initialized with {activeQuests.Count} active quests");
        }
    }

    void Update()
    {
        // PlayerData의 감정 변화를 감지하여 퀘스트 진행도 체크
        CheckQuestProgress();
    }

    // === 퀘스트 진행도 체크 (Presenter 비즈니스 로직) ===
    private void CheckQuestProgress()
    {
        if (PlayerDataManager.Instance?.CurrentPlayer == null)
            return;

        LoveModel.Emotion currentEmotion = PlayerDataManager.Instance.CurrentPlayer.Npc1Affection;

        // 감정이 변경되었을 때만 퀘스트 체크
        if (lastCheckedEmotion != currentEmotion)
        {
            if (showDebugMessages)
            {
                Debug.Log($"📊 [QUEST SYSTEM] NPC 감정 변화 감지: {lastCheckedEmotion} → {currentEmotion}");
            }

            lastCheckedEmotion = currentEmotion;

            // 모든 활성 퀘스트에 대해 완료 조건 체크
            for (int i = activeQuests.Count - 1; i >= 0; i--)
            {
                QuestData quest = activeQuests[i];

                if (!quest.IsCompleted)
                {
                    if (showDebugMessages)
                    {
                        Debug.Log($"🔍 [QUEST CHECK] '{quest.Title}' - 목표: {quest.TargetEmotion}, 현재: {currentEmotion}");
                    }

                    if (quest.CheckCompletion(currentEmotion))
                    {
                        OnQuestCompleted(quest);
                    }
                }
            }

            // UI 업데이트
            UpdateQuestView();
        }
    }

    // === 퀘스트 완료 처리 ===
    private void OnQuestCompleted(QuestData completedQuest)
    {
        if (showDebugMessages)
        {
            Debug.Log($"🎉 [QUEST SUCCESS] 퀘스트 완료! 제목: {completedQuest.Title} | 목표: {completedQuest.TargetEmotion} | ID: {completedQuest.QuestId}");
        }

        // View에 완료 메시지 표시
        if (questView != null)
        {
            questView.ShowQuestCompletion(completedQuest.Title);
        }
    }

    // === View 업데이트 (Presenter → View) ===
    private void UpdateQuestView()
    {
        if (questView != null)
        {
            // 첫 번째 미완료 퀘스트 찾기
            QuestData currentQuest = GetCurrentActiveQuest();

            if (currentQuest != null)
            {
                // 미완료 퀘스트가 있으면 표시
                LoveModel.Emotion currentEmotion = PlayerDataManager.Instance?.CurrentPlayer?.Npc1Affection ?? LoveModel.Emotion.None;
                questView.UpdateQuestDisplay(currentQuest, currentEmotion);

                if (showDebugMessages)
                {
                    Debug.Log($"📋 [QUEST VIEW] 퀘스트 표시: {currentQuest.Title}");
                }
            }
            else
            {
                // 미완료 퀘스트가 없으면 퀘스트 패널 숨기기
                questView.HideQuestDisplay();

                if (showDebugMessages)
                {
                    Debug.Log($"📋 [QUEST VIEW] 모든 퀘스트 완료 - 퀘스트 패널 숨김");
                }
            }
        }
    }

    // === 공개 메서드 ===
    public void AddQuest(QuestData newQuest)
    {
        activeQuests.Add(newQuest);
        UpdateQuestView();

        if (showDebugMessages)
        {
            Debug.Log($"Quest Added: {newQuest.Title}");
        }
    }

    public QuestData GetCurrentActiveQuest()
    {
        foreach (QuestData quest in activeQuests)
        {
            if (!quest.IsCompleted)
                return quest;
        }
        return null;
    }

    public List<QuestData> GetActiveQuests()
    {
        return new List<QuestData>(activeQuests);
    }

    // === 테스트용 메서드 ===
    private void AddTestQuest()
    {
        QuestData testQuest = new QuestData(
            1,
            "첫 번째 퀘스트",
            "NPC의 호감도를 Happy로 만들어보세요!",
            LoveModel.Emotion.Happy
        );

        AddQuest(testQuest);
    }

    // === Inspector 디버그 메서드 ===
    [ContextMenu("Add Test Quest - Happy")]
    private void AddTestQuestHappy()
    {
        QuestData quest = new QuestData(activeQuests.Count + 1, "행복하게 만들기", "NPC를 Happy 상태로 만들어주세요!", LoveModel.Emotion.Happy);
        AddQuest(quest);
    }

    [ContextMenu("Add Test Quest - Love")]
    private void AddTestQuestLove()
    {
        QuestData quest = new QuestData(activeQuests.Count + 1, "사랑에 빠뜨리기", "NPC를 Love 상태로 만들어주세요!", LoveModel.Emotion.Love);
        AddQuest(quest);
    }

    [ContextMenu("Clear All Quests")]
    private void ClearAllQuests()
    {
        activeQuests.Clear();
        UpdateQuestView();
    }
}