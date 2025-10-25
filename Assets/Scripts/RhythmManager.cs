using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 리듬 게임의 오케스트레이션(주입/모드전환/점수반영/뷰갱신)을 담당하는 매니저.
/// - IMusicCore(예: SoundManager) → BeatJudgeSystem에 주입
/// - PlayerController의 입력 맵 전환(Level/Battle)
/// - BeatJudgeSystem의 판정 이벤트 수신 → RhythmData 점수/콤보 갱신 → RhythmView 반영
/// - 판정 윈도우/차트 로딩을 단일 진입점으로 제공
/// </summary>
public class RhythmManager : MonoBehaviour
{
    [Header("Model / View")]
    [SerializeField] private RhythmData rhythmData;   // 점수, 콤보, 판정윈도우 등 단일 소스
    [SerializeField] private RhythmView rhythmView;   // 점수/콤보/BPM UI, 바/판정선 표시

    [Header("Controllers")]
    [SerializeField] private BeatJudgeSystem judge;        // 판정기
    [SerializeField] private PlayerController player;      // 입력 주체(맵 전환)
    [Tooltip("IMusicCore 구현체를 드래그(예: SoundManager)")]
    [SerializeField] private MonoBehaviour musicClockBehaviour; // IMusicCore

    [Header("Auto Start (테스트용)")]
    [SerializeField] private bool autoStartBattle = false;
    [SerializeField] private float startDelaySec = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    // ─────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Null 자동 탐색(선택)
        if (rhythmView == null) rhythmView = FindFirstObjectByType<RhythmView>();
        if (judge == null)      judge      = FindFirstObjectByType<BeatJudgeSystem>();
        if (player == null)     player     = FindFirstObjectByType<PlayerController>();

        // Judge에 클럭(IMusicCore) 주입
        if (judge != null && musicClockBehaviour != null)
            judge.musicClockBehaviour = musicClockBehaviour;

        // Player → Judge 연결 보장
        if (player != null) player.judge = judge;

        // Judge 이벤트 구독
        if (judge != null)
        {
            judge.OnBeat += HandleBeat;
            judge.OnHit  += HandleHit;
        }

