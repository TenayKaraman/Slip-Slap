using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// UIManager - Aþama 2 Refactor
/// Fixes scene transition UI reference issues
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI crystalsText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Settings")]
    [SerializeField] private float referenceRefreshDelay = 0.1f;
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool forceAutoFind = true; // Her scene'de otomatik bul

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        LogDebug("UIManager starting - Finding UI references...");
        FindUIReferences();
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            LogDebug("UIManager singleton initialized");
        }
        else
        {
            Debug.LogWarning("[UIManager] Duplicate instance destroyed");
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LogDebug($"Scene loaded: {scene.name} - Refreshing UI references");

        // Her scene deðiþikliðinde referanslarý yenile
        ClearReferences();
        Invoke(nameof(DelayedRefresh), referenceRefreshDelay);
    }

    private void ClearReferences()
    {
        if (forceAutoFind)
        {
            livesText = null;
            crystalsText = null;
            levelText = null;
            LogDebug("UI references cleared for auto-find");
        }
    }

    private void FindUIReferences()
    {
        LogDebug("Finding UI references...");

        // Lives Text
        if (livesText == null)
        {
            livesText = FindUIComponent<TextMeshProUGUI>("LivesText");
            LogDebug($"LivesText found: {livesText != null}");
        }

        // Crystals Text
        if (crystalsText == null)
        {
            crystalsText = FindUIComponent<TextMeshProUGUI>("CrystalsText");
            LogDebug($"CrystalsText found: {crystalsText != null}");
        }

        // Level Text  
        if (levelText == null)
        {
            levelText = FindUIComponent<TextMeshProUGUI>("LevelText");
            LogDebug($"LevelText found: {levelText != null}");
        }

        LogDebug($"UI References Status: Lives={livesText != null}, Crystals={crystalsText != null}, Level={levelText != null}");
    }

    private T FindUIComponent<T>(string objectName) where T : Component
    {
        // Önce GameObject.Find ile dene
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            T component = obj.GetComponent<T>();
            if (component != null)
            {
                LogDebug($"Found {objectName} component via GameObject.Find");
                return component;
            }
        }

        // Bulamazsa FindObjectOfType ile tüm sahnede ara
        T[] allComponents = FindObjectsOfType<T>();
        foreach (T comp in allComponents)
        {
            if (comp.gameObject.name.Contains(objectName.Replace("Text", "")))
            {
                LogDebug($"Found {objectName} component via FindObjectsOfType fallback");
                return comp;
            }
        }

        Debug.LogWarning($"[UIManager] Could not find {objectName} in scene");
        return null;
    }

    public void UpdateUI()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[UIManager] GameManager.Instance is null - cannot update UI");
            return;
        }

        // Yeni property-based access kullan
        int lives = GameManager.Instance.PlayerLives;
        int crystals = GameManager.Instance.CrystalsCollected;
        int level = GameManager.Instance.CurrentLevel;

        LogDebug($"Updating UI - Lives: {lives}, Crystals: {crystals}, Level: {level}");

        UpdateLivesText(lives);
        UpdateCrystalsText(crystals);
        UpdateLevelText(level);
    }

    private void UpdateLivesText(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
            LogDebug($"Updated livesText: {livesText.text}");
        }
        else
        {
            Debug.LogError("[UIManager] livesText is null - cannot update lives display");
        }
    }

    private void UpdateCrystalsText(int crystals)
    {
        if (crystalsText != null)
        {
            crystalsText.text = $"Crystals: {crystals}";
            LogDebug($"Updated crystalsText: {crystals}");
        }
        else
        {
            Debug.LogError("[UIManager] crystalsText is null - cannot update crystals display");
        }
    }

    private void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {level}";
            LogDebug($"Updated levelText: {levelText.text}");
        }
        else
        {
            Debug.LogError("[UIManager] levelText is null - cannot update level display");
        }
    }

    private void DelayedRefresh()
    {
        LogDebug("Delayed refresh starting...");
        FindUIReferences();
        UpdateUI();
    }

    // Manual assignment support
    public void SetUIReferences(TextMeshProUGUI lives, TextMeshProUGUI crystals, TextMeshProUGUI level)
    {
        livesText = lives;
        crystalsText = crystals;
        levelText = level;
        LogDebug("UI references manually assigned");
        UpdateUI();
    }

    public void RefreshReferences()
    {
        ClearReferences();
        FindUIReferences();
        UpdateUI();
        LogDebug("References manually refreshed");
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[UIManager] {message}");
        }
    }

    private void OnValidate()
    {
        referenceRefreshDelay = Mathf.Max(0.05f, referenceRefreshDelay);
    }
}