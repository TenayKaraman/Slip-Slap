// Scripts/GridSystem/Player.cs - Revize Kayma Mekaniği
using System.Collections;
using UnityEngine;

public class Player : GridEntity
{
    [Header("Slide Settings")]
    [SerializeField] private float slideSpeed = 12f;

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
                Debug.Log("[Player] Reverse swipe detected - stopping slide!");
                isSliding = false; // Coroutine kendi kendini bitirecek
                currentSlideDirection = Vector2Int.zero;

                // Snap hemen çağrılır, böylece pozisyon güvenli hale gelir
                SnapToNearestGrid();
                return;
            }

            Debug.Log("[Player] Already sliding - ignoring input.");
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

        Debug.Log($"[Player] === SLIDE START === Direction: {direction}, Grid: {gridPosition}, World: {transform.position}");

        // Engele çarpana kadar hedef grid pozisyonunu bul
        Vector2Int nextPos = gridPosition + direction;
        Vector2Int finalPos = gridPosition;

        while (CanMoveTo(nextPos) && isSliding)
        {
            finalPos = nextPos;
            nextPos += direction;
            yield return null;
        }

        // Hareket yoksa çık
        if (finalPos == gridPosition)
        {
            Debug.Log("[Player] No movement possible - blocked!");
            isSliding = false;
            currentSlideDirection = Vector2Int.zero;
            yield break;
        }

        // Entity map'ten eski pozisyonu kaldır
        if (GridManager.Instance.entityMap.ContainsKey(gridPosition))
        {
            GridManager.Instance.entityMap.Remove(gridPosition);
        }

        // Animasyon ile kaydır
        Vector3 startWorldPos = transform.position;
        Vector3 targetWorldPos = GridManager.Instance.GridToWorld(finalPos);

        float journey = 0f;
        float totalDistance = Vector3.Distance(startWorldPos, targetWorldPos);

        while (journey < totalDistance && isSliding)
        {
            journey += slideSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(startWorldPos, targetWorldPos, journey / totalDistance);
            yield return null;
        }

        // Kayma tamamlama
        if (isSliding)
        {
            // Normal kayma sonu
            transform.position = targetWorldPos;
            gridPosition = finalPos;
            GridManager.Instance.entityMap[gridPosition] = this;

            Debug.Log($"[Player] === SLIDE END === Grid: {gridPosition}, World: {transform.position}");
        }
        else
        {
            // Frenleme → snap
            SnapToNearestGrid();
            Debug.Log("[Player] Slide interrupted - snapped to grid.");
        }

        isSliding = false;
        currentSlideDirection = Vector2Int.zero;
        activeSlideRoutine = null;
    }

    /// <summary>
    /// Player'ı en yakın grid hücresine hizalar.
    /// Frenleme veya ani kesintiler sonrası pozisyon senkronunu düzeltir.
    /// </summary>
    private void SnapToNearestGrid()
    {
        if (GridManager.Instance == null) return;

        // World pozisyonunu en yakın grid’e çevir
        Vector2Int nearestGrid = GridManager.Instance.WorldToGrid(transform.position);

        // EntityMap güncelle
        if (GridManager.Instance.entityMap.ContainsKey(gridPosition))
        {
            GridManager.Instance.entityMap.Remove(gridPosition);
        }

        gridPosition = nearestGrid;
        transform.position = GridManager.Instance.GridToWorld(nearestGrid);
        GridManager.Instance.entityMap[gridPosition] = this;

        Debug.Log($"[Player] Snapped to Grid: {gridPosition}, World: {transform.position}");
    }

    protected override void Start()
    {
        base.Start();
        Debug.Log($"[Player] Initialized at Grid: {gridPosition}, World: {transform.position}");
    }
}
