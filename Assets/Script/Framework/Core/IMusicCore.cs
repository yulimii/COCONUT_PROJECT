using System;

public interface IMusicCore
{
    double NowSec { get; }         
    double BeatInterval { get; }    
    event Action<int> OnBeat;      // SoundManager에서 매 박자마다 Invoke
}