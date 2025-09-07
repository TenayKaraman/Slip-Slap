using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI crystalsText;
    public TextMeshProUGUI levelText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // SceneManager event'ine subscribe ol
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("[UIManager] Starting - Finding UI references...");
        // UI referanslarýný bul (eðer scene'de atanmamýþsa)
        FindUIReferences();
        UpdateUI();
    }

    void OnDestroy()
    {
        // Event'ten unsubscribe ol (memory leak önlemek için)
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // Yeni SceneManager event handler
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[UIManager] Scene loaded: {scene.name} - Refreshing UI references");

        // Eski referanslarý temizle
        livesText = null;
        crystalsText = null;
        levelText = null;

        StartCoroutine(RefreshNextFrame());
    }

    System.Collections.IEnumerator RefreshNextFrame()
    {
        yield return null; // 1 frame bekle
        FindUIReferences();
        UpdateUI();
    }


    void FindUIReferences()
    {
        Debug.Log("[UIManager] Finding UI references...");

        // Eðer inspector'dan atanmamýþsa, scene'de bul
        if (livesText == null)
        {
            GameObject livesObj = GameObject.Find("LivesText");
            if (livesObj != null)
            {
                livesText = livesObj.GetComponent<TextMeshProUGUI>();
                Debug.Log("[UIManager] Found LivesText component");
            }
            else
            {
                Debug.LogWarning("[UIManager] LivesText GameObject not found in scene!");
            }
        }

        if (crystalsText == null)
        {
            GameObject crystalsObj = GameObject.Find("CrystalsText");
            if (crystalsObj != null)
            {
                crystalsText = crystalsObj.GetComponent<TextMeshProUGUI>();
                Debug.Log("[UIManager] Found CrystalsText component");
            }
            else
            {
                Debug.LogWarning("[UIManager] CrystalsText GameObject not found in scene!");
            }
        }

        if (levelText == null)
        {
            GameObject levelObj = GameObject.Find("LevelText");
            if (levelObj != null)
            {
                levelText = levelObj.GetComponent<TextMeshProUGUI>();
                Debug.Log("[UIManager] Found LevelText component");
            }
            else
            {
                Debug.LogWarning("[UIManager] LevelText GameObject not found in scene!");
            }
        }

        // Referanslarýn durumu
        Debug.Log($"[UIManager] UI References Status: Lives={livesText != null}, Crystals={crystalsText != null}, Level={levelText != null}");
    }

    public void UpdateUI()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[UIManager] GameManager.Instance is null - cannot update UI");
            return;
        }

        Debug.Log($"[UIManager] Updating UI - Lives: {GameManager.Instance.playerLives}, Crystals: {GameManager.Instance.crystalsCollected}, Level: {GameManager.Instance.currentLevel}");

        if (livesText != null)
        {
            livesText.text = "Lives: " + GameManager.Instance.playerLives;
            Debug.Log($"[UIManager] Updated livesText: {livesText.text}");
        }
        else
        {
            Debug.LogError("[UIManager] livesText is null - cannot update lives display");
        }

        if (crystalsText != null)
        {
            crystalsText.text = "Crystals: " + GameManager.Instance.crystalsCollected;
            Debug.Log($"[UIManager] Updated crystalsText: {crystalsText.text}");
        }
        else
        {
            Debug.LogError("[UIManager] crystalsText is null - cannot update crystals display");
        }

        if (levelText != null)
        {
            levelText.text = "Level: " + GameManager.Instance.currentLevel;
            Debug.Log($"[UIManager] Updated levelText: {levelText.text}");
        }
        else
        {
            Debug.LogError("[UIManager] levelText is null - cannot update level display");
        }
    }

    void DelayedRefresh()
    {
        Debug.Log("[UIManager] Delayed refresh starting...");
        FindUIReferences();
        UpdateUI();
    }
}