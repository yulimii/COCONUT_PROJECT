using UnityEngine;

public class BubbleManager : MonoBehaviour
{
    public GameObject Bubble;
    public Animator BubbleAnimator;

    private LoveModel loveModel;
    private LoveModel.Emotion currentEmotion;
    private LoveModel.Emotion lastPlayerDataEmotion;

    // LoveModel에 대한 공개 프로퍼티 (다른 스크립트에서 접근 가능)
    public LoveModel Model => loveModel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Bubble = transform.GetChild(0).gameObject;
        BubbleAnimator = Bubble.GetComponent<Animator>();
        currentEmotion = LoveModel.Emotion.None;

        // LoveModel 인스턴스 생성 및 이벤트 구독
        loveModel = GetComponent<LoveModel>();
        if (loveModel != null)
        {
            loveModel.EmotionChanged += OnEmotionChanged;
        }

        // PlayerData의 초기 호감도 읽어오기
        UpdateEmotionFromPlayerData();

        Bubble.SetActive(false);
    }

    void Update()
    {
        // PlayerData의 npc1Affection 변화를 감지하여 자동 업데이트
        UpdateEmotionFromPlayerData();
    }

    void OnDestroy()
    {
        // 메모리 누수 방지를 위해 이벤트 구독 해제
        if (loveModel != null)
        {
            loveModel.EmotionChanged -= OnEmotionChanged;
        }
    }

    // PlayerData의 npc1Affection 값을 읽어와서 감정 업데이트
    private void UpdateEmotionFromPlayerData()
    {
        if (PlayerDataManager.Instance?.CurrentPlayer != null)
        {
            LoveModel.Emotion playerDataEmotion = PlayerDataManager.Instance.CurrentPlayer.Npc1Affection;

            // 값이 변경되었을 때만 업데이트
            if (lastPlayerDataEmotion != playerDataEmotion)
            {
                lastPlayerDataEmotion = playerDataEmotion;

                // LoveModel을 통해 감정 변경 (이벤트 시스템 유지)
                if (loveModel != null)
                {
                    loveModel.CurrentEmotion = playerDataEmotion;
                }

                Debug.Log($"NPC emotion updated from PlayerData: {playerDataEmotion}");
            }
        }
    }

    // LoveModel의 emotion이 변경될 때 호출되는 메서드
    private void OnEmotionChanged(LoveModel.Emotion newEmotion)
    {
        currentEmotion = newEmotion;
        BubbleAnimator.SetInteger("Emotion", (int)currentEmotion);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Bubble.SetActive(true);

            // Bubble이 활성화될 때 현재 감정 상태를 Animator에 다시 설정
            BubbleAnimator.SetInteger("Emotion", (int)currentEmotion);

            Debug.Log($"[BubbleManager] Player entered bubble area - {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            Bubble.SetActive(false);
            Debug.Log($"[BubbleManager] Player left bubble area - {gameObject.name}");
        }
    }
}
