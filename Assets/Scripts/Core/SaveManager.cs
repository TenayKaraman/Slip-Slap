using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [System.Serializable]
    public class SaveData
    {
        public int currentLevel = 1;
        public int playerLives = 5;
        public int totalCrystals = 0;
        public bool[] unlockedLevels;

        public SaveData()
        {
            unlockedLevels = new bool[50]; // 50 seviye için
            unlockedLevels[0] = true; // Ýlk seviye açýk
        }
    }

    private SaveData currentSave;
    private const string SAVE_KEY = "KayipGezegenSave";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        if (currentSave == null) currentSave = new SaveData();

        // Güncel verileri save data'ya aktar
        if (GameManager.Instance != null)
        {
            currentSave.currentLevel = GameManager.Instance.currentLevel;
            currentSave.playerLives = GameManager.Instance.playerLives;
            currentSave.totalCrystals = GameManager.Instance.crystalsCollected;
        }

        string saveString = JsonUtility.ToJson(currentSave);
        PlayerPrefs.SetString(SAVE_KEY, saveString);
        PlayerPrefs.Save();

        Debug.Log("Game Saved!");
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string saveString = PlayerPrefs.GetString(SAVE_KEY);
            currentSave = JsonUtility.FromJson<SaveData>(saveString);

            // Verileri GameManager'a aktar
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentLevel = currentSave.currentLevel;
                GameManager.Instance.playerLives = currentSave.playerLives;
                GameManager.Instance.crystalsCollected = currentSave.totalCrystals;
            }

            Debug.Log("Game Loaded!");
        }
        else
        {
            currentSave = new SaveData();
            Debug.Log("No save file found, creating new save.");
        }
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        if (currentSave == null) return levelIndex == 0;
        return levelIndex < currentSave.unlockedLevels.Length && currentSave.unlockedLevels[levelIndex];
    }

    public void UnlockLevel(int levelIndex)
    {
        if (currentSave != null && levelIndex < currentSave.unlockedLevels.Length)
        {
            currentSave.unlockedLevels[levelIndex] = true;
            SaveGame();
        }
    }
}
