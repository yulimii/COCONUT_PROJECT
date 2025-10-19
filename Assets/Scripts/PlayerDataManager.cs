using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private PlayerData currentPlayerData;

    [Header("Initial Settings")]
    [SerializeField] private int initialHealth = 5;
    [SerializeField] private int initialMaxHealth = 5;

    // 싱글톤 패턴
    public static PlayerDataManager Instance { get; private set; }

    // 다른 스크립트에서 접근할 수 있는 프로퍼티
    public PlayerData CurrentPlayer => currentPlayerData;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            InitializePlayerData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // === 초기화 ===
    private void InitializePlayerData()
    {
        if (currentPlayerData == null)
        {
            currentPlayerData = new PlayerData(initialHealth, initialMaxHealth);
        }

        Debug.Log($"PlayerDataManager initialized with {initialHealth}/{initialMaxHealth} health: " + currentPlayerData.ToString());
    }

    // === PlayerData는 순수 데이터 저장소 역할만 ===
    // 체력 관련 비즈니스 로직은 HealthManager에서 처리

    // === 디버그/테스트 메서드 ===
    [ContextMenu("Reset Data")]
    public void ResetPlayerData()
    {
        currentPlayerData.ResetToDefault();
        Debug.Log("Player data reset to default values.");
    }

    [ContextMenu("Print Data")]
    public void PrintPlayerData()
    {
        Debug.Log($"Current Player Data: {currentPlayerData?.ToString() ?? "NULL"}");
    }
}