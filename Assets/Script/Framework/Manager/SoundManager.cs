using System;
using UnityEngine;

[DisallowMultipleComponent]
public class SoundManager : MonoBehaviour, IMusicCore
{
    // ─────────────────────────────────────────────────────────────
    // [싱글톤] 
    // ─────────────────────────────────────────────────────────────
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
        // 에디터에서 "Enter Play Mode Options > Domain Reload 끔" 일 때
        // 정적 필드가 남아 있을 수 있으니 Instance와 씬 객체 둘 다 점검
        if (Instance != null)
        {
            return;
        }

        SoundManager existing = UnityEngine.Object.FindFirstObjectByType<SoundManager>();

        if (existing != null)
        {
            Instance = existing;
            return;
        }

        GameObject go = new GameObject("SoundManager");
        go.AddComponent<SoundManager>();
    }
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [BGM 모드 상태] 현재 어떤 종류의 BGM이 재생/대기 중인지 표현
    // - SOUND_NONE   : 아무것도 재생하지 않음
    // - SOUND_LEVEL  : 레벨/인트로용 일반 BGM (박자 판정 비활성)
    // - SOUND_BATTLE : 배틀용 박자 BGM (박자 판정 활성)
    //  ※ 외부 로직이 현재 상태를 참고해 흐름 제어할 수 있음
    // ─────────────────────────────────────────────────────────────
    public enum BgmMode
    {
        SOUND_NONE,
        SOUND_LEVEL,
        SOUND_BATTLE
    }
    // ─────────────────────────────────────────────────────────────

    // 외부에서 읽기만 가능. 모드 변경은 본 클래스의 Play/Stop 계열에서 수행
    public BgmMode Mode { get; private set; } = BgmMode.SOUND_NONE;
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [오디오 소스]
    // - bgmSource : BGM 전용 (loop 기본 on)
    // - sfxSource : 효과음 전용 (PlayOneShot 권장)
    //  ※ 인스펙터에서 주입하지 않으면 런타임에 자동 생성
    // ─────────────────────────────────────────────────────────────
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [배틀용 박자/시계 파라미터]
    // - bpm           : 분당 박자수 (배틀 BGM용)
    // - startDelaySec : 배틀곡을 DSP 스케줄로 시작할 때 예약 지연(초)
    // - userOffsetMs  : 사용자 타이밍 보정(ms). +면 늦게 치는 사람 보정
    // ─────────────────────────────────────────────────────────────
    [Header("Beat / Clock (Battle only)")]
    [Range(40, 240)] public double bpm = 120.0f;
    public double startDelaySec = 2.0f;
    public int userOffsetMs = 0;
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    [Header("Scene Defaults")]
    public AudioClip defaultLevelBgm;
    public AudioClip defaultBattleBgm;
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [IMusicCore 이벤트] 매 박자마다 발생
    // - BeatJudgeSystem 등 (박자 판정/연출 트리거)
    // ─────────────────────────────────────────────────────────────
    public event Action<int> OnBeat;
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [IMusicCore] 현재 진행 시간(초) - 사용자 보정 적용
    // - 배틀 박자 모드가 아닐 때는 0 반환
    // - 오디오 DSP 타임 기준. 프레임 스텝/타임스케일 영향 적음
    // ─────────────────────────────────────────────────────────────
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
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [IMusicCore] 박자 간격(초)
    // - 배틀 박자 모드일 때만 유효
    // ─────────────────────────────────────────────────────────────
    public double BeatInterval
        => (_beatActive && bpm > 0.0) ? 60.0 / bpm : double.PositiveInfinity;
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [내부 상태]
    // - _beatActive   : 박자 모드 on/off
    // - _songStartDsp : PlayScheduled 기준 시작 DSP 시각
    // - _nextBeatDsp  : 다음 박자가 울릴 DSP 시각
    // - _beatIndex    : 지금까지 발생시킨 박자 인덱스(0부터)
    // ─────────────────────────────────────────────────────────────
    private bool   _beatActive = false;
    private double _songStartDsp;
    private double _nextBeatDsp;
    private int _beatIndex;
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // 마지막으로 적용한 실제 소스 볼륨 캐시 값
    // // 같은 프레임/같은 값 재적용을 피해서 불필요한 SetVolume 호출과 미세한 연산/GC를 줄인다
    // ─────────────────────────────────────────────────────────────
    private float _lastAppliedVol = -1f;
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // 부드러운 페이드(현재 모드의 목표값으로 서서히 맞추기)
    // ─────────────────────────────────────────────────────────────
    private bool _fading = false;
    private float _fadeSpeed = 0f;
    
    // ───────── Volumes (Manager-owned)
    [Header("Volumes")]
    [Range(0f,1f)] public float master = 1.0f;
    [Range(0f,1f)] public float levelVol = 0.8f;
    [Range(0f,1f)] public float battleVol = 0.85f;
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [Awake] 싱글톤 구성 + 소스 준비
    // - bgm/sfx 소스가 미연결이면 AddComponent로 생성
    // - playOnAwake 해제, BGM loop 기본 on
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
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [Update] 박자 스케줄러
    // - 배틀 모드에서만 동작
    // - AudioSettings.dspTime을 확인해 _nextBeatDsp를 넘겼으면
    //   OnBeat를 연속 발생시키면서 다음 박자 시각을 갱신
    // - 루프 곡의 길이를 넘겼을 때 보정(드리프트 누적 방지)
    // ─────────────────────────────────────────────────────────────
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
    // ─────────────────────────────────────────────────────────────


    // ─────────────────────────────────────────────────────────────
    // 종료 시 제거
    // ─────────────────────────────────────────────────────────────
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [레벨/인트로용 BGM 재생] (박자 비활성)
    // - 배틀씬의 ‘전투 전 인트로’나 레벨씬의 일반 BGM 재생 시 사용
    // - clip이 null이면 재생하지 않음
    // - Mode를 SOUND_LEVEL로 세팅(상태 노출)
    // ─────────────────────────────────────────────────────────────
    public void PlayLevelBGM(AudioClip clip, bool loop = true, float volume = 1.0f)
    {
        if (!bgmSource)
            return;

        _beatActive = false;
        Mode = BgmMode.SOUND_LEVEL;

        bgmSource.Stop();
        bgmSource.loop = loop;
        bgmSource.volume = Mathf.Clamp01(volume);
        bgmSource.clip = clip;

        if (clip != null)
            bgmSource.Play();
    }
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [배틀용 박자 BGM 재생] (박자 활성)
    // - 정확한 BPM 기준으로 DSP 스케줄 기반 재생
    // - startDelaySec 후에 첫 박자가 0번으로 울리도록 정렬
    // - Mode를 SOUND_BATTLE로 세팅(상태 노출)
    // ─────────────────────────────────────────────────────────────
    public void PlayBattleBGM(AudioClip clip, double battleBpm, double delaySec = 2.0, bool loop = true, float volume = 1.0f)
    {
        if (!bgmSource)
            return;

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
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [배틀 전환] 인트로/레벨 BGM → 배틀 박자 BGM으로 전환
    // - 다른 작업자가 전투 시작 이벤트에서 이 함수를 호출하면 됨
    // - 내부적으로 Stop → PlayBattleBGM 순서로 처리
    // ─────────────────────────────────────────────────────────────
    public void SwitchBattlePhase(AudioClip battleClip, double battleBpm, double delaySec = 2.0, bool loop = true, float volume = 1.0f)
    {
        StopBGM();
        PlayBattleBGM(battleClip, battleBpm, delaySec, loop, volume);
    }
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [정지] 현재 재생 중인 BGM을 정지하고 박자 비활성화
    // - Mode를 SOUND_NONE으로 변경
    // ─────────────────────────────────────────────────────────────
    public void StopBGM()
    {
        if (!bgmSource)
            return;

        bgmSource.Stop();
        _beatActive = false;
        Mode = BgmMode.SOUND_NONE;
    }
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [일시정지]
    // - DSP 스케줄은 유지되지만 오디오 출력만 멈춤
    // ─────────────────────────────────────────────────────────────
    public void PauseBGM()
    {
        if (!bgmSource)
            return;

        bgmSource.Pause();
    }
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [재개]
    // - 재개 전에 RealignNextBeat로 다음 박자를 현재 DSP 기준으로 재정렬
    // - 루프/일시정지로 인한 드리프트를 최소화
    // ─────────────────────────────────────────────────────────────
    public void ResumeBGM()
    {
        if (!bgmSource)
            return;

        RealignNextBeat();
        bgmSource.UnPause();
    }
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [시크(초 단위)]
    // - 현재 곡을 지정한 절대 위치(초)로 이동
    // - 간단 구현: Stop → 현재 DSP로 즉시 재스케줄
    // - 배틀 모드면 RealignNextBeat로 박자 재정렬 수행
    //   (정확한 시크가 중요하면 오디오 서브시스템 정책에 맞춰 확장 가능)
    // ─────────────────────────────────────────────────────────────
    public void SeekBGM(double tSec)
    {
        if (!bgmSource || !bgmSource.clip)
            return;

        tSec = Mathf.Clamp((float)tSec, 0.0f, bgmSource.clip.length);

        bgmSource.Stop();

        _songStartDsp = AudioSettings.dspTime - tSec;
        bgmSource.PlayScheduled(AudioSettings.dspTime);

        if (_beatActive)
            RealignNextBeat();
    }
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [볼륨 제어] 0~1 범위로 클램프
    // - BGM/SFX를 각각 조절 (마스터 볼륨은 AudioMixer 권장)
    // ─────────────────────────────────────────────────────────────
    public void SetBGMVolume(float v)
    {
        if (bgmSource)
            bgmSource.volume = Mathf.Clamp01(v);
    }
    // ─────────────────────────────────────────────────────────────

    public void SetSFXVolume(float v)
    {
        if (sfxSource)
            sfxSource.volume = Mathf.Clamp01(v);
    }
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [SFX 재생] 단발성 효과음
    // - PlayOneShot 사용: 현재 재생 중인 클립과 무관하게 중첩 가능
    // ─────────────────────────────────────────────────────────────
    public void PlaySFXOneShot(AudioClip clip, float volume = 1.0f)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
    // ─────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────
    // [내부 유틸] 다음 박자 재정렬
    // - 일시정지/루프 구간에서 박자 드리프트를 줄이기 위한 보정
    // - dspTime을 기준으로 다음 박자 인덱스를 올림(ceil)하여 계산
    // ─────────────────────────────────────────────────────────────
    private void RealignNextBeat()
    {
        if (!_beatActive)
            return;

        var dsp = AudioSettings.dspTime;
        var beatInt = BeatInterval;

        if (double.IsInfinity(beatInt))
            return;

        var beatsPassed = (dsp - _songStartDsp) / beatInt;
        var nextIdx = Math.Ceiling(beatsPassed);
        _nextBeatDsp = _songStartDsp + nextIdx * beatInt;
        _beatIndex = (int)nextIdx;
    }
    // ─────────────────────────────────────────────────────────────
    
    // ─────────────────────────────────────────────────────────────
    // [볼륨 적용 루틴]
    // - SGF: master × (mode별 level/battle)을 계산해 실제 소스에 반영
    // - LateUpdate와 OnValidate에서 지속 반영(실시간 슬라이더 대응)
    // ─────────────────────────────────────────────────────────────
    private float GetTargetVolume()
    {
        float baseVol = (Mode == BgmMode.SOUND_BATTLE) ? battleVol : levelVol;
        float v = Mathf.Clamp01(master * baseVol);
        return v;
    }

    private void ApplyVolumeNow()
    {
        if (bgmSource == null)
        {
            return;
        }

        float v = GetTargetVolume();

        if (!Mathf.Approximately(v, _lastAppliedVol))
        {
            bgmSource.volume = v;
            _lastAppliedVol = v;
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

    public void SetMaster(float v)
    {
        master = Mathf.Clamp01(v);
        ApplyVolumeNow();
    }

    public void SetLevelVol(float v)
    {
        levelVol = Mathf.Clamp01(v);
        ApplyVolumeNow();
    }

    public void SetBattleVol(float v)
    {
        battleVol = Mathf.Clamp01(v);
        ApplyVolumeNow();
    }

    public void FadeToCurrentTarget(float seconds)
    {
        float dur = Mathf.Max(0.0001f, seconds);
        float diff = Mathf.Abs(GetTargetVolume() - (bgmSource != null ? bgmSource.volume : 0f));
        _fadeSpeed = diff / dur;
        _fading = true;
    }
    // ─────────────────────────────────────────────────────────────

}
