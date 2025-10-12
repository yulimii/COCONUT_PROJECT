using UnityEngine;
using System;
using System.Collections.Generic;

public class BeatJudgeSystem : MonoBehaviour
{
    [Header("Clock (IMusicCore)")]
    public MonoBehaviour musicClockBehaviour; 

    private IMusicCore _clock;

    public event Action<int> OnBeat;
    public event Action<HitEvent> OnHit;

    [Header("Hit (sec)")]
    public float perfect = 0.050f; 
    public float good = 0.100f; 
    public float miss = 0.180f; 

    [Header("Search (sec)")]
    public float consumeWindowSec = 0.20f; 

    [Header("Chart")]
    public List<NoteData> notes = new List<NoteData>();

    private void Awake()
    {
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

    private void HandleClockBeat(int beatIndex)
    {
        if (OnBeat != null)
        {
            OnBeat.Invoke(beatIndex);
        }
    }

    public void HandleInput(LaneId lane)
    {
        if (_clock == null)
        {
            return;
        }

        double dTime = _clock.NowSec;

        if (!TryGetNearestTarget(lane, dTime, out var target, out var deltaSec))
        {
            var missEvent = new HitEvent
            {
                note = default,
                grade = HitGrade.MISS,
                deltaMs = 999.0f
            };

            Emit(missEvent);
            return;
        }

        HitGrade grade = Judge(Mathf.Abs((float)deltaSec));

        var hitEvent = new HitEvent
        {
            note = target,
            grade = grade,
            deltaMs = (float)(deltaSec * 1000.0f)
        };

        Emit(hitEvent);

        if (grade != HitGrade.LATE)
        {
            Consume(target);
        }
    }

    private HitGrade Judge(float absDeltaSec)
    {
        if (absDeltaSec <= perfect)
            return HitGrade.PERFECT;
        if (absDeltaSec <= good)
            return HitGrade.GOOD;
        if (absDeltaSec <= miss)
            return HitGrade.MISS;

        return HitGrade.LATE;
    }

    private void Emit(HitEvent hitEvent)
    {
        if (OnHit != null)
            OnHit.Invoke(hitEvent);
    }

    private bool TryGetNearestTarget(LaneId lane, double nowSec, out NoteData target, out double deltaSec)
    {
        target = default;
        deltaSec = double.MaxValue;

        float best = float.MaxValue;
        int bestIdx = -1;

        for (int i = 0; i < notes.Count; ++i)
        {
            NoteData stNoteData = notes[i];

            if (stNoteData.lane != lane)  continue;

            float fDelta = (float)(stNoteData.timeSec - nowSec);
            float fAbsDelta = Mathf.Abs(fDelta);

            if (fAbsDelta <= consumeWindowSec && fAbsDelta < best)
            {
                best = fAbsDelta;
                bestIdx = i;
                deltaSec = fDelta;
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
