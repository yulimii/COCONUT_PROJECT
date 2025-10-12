using System;

// 레인(트랙) 식별자: A~D. UI/노트 차트와 동일 순서로 매핑
public enum LaneId
{
    A = 0,
    B = 1,
    C = 2,
    D = 3
}

// 판정 등급: Judge(abs(delta)) 결과로 산출
public enum HitGrade
{
    PERFECT,
    GOOD,
    MISS,
    LATE
}

[Serializable]
public struct NoteData
{
    // 노트 목표 시각(절대 초, double). BPM→BeatIndex→초로 환산한 값
    public double timeSec;

    // 노트가 속한 레인
    public LaneId lane;
}

public struct HitEvent
{
    // 대상 노트 정보
    public NoteData note;

    // 최종 판정 등급
    public HitGrade grade;

    // 입력 시각과 타겟 시각의 차이(ms). 정의: input - target
    public float deltaMs;
}
