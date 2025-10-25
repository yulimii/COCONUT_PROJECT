using UnityEngine;

[System.Serializable]
public enum HitAccuracy
{
    Miss,
    Good,
    Great,
    Perfect
}

// 레인(트랙) 식별자: A~D. UI/노트 차트와 동일 순서로 매핑
public enum LaneId { A = 0, B = 1, C = 2, D = 3 }

[System.Serializable]
public struct NoteData
{
    // 노트 목표 시각(절대 초, double). BPM→BeatIndex→초로 환산한 값
    public double timeSec;
    // 노트가 속한 레인
    public LaneId lane;
}

public struct HitEvent
{
    public NoteData note;        // 대상 노트 정보
    public HitAccuracy grade;    // 최종 판정 등급
    public float deltaMs;        // 입력시각-타겟시각(ms)
}

[System.Serializable]
public class RhythmData
{
    [Header("Rhythm Settings")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private bool isPlaying = false;
    [SerializeField] private float gameStartTime = 0f;

    [Header("Bar Movement")]
    [SerializeField] private float barSpeed = 5f;
    [SerializeField] private float trackWidth = 800f;
    [SerializeField] private float judgmentLinePosition = 400f; // 판정선 위치 (트랙 중앙)

    [Header("Hit Judgment Windows (초)")]
    [SerializeField] private float perfectWindow = 0.05f;   // ±50ms
    [SerializeField] private float greatWindow = 0.10f;     // ±100ms
    [SerializeField] private float goodWindow = 0.15f;      // ±150ms

    [Header("Current Game State")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int combo = 0;
    [SerializeField] private int maxCombo = 0;

    // === 순수 데이터 프로퍼티 (MVP 패턴의 Model) ===
    public float BPM
    {
        get => bpm;
        set => bpm = Mathf.Max(60f, value); // 최소 60 BPM
    }

    public bool IsPlaying
    {
        get => isPlaying;
        set => isPlaying = value;
    }

    public float GameStartTime
    {
        get => gameStartTime;
        set => gameStartTime = value;
    }

    public float BarSpeed
    {
        get => barSpeed;
        set => barSpeed = Mathf.Max(1f, value);
    }

    public float TrackWidth
    {
        get => trackWidth;
        set => trackWidth = Mathf.Max(100f, value);
    }

    public float JudgmentLinePosition
    {
        get => judgmentLinePosition;
        set => judgmentLinePosition = value;
    }

    public float PerfectWindow
    {
        get => perfectWindow;
        set => perfectWindow = Mathf.Max(0.01f, value);
    }

    public float GreatWindow
    {
        get => greatWindow;
        set => greatWindow = Mathf.Max(perfectWindow, value);
    }

    public float GoodWindow
    {
        get => goodWindow;
        set => goodWindow = Mathf.Max(greatWindow, value);
    }

    public int CurrentScore
    {
        get => currentScore;
        set => currentScore = Mathf.Max(0, value);
    }

    public int Combo
    {
        get => combo;
        set
        {
            combo = Mathf.Max(0, value);
            if (combo > maxCombo)
            {
                maxCombo = combo;
            }
        }
    }

    public int MaxCombo
    {
        get => maxCombo;
        set => maxCombo = Mathf.Max(0, value);
    }

    // === 계산된 프로퍼티 ===
    public float BeatDuration => 60f / bpm; // 한 비트의 시간 (초)

    public float CurrentGameTime => isPlaying ? Time.time - gameStartTime : 0f;

    public float CurrentBeatTime => CurrentGameTime % BeatDuration;

    public int CurrentBeatNumber => isPlaying ? (int)(CurrentGameTime / BeatDuration) : 0;

    // === 바 위치 계산 ===
    public float GetBarPositionAtTime(float gameTime)
    {
        float beatProgress = (gameTime % BeatDuration) / BeatDuration;
        return beatProgress * trackWidth;
    }

    public float GetCurrentBarPosition() => GetBarPositionAtTime(CurrentGameTime);

    // === 타이밍 판정 ===
    public HitAccuracy JudgeHitAccuracy(float inputTime, float targetTime)
    {
        float timeDifference = Mathf.Abs(inputTime - targetTime);

        if (timeDifference <= perfectWindow)
            return HitAccuracy.Perfect;
        else if (timeDifference <= greatWindow)
            return HitAccuracy.Great;
        else if (timeDifference <= goodWindow)
            return HitAccuracy.Good;
        else
            return HitAccuracy.Miss;
    }

    // === 점수 계산 ===
    public int GetScoreForAccuracy(HitAccuracy accuracy)
    {
        int baseScore = accuracy switch
        {
            HitAccuracy.Perfect => 100,
            HitAccuracy.Great => 80,
            HitAccuracy.Good => 50,
            HitAccuracy.Miss => 0,
            _ => 0
        };

        // 콤보 보너스 적용 (콤보 10마다 10% 증가, 최대 200%)
        float comboMultiplier = 1f + (combo / 10) * 0.1f;
        comboMultiplier = Mathf.Min(comboMultiplier, 2f);

        return (int)(baseScore * comboMultiplier);
    }

    // === 게임 상태 ===
    public void StartGame()
    {
        isPlaying = true;
        gameStartTime = Time.time;
        currentScore = 0;
        combo = 0;
        maxCombo = 0;
    }

    public void StopGame() => isPlaying = false;

    public void ResetGame()
    {
        StopGame();
        currentScore = 0;
        combo = 0;
        maxCombo = 0;
        gameStartTime = 0f;
    }

    public override string ToString()
    {
        return $"BPM: {bpm:F1} | Score: {currentScore} | Combo: {combo} | Playing: {isPlaying}";
    }
}
