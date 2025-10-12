using System;

public interface IMusicCore
{
    // 현재 곡 진행 시간(절대 초)
    double NowSec { get; }

    // 한 박자 간격(초)
    double BeatInterval { get; }

    // 박자 신호(0,1,2,…)를 비동기 이벤트로 전달
    event Action<int> OnBeat;
}
