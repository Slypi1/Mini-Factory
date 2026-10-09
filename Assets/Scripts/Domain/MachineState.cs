using System;

[Serializable]
public class MachineState
{
    public string MachineId;
    public bool IsUnlocked;
    public int Level;

    public MachineState(string machineId, bool isUnlocked, int level)
    {
        MachineId = machineId;
        IsUnlocked = isUnlocked;
        Level = level;
    }
}

