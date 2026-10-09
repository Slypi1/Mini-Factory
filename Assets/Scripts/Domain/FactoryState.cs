using System;
using System.Collections.Generic;

[Serializable]
public class FactoryState
{
    public double Currency;
    public List<MachineState> Machines = new List<MachineState>();

    public long LastSaveUnixTime;
    public long BoostStartUnixTime;
    public long BoostEndUnixTime;

    public FactoryState()
    {
        Currency = 0;
        LastSaveUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        BoostStartUnixTime = 0;
        BoostEndUnixTime = 0;
    }
}