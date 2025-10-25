using UnityEngine;
using System;
using System.Collections.Generic;

public class BeatJudgeSystem : MonoBehaviour
{
    [Header("Clock (IMusicCore)")]
    public MonoBehaviour musicClockBehaviour;    // SoundManager 등 IMusicCore 구현체

    private IMusicCore _clock;

    public event Action<int> OnBeat;
    public event Action<HitEvent> OnHit;

    [Header("Hit (sec)")]
    public float perfect = 0.050f;
    public float great   = 0.100f;
    public float good    = 0.150f;

    [Header("Search (sec)")]
    public float consumeWindowSec = 0.20f; 

    [Header("Chart")]
    public List<NoteData> notes = new List<NoteData>();

    private void Awake()
    {
        _clock = musicClockBehaviour as IMusicCore;
        if (_clock == null)
            Debug.LogError("musicClockBehaviour must implement IMusicCore.");
    }

    private void OnEnable()
    {
        if (_clock != null) _clock.OnBeat += HandleClockBeat;
    }

    private void OnDisable()
    {
        if (_clock != null) _clock.OnBeat -= HandleClockBeat;
    }

    private void HandleClockBeat(int beatIndex)
    {
        OnBeat?.Invoke(beatIndex);
    }

    public void HandleInput(LaneId lane)
    {
        if (_clock == null) return;

        double nowSec = _clock.NowSec;

        if (!TryGetNearestTarget(lane, nowSec, out var target, out var deltaSec))
        {
            Emit(new HitEvent { note = default, grade = HitAccuracy.Miss, deltaMs = 999.0f });
            return;
        }

        // 판정
        float absDelta = Mathf.Abs((float)deltaSec);
        HitAccuracy grade = Judge(absDelta);

        Emit(new HitEvent
        {
            note   = target,
            grade  = grade,
            deltaMs = (float)(deltaSec * 1000.0f)
        });

        // 성공 판정만 소모
        if (grade != HitAccuracy.Miss)
            Consume(target);
    }

    private HitAccuracy Judge(float absDeltaSec)
    {
        if (absDeltaSec <= perfect) return HitAccuracy.Perfect;
        if (absDeltaSec <= great)   return HitAccuracy.Great;
        if (absDeltaSec <= good)    return HitAccuracy.Good;
        return HitAccuracy.Miss;
    }

    private void Emit(HitEvent e) => OnHit?.Invoke(e);

    private bool TryGetNearestTarget(LaneId lane, double nowSec, out NoteData target, out double deltaSec)
    {
        target = default;
        deltaSec = double.MaxValue;

        float best = float.MaxValue;
        int bestIdx = -1;

        for (int i = 0; i < notes.Count; ++i)
        {
            var n = notes[i];
            if (n.lane != lane) continue;

            float d = (float)(n.timeSec - nowSec);
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
