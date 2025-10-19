using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    SaveLoadSystem saveLoadSystem;

    void Awake()
    {
        if (Instance != null)
        {
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);

        LoadData();
    }

    void LoadData()
    {
        saveLoadSystem = new SaveLoadSystem();
        saveLoadSystem.Load();
    }

    public SaveLoadSystem GetSaveLoadSystem()
    {
        return saveLoadSystem;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            saveLoadSystem.Save();
        }
    }

    void OnApplicationQuit()
    {
#if UNITY_EDITOR
        saveLoadSystem.Save();
#endif
    }
}

public class SaveLoadSystem
{
    public SaveLoadSystem()
    {
        //init
    }

    public void Load()
    {

    }

    public void Save()
    {

    }
}