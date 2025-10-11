using System;

public enum LaneId { A = 0, B = 1, C = 2, D = 3 }
public enum HitGrade { Perfect, Good, Miss, TooLate }

[Serializable]
public struct NoteData
{
    public double timeSec; // ���� ��(�� ����)
    public LaneId lane;
}

public struct HitEvent
{
    public NoteData note;
    public HitGrade grade;
    public float deltaMs; // �Է�-Ÿ�� ����(ms, ��ȣ)
}