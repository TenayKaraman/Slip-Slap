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

    // Grid veri yapýlarý
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
    /// Grid koordinatýný World pozisyonuna çevirir
    /// </summary>
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        if (levelData == null) return Vector3.zero;

        Vector2 cellSize = levelData.cellSize;

        // Grid'i ekrana ortalamak için offset hesapla
        float offsetX = -(levelData.width - 1) * cellSize.x * 0.5f;
        float offsetY = -(levelData.height - 1) * cellSize.y * 0.5f;

        return new Vector3(
            offsetX + gridPos.x * cellSize.x,
            offsetY + gridPos.y * cellSize.y,
            0f
        );
    }

    // Debug için - Grid'i görselleþtir
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

        // Player start ve exit'i göster
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(GridToWorld(levelData.playerStart), Vector3.one * 1.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GridToWorld(levelData.exitPortal), Vector3.one * 1.1f);
    }

    /// <summary>
    /// World pozisyonunu Grid koordinatýna çevirir  
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
    /// Belirtilen grid pozisyonu engellenmiþ mi?
    /// </summary>
    public bool IsBlocked(Vector2Int gridPos)
    {
        // Grid sýnýrlarý kontrolü
        if (gridPos.x < 0 || gridPos.y < 0 ||
            gridPos.x >= levelData.width || gridPos.y >= levelData.height)
        {
            return true; // Sýnýr dýþý = engellenmiþ
        }

        // Statik engeller (duvar, kapý vs.) - Kristaller engel deðil!
        if (staticMap.ContainsKey(gridPos))
        {
            GameObject obj = staticMap[gridPos];
            if (obj != null)
            {
                // Eðer kristal ise engel deðil
                Crystal crystal = obj.GetComponent<Crystal>();
                if (crystal != null) return false;

                // Diðer statik objeler engel
                return true;
            }
        }

        // Hareketli engeller (blok, düþman vs.)
        if (entityMap.ContainsKey(gridPos))
        {
            return true;
        }

        return false; // Boþ alan
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
        // Eski pozisyondan çýkar
        if (entityMap.ContainsKey(from) && entityMap[from] == entity)
        {
            entityMap.Remove(from);
        }

        // Yeni pozisyona ekle
        entity.gridPosition = to;
        entityMap[to] = entity;

        // World pozisyonunu güncelle
        entity.transform.position = GridToWorld(to);
    }

    /// <summary>
    /// Level'ý verilerden oluþtur
    /// </summary>
    public void BuildLevel()
    {
        ClearLevel();

        Debug.Log($"Building level: {levelData.width}x{levelData.height}");

        // Statik elemanlarý yerleþtir
        foreach (var c in levelData.cells)
        {
            GameObject prefab = GetPrefabForTileType(c.type);
            if (prefab != null)
            {
                Vector3 worldPos = GridToWorld(c.pos);
                GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity, tilesRoot);
                instance.name = $"{c.type}_{c.pos.x}_{c.pos.y}";

                // KRISTAL ÝSE ÖZEL ÝÞLEM
                if (IsCrystalType(c.type))
                {
                    Debug.Log($"Creating crystal at {c.pos}: {c.type}");
                    Crystal crystalComponent = instance.GetComponent<Crystal>();
                    if (crystalComponent != null)
                    {
                        crystalComponent.crystalType = GetCrystalTypeEnum(c.type);
                        Debug.Log($"Crystal component assigned: {crystalComponent.crystalType}");

                        // Kristali static map'e ekle (toplama için gerekli)
                        staticMap[c.pos] = instance;
                    }
                    else
                    {
                        Debug.LogError($"Crystal prefab {c.type} doesn't have Crystal component!");
                    }
                }
                else
                {
                    // Diðer statik objeler (duvar, blok vs.) de static map'e eklensin
                    staticMap[c.pos] = instance;
                }
            }
        }

        // Portal'ý yerleþtir
        if (portalPrefab != null)
        {
            Vector3 portalWorldPos = GridToWorld(levelData.exitPortal);
            GameObject portalInstance = Instantiate(portalPrefab, portalWorldPos, Quaternion.identity, tilesRoot);
            portalInstance.name = $"Portal_{levelData.exitPortal.x}_{levelData.exitPortal.y}";
        }

        // Player'ý yerleþtir
        if (playerPrefab != null)
        {
            Vector3 playerWorldPos = GridToWorld(levelData.playerStart);
            GameObject playerInstance = Instantiate(playerPrefab, playerWorldPos, Quaternion.identity, entitiesRoot);

            // Player'ý entity haritasýna ekle
            GridEntity playerEntity = playerInstance.GetComponent<GridEntity>();
            if (playerEntity != null)
            {
                playerEntity.gridPosition = levelData.playerStart;
                entityMap[levelData.playerStart] = playerEntity;

                // InputManager'a player referansýný ver
                if (InputManager.Instance != null)
                {
                    InputManager.Instance.player = playerEntity;
                }
            }
        }

        Debug.Log($"Level built successfully! Static objects: {staticMap.Count}, Entities: {entityMap.Count}");
    }

    /// <summary>
    /// TileType için doðru prefab'ý getir
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
    /// Level'ý temizle
    /// </summary>
    public void ClearLevel()
    {
        // Tüm child object'leri yok et
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