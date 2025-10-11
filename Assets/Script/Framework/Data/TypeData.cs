using System;

public enum LaneId { A = 0, B = 1, C = 2, D = 3 }
public enum HitGrade { Perfect, Good, Miss, TooLate }

[Serializable]
public struct NoteData
{
    public double timeSec; // 절대 초(곡 기준)
    public LaneId lane;
}

public struct HitEvent
{
    public NoteData note;
    public HitGrade grade;
    public float deltaMs; // 입력-타겟 차이(ms, 부호)
}