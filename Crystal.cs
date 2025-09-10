using UnityEngine;
using System.Collections;
using static Crystal;

/// <summary>
/// Crystal - Aþama 4 Refactor
/// Enhanced visual effects, performance optimization, and better debugging
/// </summary>
public class Crystal : MonoBehaviour
{
    public enum CrystalType
    {
        Red,    // Iþýn Kýlýcý
        Blue,   // EMP Kristali
        Yellow, // Kalkan
        Purple  // Faz Kaymasý
    }

    [Header("Crystal Configuration")]
    [SerializeField] private CrystalType crystalType = CrystalType.Red;
    [SerializeField] private int crystalValue = 1;

    [Header("Visual Effects")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseScale = 0.1f;
    [SerializeField] private float collectionDuration = 0.3f;
    [SerializeField] private bool enableGlowEffect = true;

    [Header("Debug Settings")]
    [SerializeField] private bool enableCollectionLogs = true;
    [SerializeField] private bool enableVisualLogs = false;

    // Public state
    public bool isCollected { get; private set; } = false;

    // Cached components
    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private Collider2D crystalCollider;
    private Color originalColor;

    // Animation state
    private bool isAnimating = false;
    private float pulseTimer = 0f;

    private void Awake()
    {
        CacheComponents();
    }

    private void Start()
    {
        InitializeCrystal();
    }

    private void Update()
    {
        if (!isCollected && !isAnimating)
        {
            UpdatePulseAnimation();
        }
    }

    private void OnValidate()
    {
        // Clamp values in editor
        crystalValue = Mathf.Max(1, crystalValue);
        pulseSpeed = Mathf.Max(0.1f, pulseSpeed);
        pulseScale = Mathf.Clamp(pulseScale, 0f, 1f);
        collectionDuration = Mathf.Clamp(collectionDuration, 0.1f, 2f);
    }

    // INITIALIZATION
    private void CacheComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        crystalCollider = GetComponent<Collider2D>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"[Crystal] {gameObject.name} missing SpriteRenderer component!");
        }

        if (crystalCollider == null)
        {
            LogVisual($"[Crystal] {gameObject.name} missing Collider2D - trigger collection won't work");
        }
    }

    private void InitializeCrystal()
    {
        originalScale = transform.localScale;
        SetCrystalVisuals();

        LogCollection($"Crystal initialized: Type={crystalType}, Value={crystalValue}");
    }

    private void SetCrystalVisuals()
    {
        if (spriteRenderer == null) return;

        Color crystalColor = GetCrystalColor();
        spriteRenderer.color = crystalColor;
        originalColor = crystalColor;

        LogVisual($"Crystal color set to {crystalColor} for type {crystalType}");
    }

    private Color GetCrystalColor()
    {
        return crystalType switch
        {
            CrystalType.Red => Color.red,
            CrystalType.Blue => Color.blue,
            CrystalType.Yellow => Color.yellow,
            CrystalType.Purple => new Color(0.5f, 0f, 1f),
            _ => Color.white
        };
    }

    // ANIMATION SYSTEM
    private void UpdatePulseAnimation()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;
        float pulse = Mathf.Sin(pulseTimer) * pulseScale;
        transform.localScale = originalScale + Vector3.one * pulse;

        // Glow effect
        if (enableGlowEffect && spriteRenderer != null)
        {
            float glowIntensity = (Mathf.Sin(pulseTimer * 1.5f) + 1f) * 0.5f; // 0-1 range
            Color glowColor = Color.Lerp(originalColor, Color.white, glowIntensity * 0.3f);
            spriteRenderer.color = glowColor;
        }
    }

    private void StopAnimation()
    {
        isAnimating = true;
        transform.localScale = originalScale;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // COLLECTION SYSTEM
    public void CollectCrystal(Player collector)
    {
        if (collector == null)
        {
            Debug.LogError("[Crystal] Cannot collect crystal - collector is null");
            return;
        }

        if (isCollected)
        {
            LogCollection("Crystal already collected - ignoring");
            return;
        }

        PerformCollection(collector);
    }

    private void PerformCollection(Player collector)
    {
        isCollected = true;
        StopAnimation();

        LogCollection($"{crystalType} crystal collected by {collector.name}");

        // Disable collider immediately
        if (crystalCollider != null)
        {
            crystalCollider.enabled = false;
        }

        // Notify game systems
        NotifyGameSystems();

        // Start collection effect
        StartCoroutine(CollectionEffectCoroutine());
    }

    private void NotifyGameSystems()
    {
        // Notify GameManager
        if (GameManager.Instance != null)
        {
            LogCollection($"Notifying GameManager - adding {crystalValue} crystals");
            GameManager.Instance.CollectCrystal(crystalValue);
        }
        else
        {
            Debug.LogError("[Crystal] GameManager.Instance is null!");
        }

        // Notify CrystalManager
        if (CrystalManager.Instance != null)
        {
            CrystalManager.Instance.CollectCrystalByType(crystalType);
        }
        else
        {
            LogCollection("CrystalManager not found - ability progression won't work");
        }
    }

    // COLLECTION EFFECTS
    private IEnumerator CollectionEffectCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 startScale = originalScale;
        Color startColor = originalColor;

        LogVisual("Starting collection effect animation");

        // Collection animation
        while (elapsedTime < collectionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / collectionDuration;

            // Smooth easing curve
            float easedProgress = EaseOutCubic(progress);

            UpdateCollectionEffect(easedProgress, startScale, startColor);
            yield return null;
        }

        // Destroy crystal
        LogVisual("Collection effect complete - destroying crystal");
        DestroyCrystal();
    }

    private void UpdateCollectionEffect(float progress, Vector3 startScale, Color startColor)
    {
        // Scale up effect
        float scaleMultiplier = 1f + (progress * 0.5f); // Grow by 50%
        transform.localScale = startScale * scaleMultiplier;

        // Fade out effect
        if (spriteRenderer != null)
        {
            Color currentColor = startColor;
            currentColor.a = 1f - progress;
            spriteRenderer.color = currentColor;
        }

        // Optional spin effect
        if (enableGlowEffect)
        {
            float rotation = progress * 180f; // Half rotation during collection
            transform.rotation = Quaternion.Euler(0, 0, rotation);
        }
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private void DestroyCrystal()
    {
        LogCollection($"{crystalType} crystal destroyed after collection");
        Destroy(gameObject);
    }

    // TRIGGER COLLECTION (BACKUP SYSTEM)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        LogCollection($"Trigger entered by: {other.name}");

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            LogCollection("Player detected via trigger - collecting crystal");
            CollectCrystal(player);
        }
    }

    // UTILITY METHODS
    public string GetAbilityName()
    {
        return _crystalType switch
        {
            CrystalType.Red => "Iþýn Kýlýcý",
            CrystalType.Blue => "EMP Kristali",
            CrystalType.Yellow => "Kalkan",
            CrystalType.Purple => "Faz Kaymasý",
            _ => "Unknown Ability"
        };
    }

    public CrystalType GetCrystalType() => _crystalType;
    public int GetValue() => crystalValue;
    public bool IsCollected() => isCollected;

    // LOGGING
    private void LogCollection(string message)
    {
        if (enableCollectionLogs)
        {
            Debug.Log($"[Crystal] {message}");
        }
    }

    private void LogVisual(string message)
    {
        if (enableVisualLogs)
        {
            Debug.Log($"[Crystal] {message}");
        }
    }

    // EDITOR SUPPORT
    [ContextMenu("Test Collection")]
    private void EditorTestCollection()
    {
        if (Application.isPlaying)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                CollectCrystal(player);
            }
            else
            {
                Debug.LogWarning("No player found for test collection");
            }
        }
        else
        {
            Debug.LogWarning("Test collection only works in play mode");
        }
    }

    [ContextMenu("Reset Crystal")]
    private void EditorResetCrystal()
    {
        if (Application.isPlaying)
        {
            isCollected = false;
            isAnimating = false;

            if (crystalCollider != null)
                crystalCollider.enabled = true;

            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;

            transform.localScale = originalScale;
            transform.rotation = Quaternion.identity;

            LogCollection("Crystal reset to initial state");
        }
    }
}