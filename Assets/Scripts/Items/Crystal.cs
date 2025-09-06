using UnityEngine;

public class Crystal : MonoBehaviour
{
    [Header("Crystal Settings")]
    public CrystalType crystalType = CrystalType.Red;
    public int crystalValue = 1;

    [Header("Visual Effects")]
    public float pulseSpeed = 2f;
    public float pulseScale = 0.1f;

    private Vector3 originalScale;
    public bool isCollected = false; // Bu field public yapýldý

    public enum CrystalType
    {
        Red,
        Blue,
        Yellow,
        Purple
    }

    void Start()
    {
        originalScale = transform.localScale;

        // Kristal tipine göre renk ayarla
        SetCrystalColor();
    }

    void Update()
    {
        // Kristal nabýz animasyonu
        if (!isCollected)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            transform.localScale = originalScale + Vector3.one * pulse;
        }
    }

    void SetCrystalColor()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null) return;

        switch (crystalType)
        {
            case CrystalType.Red:
                renderer.color = Color.red;
                break;
            case CrystalType.Blue:
                renderer.color = Color.blue;
                break;
            case CrystalType.Yellow:
                renderer.color = Color.yellow;
                break;
            case CrystalType.Purple:
                renderer.color = new Color(0.5f, 0f, 1f); // Mor
                break;
        }
    }

    /// <summary>
    /// Kristal toplanma iþlemi
    /// </summary>
    public void CollectCrystal(Player collector)
    {
        if (isCollected) return;

        isCollected = true;

        Debug.Log($"[Crystal] {crystalType} crystal collected by {collector.name}");

        // GameManager'a kristal toplandýðýný bildir
        if (GameManager.Instance != null)
        {
            Debug.Log($"[Crystal] Calling GameManager.CollectCrystal({crystalValue})");
            GameManager.Instance.CollectCrystal(crystalValue);
        }
        else
        {
            Debug.LogError("[Crystal] GameManager.Instance is null!");
        }

        // CrystalManager'a tipine göre bildir (yetenek sistemi için)
        if (CrystalManager.Instance != null)
        {
            CrystalManager.Instance.CollectCrystalByType(crystalType);
        }

        // Toplama efekti ve yok etme
        StartCoroutine(CollectionEffect());
    }

    System.Collections.IEnumerator CollectionEffect()
    {
        // Büyüme animasyonu
        float timer = 0f;
        float duration = 0.3f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // Büyüme ve saydamlýk
            transform.localScale = originalScale * (1f + progress);

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = 1f - progress;
                renderer.color = color;
            }

            yield return null;
        }

        // Kristali yok et
        Destroy(gameObject);
    }

    /// <summary>
    /// Trigger ile player tespiti (backup sistem)
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Crystal trigger entered by: {other.name}");

        if (isCollected) return;

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            Debug.Log("Player detected via trigger - collecting crystal");
            CollectCrystal(player);
        }
    }
}