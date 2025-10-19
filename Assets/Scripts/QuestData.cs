using UnityEngine;

public enum QuestType
{
    Emotion
}

[System.Serializable]
public class QuestData
{
    [Header("Quest Information")]
    [SerializeField] private int questId;
    [SerializeField] private string title;
    [TextArea(2, 3)]
    [SerializeField] private string description;
    [SerializeField] private QuestType type;

    [Header("Emotion Quest Settings")]
    [SerializeField] private LoveModel.Emotion targetEmotion;
    [SerializeField] private bool isCompleted;

    // === 순수 데이터 프로퍼티 (MVP 패턴의 Model) ===
    public int QuestId
    {
        get => questId;
        set => questId = value;
    }

    public string Title
    {
        get => title;
        set => title = value;
    }

    public string Description
    {
        get => description;
        set => description = value;
    }

    public QuestType Type
    {
        get => type;
        set => type = value;
    }

    public LoveModel.Emotion TargetEmotion
    {
        get => targetEmotion;
        set => targetEmotion = value;
    }

    public bool IsCompleted
    {
        get => isCompleted;
        set => isCompleted = value;
    }

    // === 생성자 ===
    public QuestData()
    {
        questId = 0;
        title = "기본 퀘스트";
        description = "퀘스트 설명";
        type = QuestType.Emotion;
        targetEmotion = LoveModel.Emotion.Happy;
        isCompleted = false;
    }

    public QuestData(int id, string questTitle, string questDescription, LoveModel.Emotion target)
    {
        questId = id;
        title = questTitle;
        description = questDescription;
        type = QuestType.Emotion;
        targetEmotion = target;
        isCompleted = false;
    }

    // === 퀘스트 상태 체크 메서드 ===
    public bool CheckCompletion(LoveModel.Emotion currentEmotion)
    {
        if (type == QuestType.Emotion && currentEmotion == targetEmotion)
        {
            isCompleted = true;
            return true;
        }
        return false;
    }

    // === 디버그용 ===
    public override string ToString()
    {
        string status = isCompleted ? "[완료]" : "[진행중]";
        return $"{status} {title}: {description} (목표: {targetEmotion})";
    }
}