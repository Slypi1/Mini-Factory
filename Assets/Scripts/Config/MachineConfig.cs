using UnityEngine;

[CreateAssetMenu(
    fileName = "MachineConfig",
    menuName = "Mini Factory/Machine Config", order = -1000)]
public class MachineConfig : ScriptableObject
{
    [Header("Identity")]
    public string MachineId;
    public string DisplayName;

    [Header("Economy")]
    [Min(0)]
    public double BaseProductionPerSecond;

    [Min(0)]
    public double UnlockCost;

    [Min(0)]
    public double BaseUpgradeCost;

    [Min(1)]
    public float UpgradeCostMultiplier;

    [Min(1)]
    public float ProductionMultiplierPerLevel;

    public double GetProduction(int level)
    {
        if (level < 1)
            return 0;

        return BaseProductionPerSecond
               * System.Math.Pow(ProductionMultiplierPerLevel, level - 1);
    }

    public double GetUpgradeCost(int currentLevel)
    {
        if (currentLevel < 1)
            currentLevel = 1;

        return BaseUpgradeCost
               * System.Math.Pow(UpgradeCostMultiplier, currentLevel - 1);
    }
}

