using UnityEngine;
using UnityEngine.UI;

public class RhythmView : MonoBehaviour
{
    [Header("Track UI Elements")]
    [SerializeField] private GameObject trackPanel;
    [SerializeField] private Image trackBackground;
    [SerializeField] private RectTransform movingBar;
    [SerializeField] private Image judgmentLine;

    [Header("Score UI Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text comboText;
    [SerializeField] private Text bpmText;

    [Header("Hit Feedback UI")]
    [SerializeField] private GameObject hitFeedbackPanel;
    [SerializeField] private Text hitAccuracyText;
    [SerializeField] private float feedbackDisplayTime = 1f;

    [Header("Visual Settings")]
    [SerializeField] private Color perfectColor = Color.yellow;
    [SerializeField] private Color greatColor = Color.green;
    [SerializeField] private Color goodColor = Color.blue;
    [SerializeField] private Color missColor = Color.red;
    [SerializeField] private Color barColor = Color.white;
    [SerializeField] private Color judgmentLineColor = Color.red;

    [Header("Animation Settings")]
    [SerializeField] private bool smoothBarMovement = true;
    [SerializeField] private float barHeight = 20f;
    [SerializeField] private float judgmentLineWidth = 5f;

    private float feedbackTimer = 0f;
    private bool isFeedbackShowing = false;

    // === 초기화 ===
    void Start()
    {
        InitializeUIElements();
        SetupTrackAppearance();

        // 초기 상태 설정
        if (trackPanel != null)
            trackPanel.SetActive(false);

        if (hitFeedbackPanel != null)
            hitFeedbackPanel.SetActive(false);
    }

    void Update()
    {
        // 피드백 타이머 처리
        HandleFeedbackTimer();
    }

    // === UI 요소 자동 찾기 ===
    private void InitializeUIElements()
    {
        // trackPanel이 없으면 자식에서 찾기
        if (trackPanel == null)
        {
            Transform trackTransform = transform.Find("TrackPanel");
            if (trackTransform != null)
                trackPanel = trackTransform.gameObject;
        }

        // Track UI 요소들 자동 찾기
        if (trackPanel != null)
        {
            if (trackBackground == null)
            {
                Transform bgTransform = trackPanel.transform.Find("TrackBackground");
                if (bgTransform != null)
                    trackBackground = bgTransform.GetComponent<Image>();
            }

            if (movingBar == null)
            {
                Transform barTransform = trackPanel.transform.Find("MovingBar");
                if (barTransform != null)
                    movingBar = barTransform.GetComponent<RectTransform>();
            }

            if (judgmentLine == null)
            {
                Transform lineTransform = trackPanel.transform.Find("JudgmentLine");
                if (lineTransform != null)
                    judgmentLine = lineTransform.GetComponent<Image>();
            }
        }

        // Score UI 요소들 자동 찾기
        if (scoreText == null)
        {
            Transform scoreTransform = transform.Find("ScoreText");
            if (scoreTransform != null)
                scoreText = scoreTransform.GetComponent<Text>();
        }

        if (comboText == null)
        {
            Transform comboTransform = transform.Find("ComboText");
            if (comboTransform != null)
                comboText = comboTransform.GetComponent<Text>();
        }

        if (bpmText == null)
        {
            Transform bpmTransform = transform.Find("BPMText");
            if (bpmTransform != null)
                bpmText = bpmTransform.GetComponent<Text>();
        }

        // Hit Feedback UI 자동 찾기
        if (hitFeedbackPanel == null)
        {
            Transform feedbackTransform = transform.Find("HitFeedbackPanel");
            if (feedbackTransform != null)
            {
                hitFeedbackPanel = feedbackTransform.gameObject;
                if (hitAccuracyText == null)
                {
                    hitAccuracyText = hitFeedbackPanel.GetComponentInChildren<Text>();
                }
            }
        }
    }

    // === 외관 설정 ===
    private void SetupTrackAppearance()
    {
        // 바 색상 설정
        if (movingBar != null)
        {
            Image barImage = movingBar.GetComponent<Image>();
            if (barImage != null)
            {
                barImage.color = barColor;
            }
        }

        // 판정선 색상 설정
        if (judgmentLine != null)
        {
            judgmentLine.color = judgmentLineColor;
        }
    }

    // === 게임 표시/숨김 ===
    public void ShowRhythmGame()
    {
        if (trackPanel != null)
            trackPanel.SetActive(true);
    }

    public void HideRhythmGame()
    {
        if (trackPanel != null)
            trackPanel.SetActive(false);

        if (hitFeedbackPanel != null)
            hitFeedbackPanel.SetActive(false);
    }

    // === 바 위치 업데이트 (Presenter에서 호출) ===
    public void UpdateBarPosition(float xPosition, float trackWidth)
    {
        if (movingBar != null && trackPanel != null)
        {
            RectTransform trackRect = trackPanel.GetComponent<RectTransform>();
            if (trackRect != null)
            {
                // 바의 위치를 트랙 내에서 상대적으로 계산
                float normalizedPosition = xPosition / trackWidth;
                float trackPixelWidth = trackRect.rect.width;
                float targetX = (normalizedPosition - 0.5f) * trackPixelWidth;

                Vector3 barPosition = movingBar.localPosition;
                barPosition.x = targetX;
                movingBar.localPosition = barPosition;
            }
        }
    }

    // === 판정선 위치 설정 ===
    public void SetJudgmentLinePosition(float position, float trackWidth)
    {
        if (judgmentLine != null && trackPanel != null)
        {
            RectTransform trackRect = trackPanel.GetComponent<RectTransform>();
            if (trackRect != null)
            {
                float normalizedPosition = position / trackWidth;
                float trackPixelWidth = trackRect.rect.width;
                float targetX = (normalizedPosition - 0.5f) * trackPixelWidth;

                Vector3 linePosition = judgmentLine.rectTransform.localPosition;
                linePosition.x = targetX;
                judgmentLine.rectTransform.localPosition = linePosition;
            }
        }
    }

    // === 점수 UI 업데이트 ===
    public void UpdateScoreDisplay(int score, int combo, float bpm)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score:N0}";

        if (comboText != null)
        {
            if (combo > 0)
            {
                comboText.text = $"Combo: {combo}";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }

        if (bpmText != null)
            bpmText.text = $"BPM: {bpm:F0}";
    }

    // === 타격 피드백 표시 ===
    public void ShowHitFeedback(HitAccuracy accuracy)
    {
        if (hitFeedbackPanel != null && hitAccuracyText != null)
        {
            // 피드백 텍스트 설정
            string feedbackText = accuracy switch
            {
                HitAccuracy.Perfect => "PERFECT!",
                HitAccuracy.Great => "GREAT!",
                HitAccuracy.Good => "GOOD",
                HitAccuracy.Miss => "MISS",
                _ => ""
            };

            // 피드백 색상 설정
            Color feedbackColor = accuracy switch
            {
                HitAccuracy.Perfect => perfectColor,
                HitAccuracy.Great => greatColor,
                HitAccuracy.Good => goodColor,
                HitAccuracy.Miss => missColor,
                _ => Color.white
            };

            hitAccuracyText.text = feedbackText;
            hitAccuracyText.color = feedbackColor;

            // 피드백 패널 활성화
            hitFeedbackPanel.SetActive(true);
            feedbackTimer = feedbackDisplayTime;
            isFeedbackShowing = true;
        }
    }

    // === 피드백 타이머 처리 ===
    private void HandleFeedbackTimer()
    {
        if (isFeedbackShowing && feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;

            if (feedbackTimer <= 0f)
            {
                if (hitFeedbackPanel != null)
                    hitFeedbackPanel.SetActive(false);

                isFeedbackShowing = false;
            }
        }
    }

    // === 공개 프로퍼티 ===
    public bool IsRhythmGameVisible => trackPanel != null && trackPanel.activeSelf;

    // === Inspector 디버그 메서드 ===
    [ContextMenu("Test Show Rhythm Game")]
    private void TestShowRhythmGame()
    {
        ShowRhythmGame();
        UpdateBarPosition(200f, 800f);
        UpdateScoreDisplay(1500, 10, 120f);
    }

    [ContextMenu("Test Show Perfect Hit")]
    private void TestShowPerfectHit()
    {
        ShowHitFeedback(HitAccuracy.Perfect);
    }

    [ContextMenu("Test Show Miss Hit")]
    private void TestShowMissHit()
    {
        ShowHitFeedback(HitAccuracy.Miss);
    }

    [ContextMenu("Hide Rhythm Game")]
    private void TestHideRhythmGame()
    {
        HideRhythmGame();
    }
}