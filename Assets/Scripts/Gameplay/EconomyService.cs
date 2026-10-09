using System;

public class EconomyService
{
    public double GetProductionPerSecond(
        MachineConfig machine,
        MachineState state)
    {
        if (machine == null || state == null || !state.IsUnlocked)
            return 0;

        return machine.GetProduction(state.Level);
    }

    public double GetTotalProduction(
        GameConfig config,
        FactoryState state)
    {
        if (config == null || state == null)
            return 0;

        double total = 0;

        foreach (MachineState machineState in state.Machines)
        {
            MachineConfig configForMachine =
                FindMachineConfig(config, machineState.MachineId);

            total += GetProductionPerSecond(
                configForMachine,
                machineState);
        }

        return total;
    }

    public double GetUpgradeCost(
        MachineConfig machine,
        MachineState state)
    {
        if (machine == null || state == null || !state.IsUnlocked)
            return 0;

        return machine.GetUpgradeCost(state.Level);
    }

    private MachineConfig FindMachineConfig(
        GameConfig config,
        string machineId)
    {
        foreach (MachineConfig machine in config.Machines)
        {
            if (machine != null && machine.MachineId == machineId)
                return machine;
        }

        return null;
    }
}