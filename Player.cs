// Scripts/GridSystem/Player.cs - Çalışan versiyon + minimal iyileştirmeler
using System.Collections;
using UnityEngine;

public class Player : GridEntity
{
    [Header("Slide Settings")]
    [SerializeField] private float slideSpeed = 12f;

    [Header("Debug Settings")]
    [SerializeField] private bool enableMovementLogs = false;
    [SerializeField] private bool enablePickupLogs = true;

    private bool isSliding = false;
    private Vector2Int currentSlideDirection = Vector2Int.zero;
    private Coroutine activeSlideRoutine;

    public override void Move(Vector2Int direction)
    {
        // Eğer halihazırda kayıyorsa
        if (isSliding)
        {
            // Ters yönde swipe → frenleme
            if (direction == -currentSlideDirection)
            {
                LogMovement("[Player] Reverse swipe detected - stopping slide!");
                isSliding = false; // Coroutine kendi kendini bitirecek
                currentSlideDirection = Vector2Int.zero;

                // Snap hemen çağrılır, böylece pozisyon güvenli hale gelir
                SnapToNearestGrid();
                return;
            }

            LogMovement("[Player] Already sliding - ignoring input.");
            return;
        }

        // Yeni kayma başlat
        if (activeSlideRoutine != null)
        {
            StopCoroutine(activeSlideRoutine);
        }
        activeSlideRoutine = StartCoroutine(SlideCoroutine(direction));
    }

    private IEnumerator SlideCoroutine(Vector2Int direction)
    {
        isSliding = true;
        currentSlideDirection = direction;

        LogMovement($"[Player] === SLIDE START === Direction: {direction}, Grid: {gridPosition}, World: {transform.position}");

        // Her grid hücresini tek tek kayma sistemi
        while (isSliding)
        {
            Vector2Int nextGridPos = gridPosition + direction;

            // Sonraki pozisyon engellenmiş mi kontrol et
            if (!CanMoveTo(nextGridPos))
            {
                LogMovement($"[Player] Blocked at {nextGridPos} - slide finished");
                break;
            }

            // Entity map'ten eski pozisyonu kaldır
            if (GridManager.Instance.entityMap.ContainsKey(gridPosition))
            {
                GridManager.Instance.entityMap.Remove(gridPosition);
            }

            // Yeni grid pozisyonuna animasyonla git
            Vector3 startWorldPos = transform.position;
            Vector3 targetWorldPos = GridManager.Instance.GridToWorld(nextGridPos);

            float journey = 0f;
            float cellDistance = Vector3.Distance(startWorldPos, targetWorldPos);
            float cellMoveTime = cellDistance / slideSpeed;

            // Tek hücre boyunca animasyon
            while (journey < cellMoveTime && isSliding)
            {
                journey += Time.deltaTime;
                float progress = journey / cellMoveTime;
                transform.position = Vector3.Lerp(startWorldPos, targetWorldPos, progress);
                yield return null;
            }

            // Grid pozisyonunu güncelle - BU SATIRLAR ÇOK KRİTİK
            if (isSliding)
            {
                transform.position = targetWorldPos; // EXACT pozisyon
                gridPosition = nextGridPos; // Grid koordinat güncelle
                GridManager.Instance.entityMap[gridPosition] = this; // Map'e ekle

                // Her grid hücresine geldiğinde pickup kontrolü yap
                CheckForPickups();

                LogMovement($"[Player] Moved to grid {gridPosition}");
            }
        }

        // Kayma tamamlandı
        LogMovement($"[Player] === SLIDE END === Final Grid: {gridPosition}");
        isSliding = false;
        currentSlideDirection = Vector2Int.zero;
        activeSlideRoutine = null;
    }

    /// <summary>
    /// Mevcut pozisyonda kristal/pickup kontrolü yap
    /// </summary>
    private void CheckForPickups()
    {
        // Kristal kontrolü
        if (GridManager.Instance.staticMap.ContainsKey(gridPosition))
        {
            GameObject obj = GridManager.Instance.staticMap[gridPosition];
            Crystal crystal = obj?.GetComponent<Crystal>();

            if (crystal != null && !crystal.isCollected)
            {
                LogPickup($"[Player] Crystal found at {gridPosition} - collecting!");
                crystal.CollectCrystal(this);
                GridManager.Instance.staticMap.Remove(gridPosition);
            }
        }

        // Portal kontrolü (level completion)
        CheckForPortal();
    }

    /// <summary>
    /// Portal'a ulaşma kontrolü
    /// </summary>
    private void CheckForPortal()
    {
        if (GridManager.Instance.levelData != null &&
            gridPosition == GridManager.Instance.levelData.exitPortal)
        {
            LogPickup("[Player] Portal reached - Level Complete!");

            // Level tamamlama işlemi
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel();
            }
        }
    }

    /// <summary>
    /// Player'ı en yakın grid hücresine hizalar.
    /// Frenleme veya ani kesintiler sonrası pozisyon senkronunu düzeltir.
    /// </summary>
    private void SnapToNearestGrid()
    {
        if (GridManager.Instance == null) return;

        // World pozisyonunu en yakın grid'e çevir
        Vector2Int nearestGrid = GridManager.Instance.WorldToGrid(transform.position);

        // EntityMap güncelle
        if (GridManager.Instance.entityMap.ContainsKey(gridPosition))
        {
            GridManager.Instance.entityMap.Remove(gridPosition);
        }

        gridPosition = nearestGrid;
        transform.position = GridManager.Instance.GridToWorld(nearestGrid);
        GridManager.Instance.entityMap[gridPosition] = this;

        // Snap sonrası da pickup kontrolü yap
        CheckForPickups();

        LogMovement($"[Player] Snapped to Grid: {gridPosition}, World: {transform.position}");
    }

    protected override void Start()
    {
        base.Start();

        // Başlangıçta da pickup kontrolü yap
        CheckForPickups();

        LogMovement($"[Player] Initialized at Grid: {gridPosition}, World: {transform.position}");
    }

    // Utility methods
    private void LogMovement(string message)
    {
        if (enableMovementLogs)
        {
            Debug.Log(message);
        }
    }

    private void LogPickup(string message)
    {
        if (enablePickupLogs)
        {
            Debug.Log(message);
        }
    }

    // Public state access
    public bool IsSliding => isSliding;
    public Vector2Int SlideDirection => isSliding ? currentSlideDirection : Vector2Int.zero;
}