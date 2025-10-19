using System;
using UnityEngine;

public class LoveModel : MonoBehaviour
{
    public enum Emotion
    {
        None,
        Angry,
        Sad,
        Idle,
        Happy,
        Love,
        Max
    }

    public event Action<Emotion> EmotionChanged;

    [SerializeField] Emotion emotion = Emotion.None;

    public Emotion CurrentEmotion
    {
        get => emotion;
        set
        {
            if (emotion != value)
            {
                emotion = value;
                EmotionChanged?.Invoke(value);
            }
        }
    }
}
