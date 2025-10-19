using UnityEngine;
using System.Collections;

public class RhythmManager : MonoBehaviour
{
    [Header("Rhythm Settings")]
    [SerializeField] private RhythmData rhythmData;
    [SerializeField] private bool showDebugMessages = true;

    [Header("UI Reference")]
    [SerializeField] private RhythmView rhythmView;

    [Header("Input Settings")]
    [SerializeField] private KeyCode rhythmInputKey = KeyCode.Space;
    [SerializeField] private bool allowMultipleInputs = false;

    [Header("Game Control")]
    [SerializeField] private bool autoStartGame = false;
    [SerializeField] private float gameStartDelay = 1f;

    private bool hasInputThisBeat = false;
    private int lastProcessedBeat = -1;

    // === 초기화 ===
    void Start()
    {
        Debug.Log("🎵 [RHYTHM MANAGER] RhythmManager Start() 호출됨!");

        // RhythmData 초기화
        if (rhythmData == null)
        {
            rhythmData = new RhythmData();
            if (showDebugMessages)
            {
                Debug.Log("🎵 [RHYTHM MANAGER] RhythmData 자동 생성");
            }
        }

        // RhythmView 자동 찾기
        if (rhythmView == null)
        {
            rhythmView = FindFirstObjectByType<RhythmView>();
            if (rhythmView == null && showDebugMessages)
            {
                Debug.LogWarning("⚠️ [RHYTHM MANAGER] RhythmView not found in scene!");
            }
            else if (rhythmView != null)
            {
                Debug.Log("✅ [RHYTHM MANAGER] RhythmView 자동 찾기 성공!");
            }
        }

        // 초기 UI 업데이트
        UpdateRhythmView();

        // 자동 시작 설정
        if (autoStartGame)
        {
            StartCoroutine(AutoStartGameCoroutine());
        }

        if (showDebugMessages)
        {
            Debug.Log($"✅ [RHYTHM MANAGER] 초기화 완료 - BPM: {rhythmData.BPM}");
        }
    }

    void Update()
    {
        if (rhythmData.IsPlaying)
        {
            // 바 위치 업데이트
            UpdateBarPosition();

            // 새 비트 감지 및 처리
            HandleBeatDetection();

            // 입력 처리
            HandleRhythmInput();

            // UI 업데이트
            UpdateRhythmView();
        }
    }

    // === 게임 시작/종료 ===
    public void StartRhythmGame()
    {
        rhythmData.StartGame();
        hasInputThisBeat = false;
        lastProcessedBeat = -1;

        if (rhythmView != null)
        {
            rhythmView.ShowRhythmGame();
            rhythmView.SetJudgmentLinePosition(rhythmData.JudgmentLinePosition, rhythmData.TrackWidth);
        }

        if (showDebugMessages)
        {
            Debug.Log($"🎵 [RHYTHM MANAGER] 리듬 게임 시작! BPM: {rhythmData.BPM}");
        }
    }

    public void StopRhythmGame()
    {
        rhythmData.StopGame();

        if (rhythmView != null)
        {
            rhythmView.HideRhythmGame();
        }

        if (showDebugMessages)
        {
            Debug.Log($"🎵 [RHYTHM MANAGER] 리듬 게임 종료 - 최종 점수: {rhythmData.CurrentScore}, 최대 콤보: {rhythmData.MaxCombo}");
        }
    }

    // === 바 위치 업데이트 ===
    private void UpdateBarPosition()
    {
        float currentBarPosition = rhythmData.GetCurrentBarPosition();

        if (rhythmView != null)
        {
            rhythmView.UpdateBarPosition(currentBarPosition, rhythmData.TrackWidth);
        }
    }

    // === 비트 감지 및 처리 ===
    private void HandleBeatDetection()
    {
        int currentBeat = rhythmData.CurrentBeatNumber;

        // 새로운 비트가 시작되었는지 확인
        if (currentBeat != lastProcessedBeat)
        {
            OnNewBeat(currentBeat);
            lastProcessedBeat = currentBeat;
            hasInputThisBeat = false; // 새 비트에서 입력 초기화

            if (showDebugMessages)
            {
                Debug.Log($"🥁 [RHYTHM MANAGER] 새 비트: {currentBeat} | 시간: {rhythmData.CurrentGameTime:F2}s");
            }
        }
    }

    // === 새 비트 처리 ===
    private void OnNewBeat(int beatNumber)
    {
        // 이전 비트에서 입력이 없었다면 Miss 처리
        if (beatNumber > 0 && !hasInputThisBeat && !allowMultipleInputs)
        {
            ProcessMiss();
        }
    }

