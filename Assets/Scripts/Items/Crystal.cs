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
    private bool isCollected = false;

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

        // Kristal tipine g�re renk ayarla
        SetCrystalColor();
    }

    void Update()
    {
        // Kristal nab�z animasyonu
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
    /// Kristal toplanma i�lemi
    /// </summary>
    public void CollectCrystal(Player collector)
    {
        if (isCollected) return;

        isCollected = true;

        Debug.Log($"[Crystal] {crystalType} crystal collected by {collector.name}");

        // GameManager'a kristal topland���n� bildir
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectCrystal(crystalValue);
        }

        // CrystalManager'a tipine g�re bildir (yetenek sistemi i�in)
        if (CrystalManager.Instance != null)
        {
            CrystalManager.Instance.CollectCrystalByType(crystalType);
        }

        // Toplama efekti ve yok etme
        StartCoroutine(CollectionEffect());
    }

    System.Collections.IEnumerator CollectionEffect()
    {
        // B�y�me animasyonu
        float timer = 0f;
        float duration = 0.3f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // B�y�me ve saydaml�k
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
    /// Trigger ile player tespiti
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Crystal trigger entered by: {other.name}");

        if (isCollected) return;

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            Debug.Log("Player detected - collecting crystal");
            CollectCrystal(player);
        }
        else
        {
            Debug.Log("Not a player object");
        }
    }
}
