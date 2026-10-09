using System;
using UnityEngine;

public class BoostService
{
    private readonly GameConfig config;
    private readonly FactoryState state;

    public BoostService(GameConfig config, FactoryState state)
    {
        this.config = config ?? throw new ArgumentNullException(nameof(config));
        this.state = state ?? throw new ArgumentNullException(nameof(state));
    }

    public bool IsActive(long currentUnixTime)
    {
        return config.BoostEnabled &&
               state.BoostEndUnixTime > currentUnixTime;
    }

    public bool TryStart(long currentUnixTime)
    {
        if (!config.BoostEnabled || IsActive(currentUnixTime))
            return false;

        state.BoostStartUnixTime = currentUnixTime;
        state.BoostEndUnixTime =
            currentUnixTime + (long)config.BoostDurationSeconds;
        
        Debug.Log(
            $"Boost started: start={state.BoostStartUnixTime}, " +
            $"end={state.BoostEndUnixTime}");

        return true;
    }

    public double GetMultiplier(long currentUnixTime)
    {
        return IsActive(currentUnixTime)
            ? config.BoostMultiplier
            : 1.0;
    }

    public long GetRemainingSeconds(long currentUnixTime)
    {
        if (!IsActive(currentUnixTime))
            return 0;

        return state.BoostEndUnixTime - currentUnixTime;
    }
}