        // 초기 View 세팅
        if (rhythmView != null)
        {
            rhythmView.SetJudgmentLinePosition(rhythmData.JudgmentLinePosition, rhythmData.TrackWidth);
            UpdateRhythmView();
        }
    }

    private void OnDestroy()
    {
        if (judge != null)
        {
            judge.OnBeat -= HandleBeat;
            judge.OnHit  -= HandleHit;
        }
    }

    private void Start()
    {
        if (autoStartBattle)
            StartCoroutine(CoAutoStart());
    }

    private IEnumerator CoAutoStart()
    {
        yield return new WaitForSeconds(startDelaySec);
        StartBattleMode(); // 차트는 외부에서 LoadChart로 넣거나, 사전에 judge.notes에 세팅
    }

    // ─────────────────────────────────────────────────────────────
    // Public Orchestration API
    // ─────────────────────────────────────────────────────────────

    /// <summary>RhythmData의 판정 윈도우를 Judge에 동기화</summary>
    public void ApplyWindowsFromData()
    {
        if (judge == null || rhythmData == null) return;
        judge.perfect = rhythmData.PerfectWindow;
        judge.great   = rhythmData.GreatWindow;
        judge.good    = rhythmData.GoodWindow;
    }

    /// <summary>외부에서 빌드한 차트(List&lt;NoteData&gt;)를 주입</summary>
    public void LoadChart(List<NoteData> notes)
    {
        if (judge == null || notes == null) return;
        judge.notes = notes;
    }

    /// <summary>배틀(리듬) 모드를 시작. 차트가 있으면 함께 주입</summary>
    public void StartBattleMode(List<NoteData> notes = null)
    {
        rhythmData.StartGame();
        ApplyWindowsFromData();
        if (notes != null) LoadChart(notes);

        // 입력맵 전환
        if (player != null) player.SwitchToBattle();

        // UI 표시
        if (rhythmView != null)
        {
            rhythmView.ShowRhythmGame();
            rhythmView.SetJudgmentLinePosition(rhythmData.JudgmentLinePosition, rhythmData.TrackWidth);
        }

        if (showDebug) Debug.Log($"[RhythmManager] Battle Start - BPM:{rhythmData.BPM}");
        UpdateRhythmView();
    }

    /// <summary>배틀(리듬) 모드를 종료</summary>
    public void StopBattleMode()
    {
        rhythmData.StopGame();

        if (player != null) player.SwitchToLevel();

        if (rhythmView != null)
            rhythmView.HideRhythmGame();

        if (showDebug) Debug.Log($"[RhythmManager] Battle Stop - Score:{rhythmData.CurrentScore} MaxCombo:{rhythmData.MaxCombo}");
        UpdateRhythmView();
    }

    /// <summary>BPM 변경(뷰 갱신 포함)</summary>
    public void SetBPM(float bpm)
    {
        rhythmData.BPM = bpm;
        UpdateRhythmView();
        if (showDebug) Debug.Log($"[RhythmManager] BPM -> {bpm}");
    }

    /// <summary>판정창(초) 변경 후 Judge에 즉시 반영</summary>
    public void SetJudgeWindows(float perfect, float great, float good)
    {
        rhythmData.PerfectWindow = perfect;
        rhythmData.GreatWindow   = great;
        rhythmData.GoodWindow    = good;
        ApplyWindowsFromData();
        if (showDebug) Debug.Log($"[RhythmManager] Windows -> P:{perfect:F3} G:{great:F3} D:{good:F3}");
    }

    // ─────────────────────────────────────────────────────────────
    // Judge Events
    // ─────────────────────────────────────────────────────────────

    private void HandleBeat(int beatIndex)
    {
        // 필요 시 비트 하이라이트, 이펙트 트리거 등
        // rhythmView.HighlightBeat(beatIndex); 같은 형태로 확장 가능
        if (showDebug) Debug.Log($"[RhythmManager] Beat {beatIndex}");
    }

    private void HandleHit(HitEvent e)
    {
        // 점수/콤보 처리 (Miss는 콤보 리셋)
        if (e.grade == HitAccuracy.Miss)
        {
            rhythmData.Combo = 0;
        }
        else
        {
            rhythmData.Combo += 1;
            rhythmData.CurrentScore += rhythmData.GetScoreForAccuracy(e.grade);
        }

        // UI 피드백
        if (rhythmView != null)
            rhythmView.ShowHitFeedback(e.grade);

        UpdateRhythmView();

        if (showDebug)
            Debug.Log($"[RhythmManager] Hit {e.grade} Δ={e.deltaMs:F1}ms  Score:{rhythmData.CurrentScore} Combo:{rhythmData.Combo}");
    }

    // ─────────────────────────────────────────────────────────────
    // View Update
    // ─────────────────────────────────────────────────────────────

    private void UpdateRhythmView()
    {
        if (rhythmView == null) return;
        rhythmView.UpdateScoreDisplay(
            rhythmData.CurrentScore,
            rhythmData.Combo,
            rhythmData.BPM
        );
    }

    // ─────────────────────────────────────────────────────────────
    // Inspector Helpers (테스트용)
    // ─────────────────────────────────────────────────────────────

    [ContextMenu("Start Battle Mode")]
    private void _TestStart() => StartBattleMode();

    [ContextMenu("Stop Battle Mode")]
    private void _TestStop()  => StopBattleMode();

    [ContextMenu("Set BPM 120")]
    private void _TestBpm120() => SetBPM(120f);

    [ContextMenu("Set Judge Windows (0.05/0.10/0.15)")]
    private void _TestWindows() => SetJudgeWindows(0.05f, 0.10f, 0.15f);
}
