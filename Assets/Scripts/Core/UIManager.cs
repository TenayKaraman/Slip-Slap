using UnityEngine;
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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // UI referanslarýný bul (eðer scene'de atanmamýþsa)
        FindUIReferences();
        UpdateUI();
    }

    void FindUIReferences()
    {
        // Eðer inspector'dan atanmamýþsa, scene'de bul
        if (livesText == null)
        {
            GameObject livesObj = GameObject.Find("LivesText");
            if (livesObj != null) livesText = livesObj.GetComponent<TextMeshProUGUI>();
        }

        if (crystalsText == null)
        {
            GameObject crystalsObj = GameObject.Find("CrystalsText");
            if (crystalsObj != null) crystalsText = crystalsObj.GetComponent<TextMeshProUGUI>();
        }

        if (levelText == null)
        {
            GameObject levelObj = GameObject.Find("LevelText");
            if (levelObj != null) levelText = levelObj.GetComponent<TextMeshProUGUI>();
        }
    }

    public void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        if (livesText != null)
        {
            livesText.text = "Lives: " + GameManager.Instance.playerLives;
        }

        if (crystalsText != null)
        {
            crystalsText.text = "Crystals: " + GameManager.Instance.crystalsCollected;
        }

        if (levelText != null)
        {
            levelText.text = "Level: " + GameManager.Instance.currentLevel;
        }
    }

    void OnLevelWasLoaded(int level)
    {
        // Yeni scene yüklendiðinde UI referanslarýný tekrar bul
        Invoke("DelayedRefresh", 0.1f);
    }

    void DelayedRefresh()
    {
        FindUIReferences();
        UpdateUI();
    }
}