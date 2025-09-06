using UnityEngine;
using System.Collections.Generic;

public class CrystalManager : MonoBehaviour
{
    public static CrystalManager Instance;

    [Header("Crystal Counts")]
    public int redCrystals = 0;
    public int blueCrystals = 0;
    public int yellowCrystals = 0;
    public int purpleCrystals = 0;

    [Header("Ability Requirements")]
    public int abilityRequirement = 3; // Her yetenek için gerekli kristal sayýsý

    // Yeteneklerin aktif durumu
    private Dictionary<Crystal.CrystalType, bool> unlockedAbilities = new Dictionary<Crystal.CrystalType, bool>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAbilities();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAbilities()
    {
        unlockedAbilities[Crystal.CrystalType.Red] = false;    // Iþýn Kýlýcý
        unlockedAbilities[Crystal.CrystalType.Blue] = false;   // EMP Kristali
        unlockedAbilities[Crystal.CrystalType.Yellow] = false; // Kalkan
        unlockedAbilities[Crystal.CrystalType.Purple] = false; // Faz Kaymasý
    }

    public void CollectCrystalByType(Crystal.CrystalType type)
    {
        switch (type)
        {
            case Crystal.CrystalType.Red:
                redCrystals++;
                CheckAbilityUnlock(type, redCrystals);
                break;
            case Crystal.CrystalType.Blue:
                blueCrystals++;
                CheckAbilityUnlock(type, blueCrystals);
                break;
            case Crystal.CrystalType.Yellow:
                yellowCrystals++;
                CheckAbilityUnlock(type, yellowCrystals);
                break;
            case Crystal.CrystalType.Purple:
                purpleCrystals++;
                CheckAbilityUnlock(type, purpleCrystals);
                break;
        }

        Debug.Log($"[CrystalManager] {type} crystals: {GetCrystalCount(type)}");
    }

    void CheckAbilityUnlock(Crystal.CrystalType type, int count)
    {
        if (count >= abilityRequirement && !unlockedAbilities[type])
        {
            unlockedAbilities[type] = true;
            UnlockAbility(type);
        }
    }

    void UnlockAbility(Crystal.CrystalType type)
    {
        string abilityName = GetAbilityName(type);
        Debug.Log($"[CrystalManager] ABILITY UNLOCKED: {abilityName}!");

        // UI bildirim sistemi çaðrýlabilir
        if (UIManager.Instance != null)
        {
            // UIManager'a yetenek bildirimini gönder
        }
    }

    string GetAbilityName(Crystal.CrystalType type)
    {
        switch (type)
        {
            case Crystal.CrystalType.Red: return "Iþýn Kýlýcý";
            case Crystal.CrystalType.Blue: return "EMP Kristali";
            case Crystal.CrystalType.Yellow: return "Kalkan";
            case Crystal.CrystalType.Purple: return "Faz Kaymasý";
            default: return "Unknown Ability";
        }
    }

    public int GetCrystalCount(Crystal.CrystalType type)
    {
        switch (type)
        {
            case Crystal.CrystalType.Red: return redCrystals;
            case Crystal.CrystalType.Blue: return blueCrystals;
            case Crystal.CrystalType.Yellow: return yellowCrystals;
            case Crystal.CrystalType.Purple: return purpleCrystals;
            default: return 0;
        }
    }

    public bool IsAbilityUnlocked(Crystal.CrystalType type)
    {
        return unlockedAbilities.ContainsKey(type) && unlockedAbilities[type];
    }
}