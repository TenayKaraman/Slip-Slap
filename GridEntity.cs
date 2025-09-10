// Scripts/GridSystem/GridEntity.cs - GridManager ile uyumlu versiyon
using UnityEngine;

public abstract class GridEntity : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2Int gridPosition;

    protected virtual void Start()
    {
        // Transform pozisyonunu grid pozisyonuna ayarla
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.GridToWorld(gridPosition);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: GridManager not found!");
        }
    }

    /// <summary>
    /// Entity'yi belirtilen y�ne hareket ettir
    /// </summary>
    public abstract void Move(Vector2Int direction);

    /// <summary>
    /// Ba�ka bir entity bu grid cell'e girdi�inde �a�r�l�r
    /// </summary>
    public virtual void OnGridEnter(GridEntity other)
    {
        Debug.Log($"{other.name} entered {gameObject.name}'s grid cell");
    }

    /// <summary>
    /// Ba�ka bir entity bu grid cell'den ��kt���nda �a�r�l�r
    /// </summary>
    public virtual void OnGridExit(GridEntity other)
    {
        Debug.Log($"{other.name} exited {gameObject.name}'s grid cell");
    }

    /// <summary>
    /// Belirtilen pozisyona hareket edebilir miyiz?
    /// </summary>
    protected virtual bool CanMoveTo(Vector2Int targetPos)
    {
        return GridManager.Instance != null && !GridManager.Instance.IsBlocked(targetPos);
    }

    /// <summary>
    /// Entity'yi grid'de yeni pozisyona ta��
    /// </summary>
    protected virtual void SetGridPosition(Vector2Int newPos)
    {
        if (GridManager.Instance != null)
        {
            Vector2Int oldPos = gridPosition;
            GridManager.Instance.MoveEntity(this, oldPos, newPos);
        }
        else
        {
            // GridManager yoksa manuel g�ncelle
            gridPosition = newPos;
            transform.position = new Vector3(newPos.x, newPos.y, 0);
        }
    }

    /// <summary>
    /// Bu entity'nin bulundu�u grid pozisyonunu g�ncelle (GridManager i�in)
    /// </summary>
    public virtual void UpdateGridPosition()
    {
        if (GridManager.Instance != null)
        {
            Vector2Int worldToGrid = GridManager.Instance.WorldToGrid(transform.position);
            if (worldToGrid != gridPosition)
            {
                SetGridPosition(worldToGrid);
            }
        }
    }
}