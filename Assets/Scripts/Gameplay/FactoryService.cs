using System;

public class FactoryService
{
    private readonly GameConfig config;
    private readonly FactoryState state;
    private readonly EconomyService economy;

    public FactoryService(
        GameConfig config,
        FactoryState state,
        EconomyService economy)
    {
        this.config = config ? config : throw new ArgumentNullException(nameof(config));

        this.state = state
            ?? throw new ArgumentNullException(nameof(state));

        this.economy = economy
            ?? throw new ArgumentNullException(nameof(economy));
    }

    public bool UnlockMachine(string machineId)
    {
        MachineConfig machine = FindConfig(machineId);
        MachineState machineState = FindState(machineId);

        if (machine == null || machineState == null)
            return false;

        if (machineState.IsUnlocked || state.Currency < machine.UnlockCost)
            return false;

        state.Currency -= machine.UnlockCost;
        machineState.IsUnlocked = true;
        machineState.Level = 1;

        return true;
    }

    public bool UpgradeMachine(string machineId)
    {
        MachineConfig machine = FindConfig(machineId);
        MachineState machineState = FindState(machineId);

        if (machine == null || machineState == null ||
            !machineState.IsUnlocked)
            return false;

        double cost = economy.GetUpgradeCost(machine, machineState);

        if (state.Currency < cost)
            return false;

        state.Currency -= cost;
        machineState.Level++;

        return true;
    }

    public void AddCurrency(double amount)
    {
        if (amount <= 0 ||
            double.IsNaN(amount) ||
            double.IsInfinity(amount))
            return;

        state.Currency += amount;
    }

    public double GetTotalProduction()
    {
        return economy.GetTotalProduction(config, state);
    }

    private MachineConfig FindConfig(string machineId)
    {
        foreach (MachineConfig machine in config.Machines)
        {
            if (machine != null && machine.MachineId == machineId)
                return machine;
        }

        return null;
    }

    private MachineState FindState(string machineId)
    {
        foreach (MachineState machine in state.Machines)
        {
            if (machine.MachineId == machineId)
                return machine;
        }

        return null;
    }
}
