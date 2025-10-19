using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("Health Data")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;

    [Header("NPC Affection Data")]
    // 사용 가능한 호감도: Angry, Sad, Idle, Happy, Love (None, Max 제외)
    [SerializeField] private LoveModel.Emotion npc1Affection = LoveModel.Emotion.Idle;
    [SerializeField] private LoveModel.Emotion npc2Affection = LoveModel.Emotion.Idle;
    [SerializeField] private LoveModel.Emotion npc3Affection = LoveModel.Emotion.Idle;
    [SerializeField] private LoveModel.Emotion npc4Affection = LoveModel.Emotion.Idle;
    [SerializeField] private LoveModel.Emotion npc5Affection = LoveModel.Emotion.Idle;

    // === 순수 데이터 프로퍼티 (이벤트 없음) ===
    public int CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = value; // 단순 저장만
    }

    public int MaxHealth
    {
        get => maxHealth;
        set => maxHealth = value; // 단순 저장만
    }

    // === NPC 호감도 프로퍼티 (순수 데이터 저장만) ===
    public LoveModel.Emotion Npc1Affection
    {
        get => npc1Affection;
        set => npc1Affection = value;
    }

    public LoveModel.Emotion Npc2Affection
    {
        get => npc2Affection;
        set => npc2Affection = value;
    }

    public LoveModel.Emotion Npc3Affection
    {
        get => npc3Affection;
        set => npc3Affection = value;
    }

    public LoveModel.Emotion Npc4Affection
    {
        get => npc4Affection;
        set => npc4Affection = value;
    }

    public LoveModel.Emotion Npc5Affection
    {
        get => npc5Affection;
        set => npc5Affection = value;
    }

    // === 생성자 ===
    public PlayerData()
    {
        ResetToDefault();
    }

    public PlayerData(int startingCurrentHealth, int startingMaxHealth)
    {
        currentHealth = startingCurrentHealth;
        maxHealth = startingMaxHealth;
    }

    // === 데이터 관리 메서드 ===
    public void ResetToDefault()
    {
        maxHealth = 5;
        currentHealth = 5;

        // 모든 NPC 호감도를 기본값(Idle)으로 초기화
        npc1Affection = LoveModel.Emotion.Idle;
        npc2Affection = LoveModel.Emotion.Idle;
        npc3Affection = LoveModel.Emotion.Idle;
        npc4Affection = LoveModel.Emotion.Idle;
        npc5Affection = LoveModel.Emotion.Idle;
    }

    // === 순수 데이터 저장소 - 비즈니스 로직 없음 ===

    // 디버그용 정보 출력
    public override string ToString()
    {
        return $"Health: {currentHealth}/{maxHealth} | " +
               $"Affections: [{npc1Affection}, {npc2Affection}, {npc3Affection}, {npc4Affection}, {npc5Affection}]";
    }
}