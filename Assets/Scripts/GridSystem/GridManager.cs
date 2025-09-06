using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Level Data")]
    public LevelData levelData;

    [Header("Scene References")]
    public Transform tilesRoot;
    public Transform entitiesRoot;

    [Header("Prefab References - Environment")]
    public GameObject wallPrefab;
    public GameObject blockPrefab;
    public GameObject doorPrefab;
    public GameObject sensorPrefab;
    public GameObject trapPrefab;
    public GameObject portalPrefab;

    [Header("Prefab References - Crystals")]
    public GameObject crystalRedPrefab;
    public GameObject crystalBluePrefab;
    public GameObject crystalYellowPrefab;
    public GameObject crystalPurplePrefab;

    [Header("Prefab References - Entities")]
    public GameObject enemyPrefab;
    public GameObject playerPrefab;

    // Grid veri yap�lar�
    [System.NonSerialized]
    public Dictionary<Vector2Int, GameObject> staticMap = new Dictionary<Vector2Int, GameObject>();
    [System.NonSerialized]
    public Dictionary<Vector2Int, GridEntity> entityMap = new Dictionary<Vector2Int, GridEntity>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (levelData != null)
        {
            BuildLevel();
        }
        else
        {
            Debug.LogError("LevelData is not assigned to GridManager!");
        }
    }

    /// <summary>
    /// Grid koordinat�n� World pozisyonuna �evirir
    /// </summary>
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        if (levelData == null) return Vector3.zero;

        Vector2 cellSize = levelData.cellSize;

        // Grid'i ekrana ortalamak i�in offset hesapla
        float offsetX = -(levelData.width - 1) * cellSize.x * 0.5f;
        float offsetY = -(levelData.height - 1) * cellSize.y * 0.5f;

        return new Vector3(
            offsetX + gridPos.x * cellSize.x,
            offsetY + gridPos.y * cellSize.y,
            0f
        );
    }

    // Debug i�in - Grid'i g�rselle�tir
    void OnDrawGizmos()
    {
        if (levelData == null) return;

        Gizmos.color = Color.gray;

        for (int x = 0; x < levelData.width; x++)
        {
            for (int y = 0; y < levelData.height; y++)
            {
                Vector3 pos = GridToWorld(new Vector2Int(x, y));
                Gizmos.DrawWireCube(pos, Vector3.one * 0.9f);
            }
        }

        // Player start ve exit'i g�ster
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(GridToWorld(levelData.playerStart), Vector3.one * 1.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GridToWorld(levelData.exitPortal), Vector3.one * 1.1f);
    }

    /// <summary>
    /// World pozisyonunu Grid koordinat�na �evirir  
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        if (levelData == null) return Vector2Int.zero;

        Vector2 cellSize = levelData.cellSize;

        float offsetX = -(levelData.width - 1) * cellSize.x * 0.5f;
        float offsetY = -(levelData.height - 1) * cellSize.y * 0.5f;

        int x = Mathf.RoundToInt((worldPos.x - offsetX) / cellSize.x);
        int y = Mathf.RoundToInt((worldPos.y - offsetY) / cellSize.y);

        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Belirtilen grid pozisyonu engellenmi� mi?
    /// </summary>
    public bool IsBlocked(Vector2Int gridPos)
    {
        // Grid s�n�rlar� kontrol�
        if (gridPos.x < 0 || gridPos.y < 0 ||
            gridPos.x >= levelData.width || gridPos.y >= levelData.height)
        {
            return true; // S�n�r d��� = engellenmi�
        }

        // Statik engeller (duvar, kap� vs.) - Kristaller engel de�il!
        if (staticMap.ContainsKey(gridPos))
        {
            GameObject obj = staticMap[gridPos];
            if (obj != null)
            {
                // E�er kristal ise engel de�il
                Crystal crystal = obj.GetComponent<Crystal>();
                if (crystal != null) return false;

                // Di�er statik objeler engel
                return true;
            }
        }

        // Hareketli engeller (blok, d��man vs.)
        if (entityMap.ContainsKey(gridPos))
        {
            return true;
        }

        return false; // Bo� alan
    }

    /// <summary>
    /// Grid pozisyonunda ne var?
    /// </summary>
    public bool HasEntityAt(Vector2Int gridPos)
    {
        return entityMap.ContainsKey(gridPos);
    }

    public GridEntity GetEntityAt(Vector2Int gridPos)
    {
        entityMap.TryGetValue(gridPos, out GridEntity entity);
        return entity;
    }

    bool IsCrystalType(LevelData.TileType type)
    {
        return type == LevelData.TileType.CrystalRed ||
               type == LevelData.TileType.CrystalBlue ||
               type == LevelData.TileType.CrystalYellow ||
               type == LevelData.TileType.CrystalPurple;
    }

    Crystal.CrystalType GetCrystalTypeEnum(LevelData.TileType type)
    {
        switch (type)
        {
            case LevelData.TileType.CrystalRed: return Crystal.CrystalType.Red;
            case LevelData.TileType.CrystalBlue: return Crystal.CrystalType.Blue;
            case LevelData.TileType.CrystalYellow: return Crystal.CrystalType.Yellow;
            case LevelData.TileType.CrystalPurple: return Crystal.CrystalType.Purple;
            default: return Crystal.CrystalType.Red;
        }
    }

    /// <summary>
    /// Entity'yi grid'de hareket ettir
    /// </summary>
    public void MoveEntity(GridEntity entity, Vector2Int from, Vector2Int to)
    {
        // Eski pozisyondan ��kar
        if (entityMap.ContainsKey(from) && entityMap[from] == entity)
        {
            entityMap.Remove(from);
        }

        // Yeni pozisyona ekle
        entity.gridPosition = to;
        entityMap[to] = entity;

        // World pozisyonunu g�ncelle
        entity.transform.position = GridToWorld(to);
    }

    /// <summary>
    /// Level'� verilerden olu�tur
    /// </summary>
    public void BuildLevel()
    {
        ClearLevel();

        Debug.Log($"Building level: {levelData.width}x{levelData.height}");

        // Statik elemanlar� yerle�tir
        foreach (var c in levelData.cells)
        {
            GameObject prefab = GetPrefabForTileType(c.type);
            if (prefab != null)
            {
                Vector3 worldPos = GridToWorld(c.pos);
                GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity, tilesRoot);
                instance.name = $"{c.type}_{c.pos.x}_{c.pos.y}";

                // KRISTAL �SE �ZEL ��LEM
                if (IsCrystalType(c.type))
                {
                    Debug.Log($"Creating crystal at {c.pos}: {c.type}");
                    Crystal crystalComponent = instance.GetComponent<Crystal>();
                    if (crystalComponent != null)
                    {
                        crystalComponent.crystalType = GetCrystalTypeEnum(c.type);
                        Debug.Log($"Crystal component assigned: {crystalComponent.crystalType}");

                        // Kristali static map'e ekle (toplama i�in gerekli)
                        staticMap[c.pos] = instance;
                    }
                    else
                    {
                        Debug.LogError($"Crystal prefab {c.type} doesn't have Crystal component!");
                    }
                }
                else
                {
                    // Di�er statik objeler (duvar, blok vs.) de static map'e eklensin
                    staticMap[c.pos] = instance;
                }
            }
        }

        // Portal'� yerle�tir
        if (portalPrefab != null)
        {
            Vector3 portalWorldPos = GridToWorld(levelData.exitPortal);
            GameObject portalInstance = Instantiate(portalPrefab, portalWorldPos, Quaternion.identity, tilesRoot);
            portalInstance.name = $"Portal_{levelData.exitPortal.x}_{levelData.exitPortal.y}";
        }

        // Player'� yerle�tir
        if (playerPrefab != null)
        {
            Vector3 playerWorldPos = GridToWorld(levelData.playerStart);
            GameObject playerInstance = Instantiate(playerPrefab, playerWorldPos, Quaternion.identity, entitiesRoot);

            // Player'� entity haritas�na ekle
            GridEntity playerEntity = playerInstance.GetComponent<GridEntity>();
            if (playerEntity != null)
            {
                playerEntity.gridPosition = levelData.playerStart;
                entityMap[levelData.playerStart] = playerEntity;

                // InputManager'a player referans�n� ver
                if (InputManager.Instance != null)
                {
                    InputManager.Instance.player = playerEntity;
                }
            }
        }

        Debug.Log($"Level built successfully! Static objects: {staticMap.Count}, Entities: {entityMap.Count}");
    }

    /// <summary>
    /// TileType i�in do�ru prefab'� getir
    /// </summary>
    GameObject GetPrefabForTileType(LevelData.TileType type)
    {
        switch (type)
        {
            case LevelData.TileType.Wall: return wallPrefab;
            case LevelData.TileType.Block: return blockPrefab;
            case LevelData.TileType.Enemy: return enemyPrefab;
            case LevelData.TileType.Door: return doorPrefab;
            case LevelData.TileType.Sensor: return sensorPrefab;
            case LevelData.TileType.CrystalRed: return crystalRedPrefab;
            case LevelData.TileType.CrystalBlue: return crystalBluePrefab;
            case LevelData.TileType.CrystalYellow: return crystalYellowPrefab;
            case LevelData.TileType.CrystalPurple: return crystalPurplePrefab;
            case LevelData.TileType.Trap: return trapPrefab;
            default: return null;
        }
    }

    /// <summary>
    /// Level'� temizle
    /// </summary>
    public void ClearLevel()
    {
        // T�m child object'leri yok et
        foreach (Transform child in tilesRoot)
        {
            DestroyImmediate(child.gameObject);
        }

        foreach (Transform child in entitiesRoot)
        {
            DestroyImmediate(child.gameObject);
        }

        // Dictionary'leri temizle
        staticMap.Clear();
        entityMap.Clear();

        Debug.Log("Level cleared.");
    }
}