using UnityEngine;

[CreateAssetMenu(
    fileName = "MachineConfig",
    menuName = "Mini Factory/Machine Config")]
public class MachineConfig : ScriptableObject
{
    [Header("Identity")]
    public string MachineId;
    public string DisplayName;

    [Header("Economy")]
    [Min(0)]
    public double BaseProductionPerSecond = 1;

    [Min(0)]
    public double UnlockCost = 10;

    [Min(0)]
    public double BaseUpgradeCost = 25;

    [Min(1)]
    public float UpgradeCostMultiplier = 1.5f;

    [Min(1)]
    public float ProductionMultiplierPerLevel = 2f;

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

