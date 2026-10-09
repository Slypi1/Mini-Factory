using System;

public static class FactoryInitializer
{
    public static FactoryState CreateNewGame(GameConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        FactoryState state = new FactoryState();

        foreach (MachineConfig machine in config.Machines)
        {
            if (machine == null)
                continue;

            bool isFirstMachine = state.Machines.Count == 0;

            state.Machines.Add(
                new MachineState(
                    machine.MachineId,
                    isFirstMachine,
                    isFirstMachine ? 1 : 0));
        }

        return state;
    }
}