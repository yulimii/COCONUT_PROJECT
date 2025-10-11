using System;

// ============================================================================
// Role   : 리듬/판정 시스템이 참조하는 음악 타임라인 인터페이스
// Notes  : NowSec은 "DSP 기준 절대 초"로 노출하는 것을 권장
//          BeatInterval = 60 / BPM (초/beat)
// Events : OnBeat(beatIndex) - 각 박자 경계에서 호출
// ============================================================================

public interface IMusicCore
{
    // 현재 곡 진행 시간(절대 초). 사용자 오프셋 반영 여부는 구현체 책임
    double NowSec { get; }

    // 한 박자 간격(초). BPM에 의해 결정
    double BeatInterval { get; }

    // 박자 신호(0,1,2,…)를 비동기 이벤트로 전달
    event Action<int> OnBeat;
}