    // === 리듬 입력 처리 ===
    private void HandleRhythmInput()
    {
        if (Input.GetKeyDown(rhythmInputKey))
        {
            // 비트당 한 번만 입력 허용 (설정에 따라)
            if (!allowMultipleInputs && hasInputThisBeat)
            {
                if (showDebugMessages)
                {
                    Debug.Log($"⚠️ [RHYTHM INPUT] 이미 이 비트에서 입력했습니다");
                }
                return;
            }

            ProcessRhythmInput();
            hasInputThisBeat = true;
        }
    }

    // === 리듬 입력 처리 ===
    private void ProcessRhythmInput()
    {
        float currentTime = rhythmData.CurrentGameTime;
        float currentBeatTime = rhythmData.CurrentBeatTime;

        // 비트의 정확한 타이밍 계산 (비트 시작점 기준)
        float beatStartTime = rhythmData.CurrentBeatNumber * rhythmData.BeatDuration;
        float timeDifferenceFromBeatStart = Mathf.Abs(currentBeatTime);

        // 비트 끝 근처에서의 입력도 고려 (다음 비트 시작점 기준)
        float timeDifferenceFromBeatEnd = Mathf.Abs(rhythmData.BeatDuration - currentBeatTime);
        float actualTimeDifference = Mathf.Min(timeDifferenceFromBeatStart, timeDifferenceFromBeatEnd);

        // 판정 계산
        HitAccuracy accuracy = GetAccuracyFromTimeDifference(actualTimeDifference);

        // 점수 및 콤보 처리
        ProcessHitResult(accuracy, actualTimeDifference);

        if (showDebugMessages)
        {
            Debug.Log($"🎯 [RHYTHM INPUT] 입력! 시간차: {actualTimeDifference:F3}s, 판정: {accuracy}, 점수: +{rhythmData.GetScoreForAccuracy(accuracy)}");
        }
    }

    // === 타이밍 차이로 판정 계산 ===
    private HitAccuracy GetAccuracyFromTimeDifference(float timeDifference)
    {
        if (timeDifference <= rhythmData.PerfectWindow)
            return HitAccuracy.Perfect;
        else if (timeDifference <= rhythmData.GreatWindow)
            return HitAccuracy.Great;
        else if (timeDifference <= rhythmData.GoodWindow)
            return HitAccuracy.Good;
        else
            return HitAccuracy.Miss;
    }

    // === 타격 결과 처리 ===
    private void ProcessHitResult(HitAccuracy accuracy, float timeDifference)
    {
        if (accuracy != HitAccuracy.Miss)
        {
            // 성공한 타격
            rhythmData.Combo++;
            int scoreGain = rhythmData.GetScoreForAccuracy(accuracy);
            rhythmData.CurrentScore += scoreGain;
        }
        else
        {
            // 실패한 타격
            rhythmData.Combo = 0;
        }

        // UI 피드백 표시
        if (rhythmView != null)
        {
            rhythmView.ShowHitFeedback(accuracy);
        }
    }

    // === Miss 처리 ===
    private void ProcessMiss()
    {
        rhythmData.Combo = 0;

        if (rhythmView != null)
        {
            rhythmView.ShowHitFeedback(HitAccuracy.Miss);
        }

        if (showDebugMessages)
        {
            Debug.Log($"❌ [RHYTHM MANAGER] Miss! 콤보 리셋");
        }
    }

    // === View 업데이트 ===
    private void UpdateRhythmView()
    {
        if (rhythmView != null)
        {
            rhythmView.UpdateScoreDisplay(
                rhythmData.CurrentScore,
                rhythmData.Combo,
                rhythmData.BPM
            );
        }
    }

    // === 자동 시작 코루틴 ===
    private IEnumerator AutoStartGameCoroutine()
    {
        yield return new WaitForSeconds(gameStartDelay);
        StartRhythmGame();
    }

    // === 공개 메서드 ===
    public void SetBPM(float newBPM)
    {
        rhythmData.BPM = newBPM;
        if (showDebugMessages)
        {
            Debug.Log($"🎵 [RHYTHM MANAGER] BPM 변경: {newBPM}");
        }
    }

    public RhythmData GetRhythmData()
    {
        return rhythmData;
    }

    public bool IsGameActive()
    {
        return rhythmData.IsPlaying;
    }

    // === Inspector 디버그 메서드 ===
    [ContextMenu("Start Rhythm Game")]
    private void DebugStartGame()
    {
        StartRhythmGame();
    }

    [ContextMenu("Stop Rhythm Game")]
    private void DebugStopGame()
    {
        StopRhythmGame();
    }

    [ContextMenu("Set BPM to 60")]
    private void DebugSetBPM60()
    {
        SetBPM(60f);
    }

    [ContextMenu("Set BPM to 120")]
    private void DebugSetBPM120()
    {
        SetBPM(120f);
    }

    [ContextMenu("Set BPM to 180")]
    private void DebugSetBPM180()
    {
        SetBPM(180f);
    }

    [ContextMenu("Reset Game")]
    private void DebugResetGame()
    {
        rhythmData.ResetGame();
        UpdateRhythmView();
    }
}