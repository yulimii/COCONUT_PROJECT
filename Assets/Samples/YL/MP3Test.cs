using UnityEngine;
using UnityEngine.SceneManagement;

public class MP3Test : MonoBehaviour
{
    public AudioClip levelBgm;
    public AudioClip battleBgm;

    public double battleBpm = 120.0;
    public double startDelaySec = 0.3;

    [Range(0f,1f)] public float levelVolume = 0.8f;   // 인스펙터 바
    [Range(0f,1f)] public float battleVolume = 0.85f; // 인스펙터 바

    [Header("Scene Behavior")]
    public bool isBattleScene = false;
    public bool autoSwitchOnStart = false;
    public float autoSwitchDelay = 1.0f;

    private bool paused = false;
    private bool switchedToBattle = false;

    private void Start()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogError("SoundManager instance not found.");
            return;
        }

        // 시작할 때 한 번 반영
        SoundManager.Instance.SetLevelVol(levelVolume);
        SoundManager.Instance.SetBattleVol(battleVolume);

        if (levelBgm != null)
        {
            SoundManager.Instance.PlayLevelBGM(levelBgm, loop: true, volume: levelVolume);
        }

        if (isBattleScene && autoSwitchOnStart)
        {
            Invoke(nameof(SwitchToBattleBgm), Mathf.Max(0f, autoSwitchDelay));
        }
    }

    private void LateUpdate()
    {
        if (SoundManager.Instance == null)
        {
            return;
        }

        // 인스펙터에서 슬라이더를 움직이면 실시간 반영
        SoundManager.Instance.SetLevelVol(levelVolume);
        SoundManager.Instance.SetBattleVol(battleVolume);
    }

    private void Update()
    {
        if (SoundManager.Instance == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (battleBgm != null && isBattleScene) 
            {
                SoundManager.Instance.PlayBattleBGM(
                    battleBgm,
                    battleBpm,
                    startDelaySec,
                    loop: true,
                    volume: battleVolume
                );
            }
            else
            {
                Debug.LogWarning("battleBgm is null.");
            }
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (levelBgm != null)
            {
                SoundManager.Instance.PlayLevelBGM(
                    levelBgm,
                    loop: true,
                    volume: levelVolume
                );
            }
            else
            {
                Debug.LogWarning("levelBgm is null.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (paused)
            {
                SoundManager.Instance.ResumeBGM();
                paused = false;
            }
            else
            {
                SoundManager.Instance.PauseBGM();
                paused = true;
            }
        }
    }

    private void SwitchToBattleBgm()
    {
        if (switchedToBattle)
        {
            return;
        }

        if (battleBgm == null)
        {
            Debug.LogWarning("battleBgm is null.");
            return;
        }

        SoundManager.Instance.PlayBattleBGM(
            battleBgm,
            battleBpm,
            startDelaySec,
            loop: true,
            volume: battleVolume
        );

        switchedToBattle = true;
        paused = false;
    }

    private void OnGUI()
    {
        string mode = SoundManager.Instance != null ? SoundManager.Instance.Mode.ToString() : "NO_SM";
        string pauseStr = paused ? "PAUSED" : "PLAYING";
        string sceneType = isBattleScene ? "BATTLE-SCENE" : "LEVEL-SCENE";
        GUILayout.Label($"Scene: {sceneType} | Mode: {mode} | {pauseStr}");
        GUILayout.Label("Keys: B=Battle, L=Level, Space=Pause/Resume");
    }
}
