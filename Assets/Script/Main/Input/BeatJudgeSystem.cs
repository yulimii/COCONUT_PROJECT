using UnityEngine;
using System;
using System.Collections.Generic;

public class BeatJudgeSystem : MonoBehaviour
{
    [Header("Clock (IMusicCore ±¸ÇöÃ¼)")]
    public MonoBehaviour musicClockBehaviour;   // SoundManager
    IMusicCore _clock;

    public event Action<int> OnBeat;  
    public event Action<HitEvent> OnHit;

    [Header("Hit Window (¡¾sec)")]
    public float perfect = 0.050f;
    public float good    = 0.100f;
    public float miss    = 0.180f;
    [Header("Search Window (sec)")]
    public float consumeWindowSec = 0.20f;

    [Header("Chart")]
    public List<NoteData> notes = new List<NoteData>();

    void Awake()
    {
        _clock = musicClockBehaviour as IMusicCore;
        if (_clock == null)
            Debug.LogError("musicClockBehaviour must implement IMusicCore."); 
    }

    void OnEnable()
    {
        if (_clock != null) _clock.OnBeat += HandleClockBeat;  
    }

    void OnDisable()
    {
        if (_clock != null) _clock.OnBeat -= HandleClockBeat;  
    }

    void HandleClockBeat(int beatIndex)
    {
        OnBeat?.Invoke(beatIndex);
    }

    public void HandleInput(LaneId lane)
    {
        if (_clock == null) return;

        double t = _clock.NowSec;
        if (!TryGetNearestTarget(lane, t, out var target, out var deltaSec))
        {
            Emit(new HitEvent { note = default, grade = HitGrade.Miss, deltaMs = 999f });
            return;
        }

        var grade = Judge(Mathf.Abs((float)deltaSec));
        Emit(new HitEvent
        {
            note = target,
            grade = grade,
            deltaMs = (float)(deltaSec * 1000.0)
        });

        if (grade != HitGrade.TooLate)
            Consume(target);
    }

    HitGrade Judge(float absDeltaSec)
    {
        if (absDeltaSec <= perfect) return HitGrade.Perfect;
        if (absDeltaSec <= good)    return HitGrade.Good;
        if (absDeltaSec <= miss)    return HitGrade.Miss;
        return HitGrade.TooLate;
    }

    void Emit(HitEvent e) => OnHit?.Invoke(e);

    bool TryGetNearestTarget(LaneId lane, double nowSec, out NoteData target, out double deltaSec)
    {
        target = default;
        deltaSec = double.MaxValue;

        float best = float.MaxValue;
        int bestIdx = -1;

        for (int i = 0; i < notes.Count; ++i)
        {
            var n = notes[i];
            if (n.lane != lane) continue;

            float d  = (float)(n.timeSec - nowSec);
            float ad = Mathf.Abs(d);
            if (ad <= consumeWindowSec && ad < best)
            {
                best = ad;
                bestIdx = i;
                deltaSec = d;
            }
        }

        if (bestIdx >= 0) { target = notes[bestIdx]; return true; }
        return false;
    }

    void Consume(NoteData note)
    {
        for (int i = 0; i < notes.Count; ++i)
            if (notes[i].lane == note.lane && notes[i].timeSec == note.timeSec)
            { notes.RemoveAt(i); return; }
    }
}
