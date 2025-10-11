using System;

// ============================================================================
// Role   : 리듬/판정 시스템 공용 데이터 타입(열거형/구조체) 정의
// Note   : 전체 코드베이스 어디서든 직렬화/직참조 가능한 가벼운 DTO로 유지
// Time   : 곡 기준 "절대 초(double)" 사용 → DSP/게임타임 변환 명확
// Delta  : deltaMs = inputTimeMs - targetTimeMs (양수=Late, 음수=Early)
// ============================================================================

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
    Perfect,
    Good,
    Miss,
    TooLate
}

// 특정 노트(타겟)의 메타데이터
[Serializable]
public struct NoteData
{
    // 노트 목표 시각(절대 초, double). BPM→BeatIndex→초로 환산한 값
    public double timeSec;

    // 노트가 속한 레인
    public LaneId lane;
}

// 입력 판정 결과 DTO(한 번의 히트)
public struct HitEvent
{
    // 대상 노트 정보(시각/레인)
    public NoteData note;

    // 최종 판정 등급
    public HitGrade grade;

    // 입력 시각과 타겟 시각의 차이(ms). 정의: input - target
    //   > 0 => 늦게 입력(Late), < 0 => 일찍 입력(Early)
    public float deltaMs;
}
