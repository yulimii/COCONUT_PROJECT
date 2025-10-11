using System;

public interface IMusicCore
{
    double NowSec { get; }         
    double BeatInterval { get; }    
    event Action<int> OnBeat;      // SoundManager���� �� ���ڸ��� Invoke
}