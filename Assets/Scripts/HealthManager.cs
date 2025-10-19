using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private HealthView healthView;

    [Header("Debug - Test Controls")]
    [SerializeField] private bool enableTestControls = true;

    // PlayerData 참조 (Model)
    private PlayerData playerData => PlayerDataManager.Instance?.CurrentPlayer;

    private void Awake()
    {
        // 컴포넌트 자동 할당
        if (healthView == null)
            healthView = GetComponent<HealthView>();

        // 필수 컴포넌트 확인
        if (healthView == null)
        {
            Debug.LogError("HealthView component not found! Please add HealthView to this GameObject.");
        }
    }

    private void Start()
    {
        // 초기 UI 업데이트 (MVP: Presenter가 직접 View 업데이트)
        UpdateView();

        Debug.Log("HealthManager (Presenter) initialized");
    }

    // MVP 패턴: Presenter가 View를 직접 업데이트
    private void UpdateView()
    {
        if (playerData != null && healthView != null)
        {
            healthView.UpdateHealthDisplay(playerData.CurrentHealth, playerData.MaxHealth);
        }
    }

    // Update에서 테스트 키 처리
    private void Update()
    {
        if (!enableTestControls || playerData == null) return;

        HandleTestInput();
    }

    // 테스트용 키보드 입력 처리
    private void HandleTestInput()
    {
        // Q키: 데미지 받기 (1)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TakeDamage(1);
        }

        // W키: 회복하기 (1)
        if (Input.GetKeyDown(KeyCode.W))
        {
            Heal(1);
        }

        // E키: 최대 체력 증가 (1) + 체력 1 회복
        if (Input.GetKeyDown(KeyCode.E))
        {
            IncreaseMaxHealth(1);
        }

        // Y키: 최대 체력 감소 (1)
        if (Input.GetKeyDown(KeyCode.Y))
        {
            DecreaseMaxHealth(1);
        }

        // R키: 완전 회복
        if (Input.GetKeyDown(KeyCode.R))
        {
            FullHeal();
        }

        // T키: 체력 1로 설정 (거의 죽음)
        if (Input.GetKeyDown(KeyCode.T))
        {
            SetHealth(1);
        }
    }

    // === 비즈니스 로직 메서드들 (Presenter 역할) ===

    /// <summary>
    /// 플레이어가 데미지를 받을 때 호출 (MVP: Presenter가 비즈니스 로직 처리)
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (damage <= 0 || playerData == null) return;

        // 1. 비즈니스 로직 처리
        int newHealth = Mathf.Clamp(playerData.CurrentHealth - damage, 0, playerData.MaxHealth);

        // 2. Model 업데이트
        playerData.CurrentHealth = newHealth;

        // 3. View 업데이트 (수동으로!)
        UpdateView();

        // 4. 로그
        Debug.Log($"Took {damage} damage. Current Health: {playerData.CurrentHealth}/{playerData.MaxHealth}");

        if (playerData.CurrentHealth <= 0)
        {
            Debug.Log("Player died!");
        }
    }

    /// <summary>
    /// 플레이어가 회복할 때 호출
    /// </summary>
    public void Heal(int amount)
    {
        if (amount <= 0 || playerData == null) return;

        // 1. 비즈니스 로직
        int newHealth = Mathf.Clamp(playerData.CurrentHealth + amount, 0, playerData.MaxHealth);

        // 2. Model 업데이트
        playerData.CurrentHealth = newHealth;

        // 3. View 업데이트
        UpdateView();

        Debug.Log($"Healed {amount} HP. Current Health: {playerData.CurrentHealth}/{playerData.MaxHealth}");
    }

    /// <summary>
    /// 아이템으로 최대 체력을 증가시킬 때 호출 (체력도 1 회복됨)
    /// </summary>
    public void IncreaseMaxHealth(int amount)
    {
        if (amount <= 0 || playerData == null) return;

        // 1. 비즈니스 로직
        int newMaxHealth = playerData.MaxHealth + amount;
        int newCurrentHealth = Mathf.Clamp(playerData.CurrentHealth + 1, 0, newMaxHealth); // 체력도 1 회복

        // 2. Model 업데이트
        playerData.MaxHealth = newMaxHealth;
        playerData.CurrentHealth = newCurrentHealth;

        // 3. View 업데이트
        UpdateView();

        Debug.Log($"Max health increased by {amount}. New max health: {playerData.MaxHealth}. Healed 1 HP.");
    }

    /// <summary>
    /// 저주나 디버프로 최대 체력을 감소시킬 때 호출
    /// </summary>
    public void DecreaseMaxHealth(int amount)
    {
        if (amount <= 0 || playerData == null) return;

        // 1. 비즈니스 로직
        int newMaxHealth = Mathf.Max(1, playerData.MaxHealth - amount);
        int newCurrentHealth = Mathf.Clamp(playerData.CurrentHealth, 0, newMaxHealth);

        // 2. Model 업데이트
        playerData.MaxHealth = newMaxHealth;
        playerData.CurrentHealth = newCurrentHealth;

        // 3. View 업데이트
        UpdateView();

        Debug.Log($"Max health decreased by {amount}. New max health: {playerData.MaxHealth}");
    }

    /// <summary>
    /// 체력을 완전히 회복
    /// </summary>
    public void FullHeal()
    {
        if (playerData == null) return;

        // 1. 비즈니스 로직 & 2. Model 업데이트
        playerData.CurrentHealth = playerData.MaxHealth;

        // 3. View 업데이트
        UpdateView();

        Debug.Log($"Fully healed! Health: {playerData.CurrentHealth}/{playerData.MaxHealth}");
    }

    /// <summary>
    /// 현재 체력을 특정 값으로 설정
    /// </summary>
    public void SetHealth(int health)
    {
        if (playerData == null) return;

        // 1. 비즈니스 로직
        int newHealth = Mathf.Clamp(health, 0, playerData.MaxHealth);

        // 2. Model 업데이트
        playerData.CurrentHealth = newHealth;

        // 3. View 업데이트
        UpdateView();
    }

    // === 상태 확인 메서드들 ===
    public int GetCurrentHealth() => playerData?.CurrentHealth ?? 0;
    public int GetMaxHealth() => playerData?.MaxHealth ?? 0;
    public bool IsPlayerDead() => playerData?.CurrentHealth <= 0;
    public bool IsPlayerFullHealth() => playerData?.CurrentHealth >= playerData?.MaxHealth;
}