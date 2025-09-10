using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game state manager - Aşama 1 Refactor
/// Mevcut kod ile uyumlu, minimal breaking changes
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private int _currentLevel = 1;
    [SerializeField] private int _playerLives = 5;
    [SerializeField] private int _crystalsCollected = 0;

    // Public properties for new code
    public int CurrentLevel => _currentLevel;
    public int PlayerLives => _playerLives;
    public int CrystalsCollected => _crystalsCollected;

    // Backward compatibility - old public fields (deprecated but working)
    [System.Obsolete("Use CurrentLevel property instead")]
    public int currentLevel
    {
        get => _currentLevel;
        set => _currentLevel = value;
    }

    [System.Obsolete("Use PlayerLives property instead")]
    public int playerLives
    {
        get => _playerLives;
        set => _playerLives = value;
    }

    [System.Obsolete("Use CrystalsCollected property instead")]
    public int crystalsCollected
    {
        get => _crystalsCollected;
        set => _crystalsCollected = value;
    }

    [Header("Settings")]
    [SerializeField] private float sceneTransitionDelay = 3f;
    [SerializeField] private bool enableDebugLogs = true;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        LogDebug("GameManager initialized successfully!");
        LogDebug($"Starting stats - Lives: {_playerLives}, Crystals: {_crystalsCollected}, Level: {_currentLevel}");

        // Delayed level start (same as before)
        Invoke("TestStartLevel", sceneTransitionDelay);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LogDebug("GameManager singleton initialized");
        }
        else
        {
            LogDebug("Duplicate GameManager destroyed");
            Destroy(gameObject);
        }
    }

    private void TestStartLevel()
    {
        StartLevel("Level_001");
    }

    public void StartLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[GameManager] Scene name cannot be null or empty");
            return;
        }

        LogDebug($"Starting level: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    public void CompleteLevel()
    {
        LogDebug($"Level {_currentLevel} completed!");

        _currentLevel++;
        NotifyUIUpdate();

        LogDebug($"Advanced to level: {_currentLevel}");
    }

    public void RestartLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LogDebug($"Restarting level: {currentScene}");
        SceneManager.LoadScene(currentScene);
    }

    public void CollectCrystal(int amount = 1)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("[GameManager] Crystal amount must be positive");
            return;
        }

        _crystalsCollected += amount;
        LogDebug($"Crystals collected: {_crystalsCollected} (+{amount})");

        NotifyUIUpdate();
    }

    public void SpendLife()
    {
        if (_playerLives <= 0)
        {
            Debug.LogWarning("[GameManager] No lives remaining");
            return;
        }

        _playerLives--;
        LogDebug($"Life lost. Lives remaining: {_playerLives}");

        NotifyUIUpdate();

        if (_playerLives <= 0)
        {
            GameOver();
        }
    }

    public void AddLife()
    {
        _playerLives++;
        LogDebug($"Life gained. Total lives: {_playerLives}");
        NotifyUIUpdate();
    }

    private void GameOver()
    {
        LogDebug("Game Over!");
        // TODO: Game over logic
    }

    private void NotifyUIUpdate()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            LogDebug("UI update requested");
        }
        else
        {
            Debug.LogWarning("[GameManager] UIManager not found - UI not updated");
        }
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[GameManager] {message}");
        }
    }

    private void OnValidate()
    {
        // Keep values within reasonable bounds
        _playerLives = Mathf.Max(0, _playerLives);
        _crystalsCollected = Mathf.Max(0, _crystalsCollected);
        _currentLevel = Mathf.Max(1, _currentLevel);
        sceneTransitionDelay = Mathf.Max(0.1f, sceneTransitionDelay);
    }
}