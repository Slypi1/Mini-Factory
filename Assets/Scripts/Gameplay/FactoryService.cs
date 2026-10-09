using System;

public class FactoryService
{
    private readonly GameConfig _config;
    private readonly FactoryState _state;
    private readonly EconomyService _economy;

    public FactoryService(
        GameConfig config,
        FactoryState state,
        EconomyService economy)
    {
        this._config = config ? config : throw new ArgumentNullException(nameof(config));

        this._state = state
            ?? throw new ArgumentNullException(nameof(state));

        this._economy = economy
            ?? throw new ArgumentNullException(nameof(economy));
    }

    public bool UnlockMachine(string machineId)
    {
        MachineConfig machine = FindConfig(machineId);
        MachineState machineState = FindState(machineId);

        if (machine == null || machineState == null)
            return false;

        if (machineState.IsUnlocked || _state.Currency < machine.UnlockCost)
            return false;

        _state.Currency -= machine.UnlockCost;
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

        double cost = _economy.GetUpgradeCost(machine, machineState);

        if (_state.Currency < cost)
            return false;

        _state.Currency -= cost;
        machineState.Level++;

        return true;
    }

    public void AddCurrency(double amount)
    {
        if (amount <= 0 ||
            double.IsNaN(amount) ||
            double.IsInfinity(amount))
            return;

        _state.Currency += amount;
    }

    public double GetTotalProduction()
    {
        return _economy.GetTotalProduction(_config, _state);
    }

    private MachineConfig FindConfig(string machineId)
    {
        foreach (MachineConfig machine in _config.Machines)
        {
            if (machine != null && machine.MachineId == machineId)
                return machine;
        }

        return null;
    }

    private MachineState FindState(string machineId)
    {
        foreach (MachineState machine in _state.Machines)
        {
            if (machine.MachineId == machineId)
                return machine;
        }

        return null;
    }
}
