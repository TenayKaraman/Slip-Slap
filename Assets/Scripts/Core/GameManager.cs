using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public int currentLevel = 1;
    public int playerLives = 5;
    public int crystalsCollected = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("GameManager initialized successfully!");
        Debug.Log($"Player Lives: {playerLives}");

        // 3 saniye sonra level'i başlat (test için)
        Invoke("TestStartLevel", 3f);
    }

    void TestStartLevel()
    {
        StartLevel("Level_001");
    }

    public void StartLevel(string sceneName)
    {
        Debug.Log($"Starting level: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    public void CompleteLevel()
    {
        Debug.Log("Level Complete!");
        currentLevel++;
        // Sonraki seviye veya menüye dön
    }

    public void GameOver()
    {
        playerLives--;
        if (playerLives <= 0)
        {
            Debug.Log("Game Over!");
            // Game over UI göster
        }
        else
        {
            // Seviyeyi yeniden başlat
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void CollectCrystal(int amount = 1)
    {
        crystalsCollected += amount;
        Debug.Log($"Crystals collected: {crystalsCollected}");

        // UI'ı güncelle
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
        }
    }

    public void SpendLife()
    {
        playerLives--;
        Debug.Log($"Lives remaining: {playerLives}");

        // UI'ı güncelle
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
        }

        if (playerLives <= 0)
        {
            GameOver();
        }
    }
}