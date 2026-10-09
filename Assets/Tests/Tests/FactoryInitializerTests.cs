using NUnit.Framework;
using UnityEngine;

public class FactoryInitializerTests
{
    private GameConfig gameConfig;
    private MachineConfig machineA;
    private MachineConfig machineB;

    [SetUp]
    public void SetUp()
    {
        gameConfig = ScriptableObject.CreateInstance<GameConfig>();
        machineA = ScriptableObject.CreateInstance<MachineConfig>();
        machineB = ScriptableObject.CreateInstance<MachineConfig>();

        machineA.MachineId = "machine_a";
        machineB.MachineId = "machine_b";

        gameConfig.Machines = new[] { machineA, machineB };
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameConfig);
        Object.DestroyImmediate(machineA);
        Object.DestroyImmediate(machineB);
    }

    [Test]
    public void CreateNewGame_UnlocksOnlyFirstMachine()
    {
        FactoryState state = FactoryInitializer.CreateNewGame(gameConfig);

        Assert.AreEqual(2, state.Machines.Count);

        Assert.IsTrue(state.Machines[0].IsUnlocked);
        Assert.AreEqual(1, state.Machines[0].Level);

        Assert.IsFalse(state.Machines[1].IsUnlocked);
        Assert.AreEqual(0, state.Machines[1].Level);
    }

    [Test]
    public void CreateNewGame_StartsWithZeroCurrency()
    {
        FactoryState state = FactoryInitializer.CreateNewGame(gameConfig);

        Assert.AreEqual(0, state.Currency);
    }
    
    [Test]
    public void ProductionService_CalculatesIncomeForElapsedTime()
    {
        machineA.BaseProductionPerSecond = 3;

        GameConfig config = gameConfig;
        FactoryState state = FactoryInitializer.CreateNewGame(config);

        EconomyService economy = new EconomyService();
        FactoryService factory = new FactoryService(config, state, economy);
        ProductionService production = new ProductionService(factory);

        double income = production.CalculateIncome(10);

        Assert.AreEqual(30, income, 0.001);
    }
}

