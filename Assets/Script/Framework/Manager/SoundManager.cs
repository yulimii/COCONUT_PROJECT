using System;
using UnityEngine;

[DisallowMultipleComponent]
public class SoundManager : MonoBehaviour, IMusicCore
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindFirstObjectByType<SoundManager>();
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    // 게임 시작 시 1회 생성
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoBootstrap()
    {
        if (Instance != null) 
            return;

        SoundManager existing = UnityEngine.Object.FindFirstObjectByType<SoundManager>();

        if (existing != null)
        {
            Instance = existing;
            return;
        }

        GameObject soundManager = new GameObject("SoundManager");
        soundManager.AddComponent<SoundManager>();
    }

    // ─────────────────────────────────────────────────────────────
    // [BGM 모드 상태] 현재 어떤 종류의 BGM이 재생/대기 중인지 표현
    // - SOUND_NONE   : 아무것도 재생하지 않음
    // - SOUND_LEVEL  : 레벨/인트로용 일반 BGM (박자 판정 비활성)
    // - SOUND_BATTLE : 배틀용 박자 BGM (박자 판정 활성)
    // ─────────────────────────────────────────────────────────────
    public enum BgmMode
    {
        SOUND_NONE,
        SOUND_LEVEL,
        SOUND_BATTLE
    }
    public BgmMode Mode { get; private set; } = BgmMode.SOUND_NONE;
    // ─────────────────────────────────────────────────────────────

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Beat / Clock (Battle only)")]
    [Range(40, 240)] public double bpm = 120.0f;
    public double startDelaySec = 2.0f;
    public int userOffsetMs = 0;

    [Header("Scene Defaults")]
    public AudioClip defaultLevelBgm;
    public AudioClip defaultBattleBgm;

    public event Action<int> OnBeat;

    public double NowSec
    {
        get
        {
            if (!_beatActive)
                return 0.0;

            var baseSec = AudioSettings.dspTime - _songStartDsp;
            return baseSec + (userOffsetMs / 1000.0);
        }
    }

    public double BeatInterval => (_beatActive && bpm > 0.0) ? 60.0 / bpm : double.PositiveInfinity;
    private bool   _beatActive = false;
    private double _songStartDsp;
    private double _nextBeatDsp;
    private int _beatIndex;
    private float _lastAppliedVol = -1f;
    private bool _fading = false;
    private float _fadeSpeed = 0f;
    
    [Header("Volumes")]
    [Range(0f,1f)] public float master = 1.0f;
    [Range(0f,1f)] public float levelVol = 0.8f;
    [Range(0f,1f)] public float battleVol = 0.85f;

    // ─────────────────────────────────────────────────────────────
    // Life Cycle
    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!bgmSource)
            bgmSource = gameObject.AddComponent<AudioSource>();

        if (!sfxSource)
            sfxSource = gameObject.AddComponent<AudioSource>();

        bgmSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
        bgmSource.loop = true;
    }

    private void Update()
    {
        if (_fading)
        {
            if (bgmSource == null)
            {
                _fading = false;
                return;
            }

            float target = GetTargetVolume();
            float cur = bgmSource.volume;
            float next = Mathf.MoveTowards(cur, target, _fadeSpeed * Time.unscaledDeltaTime);
            bgmSource.volume = next;

            if (Mathf.Approximately(next, target))
            {
                _fading = false;
                _lastAppliedVol = next;
            }
        }

        if (!_beatActive || !bgmSource || !bgmSource.isPlaying)
            return;

        var dsp = AudioSettings.dspTime;
        var beatInt = BeatInterval;

        if (double.IsInfinity(beatInt))
            return;

        while (dsp >= _nextBeatDsp)
        {
            OnBeat?.Invoke(_beatIndex++);
            _nextBeatDsp += beatInt;

            if (bgmSource.loop && bgmSource.clip != null)
            {
                var songPos = dsp - _songStartDsp;

                if (songPos >= bgmSource.clip.length)
                {
                    var cycles = Math.Floor(songPos / bgmSource.clip.length);
                    _songStartDsp += bgmSource.clip.length * cycles;
                    RealignNextBeat();
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    // ─────────────────────────────────────────────────────────────
    // ─────────────────────────────────────────────────────────────

    public void PlayLevelBGM(AudioClip clip, bool loop = true, float volume = 1.0f)
    {
        if (!bgmSource) return;

        _beatActive = false;
        Mode = BgmMode.SOUND_LEVEL;

        bgmSource.Stop();
        bgmSource.loop = loop;
        bgmSource.volume = Mathf.Clamp01(volume);
        bgmSource.clip = clip;

        if (clip != null)
            bgmSource.Play();
    }

    public void PlayBattleBGM(AudioClip clip, double battleBpm, double delaySec = 2.0, bool loop = true, float volume = 1.0f)
    {
        if (!bgmSource) return;

        bpm = Math.Max(1.0f, battleBpm);
        startDelaySec = Math.Max(0.0f, delaySec);

        bgmSource.Stop();
        bgmSource.loop = loop;
        bgmSource.volume = Mathf.Clamp01(volume);
        bgmSource.clip = clip;

        _songStartDsp = AudioSettings.dspTime + startDelaySec;
        bgmSource.PlayScheduled(_songStartDsp);

        _beatIndex   = 0;
        _nextBeatDsp = _songStartDsp;
        _beatActive  = true;
        Mode         = BgmMode.SOUND_BATTLE;
    }
  
    public void SwitchBattlePhase(AudioClip battleClip, double battleBpm, double delaySec = 2.0, bool loop = true, float volume = 1.0f)
    {
        StopBGM();
        PlayBattleBGM(battleClip, battleBpm, delaySec, loop, volume);
    }
 
    public void StopBGM()
    {
        if (!bgmSource) return;

        bgmSource.Stop();
        _beatActive = false;
        Mode = BgmMode.SOUND_NONE;
    }

    public void PauseBGM()
    {
        if (!bgmSource) return;

        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (!bgmSource) return;

        RealignNextBeat();
        bgmSource.UnPause();
    }

    public void SeekBGM(double time)
    {
        if (!bgmSource || !bgmSource.clip)
            return;

        time = Mathf.Clamp((float)time, 0.0f, bgmSource.clip.length);

        bgmSource.Stop();

        _songStartDsp = AudioSettings.dspTime - time;
        bgmSource.PlayScheduled(AudioSettings.dspTime);

        if (_beatActive)
            RealignNextBeat();
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmSource)
            bgmSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource)
            sfxSource.volume = Mathf.Clamp01(volume);
    }

    public void PlaySFXOneShot(AudioClip clip, float volume = 1.0f)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    private void RealignNextBeat()
    {
        if (!_beatActive) return;

        var dsp = AudioSettings.dspTime;
        var beatInt = BeatInterval;

        if (double.IsInfinity(beatInt))
            return;

        var beatsPassed = (dsp - _songStartDsp) / beatInt;
        var nextIdx = Math.Ceiling(beatsPassed);
        _nextBeatDsp = _songStartDsp + nextIdx * beatInt;
        _beatIndex = (int)nextIdx;
    }
   
    private float GetTargetVolume()
    {
        float baseVol = (Mode == BgmMode.SOUND_BATTLE) ? battleVol : levelVol;
        float vol = Mathf.Clamp01(master * baseVol);
        return vol;
    }

    private void ApplyVolumeNow()
    {
        if ( !bgmSource ) return;

        float vol = GetTargetVolume();

        if (!Mathf.Approximately(vol, _lastAppliedVol))
        {
            bgmSource.volume = vol;
            _lastAppliedVol = vol;
        }
    }

    // 외부에서 옵션 UI가 값만 바꿔도 즉시 반영됨
    private void LateUpdate()
    {
        ApplyVolumeNow();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyVolumeNow();
        }
    }

    public void SetMaster(float vol)
    {
        master = Mathf.Clamp01(vol);
        ApplyVolumeNow();
    }

    public void SetLevelVol(float vol)
    {
        levelVol = Mathf.Clamp01(vol);
        ApplyVolumeNow();
    }

    public void SetBattleVol(float vol)
    {
        battleVol = Mathf.Clamp01(vol);
        ApplyVolumeNow();
    }

    public void FadeToCurrentTarget(float seconds)
    {
        float duration = Mathf.Max(0.0001f, seconds);
        float diff = Mathf.Abs(GetTargetVolume() - (bgmSource != null ? bgmSource.volume : 0f));
        _fadeSpeed = diff / duration;
        _fading = true;
    }

}
