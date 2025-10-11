using UnityEngine;
using System;
using System.Collections.Generic;

// ============================================================================
// Role   : 입력 → 최근접 타겟 노트 매칭 → 판정 → 이벤트 발행
// Clock  : IMusicCore(예: SoundManager)에서 NowSec/OnBeat을 공급받음
// Windows: perfect/good/miss는 "±초" 범위. miss 초과는 TooLate 처리
// Consume: 판정 성공 시 동일 (lane,timeSec) 노트를 소모하여 중복 히트 방지
// ============================================================================

public class BeatJudgeSystem : MonoBehaviour
{
    [Header("Clock (IMusicCore 구현체)")]
    public MonoBehaviour musicClockBehaviour;   // SoundManager 드래그

    private IMusicCore _clock;

    // 외부 구독용 이벤트: 박자/히트
    public event Action<int> OnBeat;
    public event Action<HitEvent> OnHit;

    [Header("Hit Window (±sec)")]
    public float perfect = 0.050f;  // ±0.050s 이내면 Perfect
    public float good = 0.100f;  // ±0.100s 이내면 Good
    public float miss = 0.180f;  // ±0.180s 이내면 Miss (초과 시 TooLate)

    [Header("Search Window (sec)")]
    public float consumeWindowSec = 0.20f; // 최근접 타겟 탐색/소모 허용 범위

    [Header("Chart")]
    public List<NoteData> notes = new List<NoteData>();

    private void Awake()
    {
        // Why: 인스펙터에 MonoBehaviour로 배치되므로 as 캐스팅 필요
        _clock = musicClockBehaviour as IMusicCore;

        if (_clock == null)
        {
            Debug.LogError("musicClockBehaviour must implement IMusicCore.");
        }
    }

    private void OnEnable()
    {
        if (_clock != null)
        {
            _clock.OnBeat += HandleClockBeat;
        }
    }

    private void OnDisable()
    {
        if (_clock != null)
        {
            _clock.OnBeat -= HandleClockBeat;
        }
    }

    // 박자 신호를 그대로 중계
    private void HandleClockBeat(int beatIndex)
    {
        if (OnBeat != null)
        {
            OnBeat.Invoke(beatIndex);
        }
    }

    // 외부 입력(특정 레인) → 최근접 노트 판정
    public void HandleInput(LaneId lane)
    {
        if (_clock == null)
        {
            return;
        }

        double t = _clock.NowSec;

        if (!TryGetNearestTarget(lane, t, out var target, out var deltaSec))
        {
            // 타겟이 없으면 Miss로 이벤트만 발행(소모 없음)
            var missEvent = new HitEvent
            {
                note = default,
                grade = HitGrade.Miss,
                deltaMs = 999.0f
            };

            Emit(missEvent);
            return;
        }

        // Judge는 절대값으로 등급 결정을 수행
        HitGrade grade = Judge(Mathf.Abs((float)deltaSec));

        var e = new HitEvent
        {
            note = target,
            grade = grade,
            // deltaMs 부호 유지: (입력 - 타겟) * 1000
            deltaMs = (float)(deltaSec * 1000.0f)
        };

        Emit(e);

        // TooLate는 너무 늦게 들어온 입력 → 노트 소모 없음
        if (grade != HitGrade.TooLate)
        {
            Consume(target);
        }
    }

    // 절대 편차(초) → 등급 매핑
    private HitGrade Judge(float absDeltaSec)
    {
        if (absDeltaSec <= perfect)
        {
            return HitGrade.Perfect;
        }
        if (absDeltaSec <= good)
        {
            return HitGrade.Good;
        }
        if (absDeltaSec <= miss)
        {
            return HitGrade.Miss;
        }

        return HitGrade.TooLate;
    }

    // 이벤트 발행 유틸
    private void Emit(HitEvent e)
    {
        if (OnHit != null)
        {
            OnHit.Invoke(e);
        }
    }

    // nowSec 기준으로 lane 내 최근접 타겟 탐색(consumeWindowSec 범위 내)
    private bool TryGetNearestTarget(LaneId lane, double nowSec, out NoteData target, out double deltaSec)
    {
        target = default;
        deltaSec = double.MaxValue;

        float best = float.MaxValue;
        int bestIdx = -1;

        for (int i = 0; i < notes.Count; ++i)
        {
            NoteData n = notes[i];

            if (n.lane != lane)
            {
                continue;
            }

            float d = (float)(n.timeSec - nowSec); // +면 타겟이 미래, -면 과거
            float ad = Mathf.Abs(d);

            if (ad <= consumeWindowSec && ad < best)
            {
                best = ad;
                bestIdx = i;
                deltaSec = d;
            }
        }

        if (bestIdx >= 0)
        {
            target = notes[bestIdx];
            return true;
        }

        return false;
    }

    // 동일 (lane,timeSec) 노트를 1개 제거하여 중복 처리 방지
    private void Consume(NoteData note)
    {
        for (int i = 0; i < notes.Count; ++i)
        {
            if (notes[i].lane == note.lane && notes[i].timeSec == note.timeSec)
            {
                notes.RemoveAt(i);
                return;
            }
        }
    }
}
