using System;
using UnityEngine;

public class BoostService
{
    private readonly GameConfig _config;
    private readonly FactoryState _state;

    public BoostService(GameConfig config, FactoryState state)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _state = state ?? throw new ArgumentNullException(nameof(state));
    }

    public bool IsActive(long currentUnixTime)
    {
        return _config.BoostEnabled &&
               _state.BoostEndUnixTime > currentUnixTime;
    }

    public bool TryStart(long currentUnixTime)
    {
        if (!_config.BoostEnabled || IsActive(currentUnixTime))
            return false;

        _state.BoostStartUnixTime = currentUnixTime;
        _state.BoostEndUnixTime =
            currentUnixTime + (long)_config.BoostDurationSeconds;
        
        return true;
    }

    public double GetMultiplier(long currentUnixTime)
    {
        return IsActive(currentUnixTime)
            ? _config.BoostMultiplier
            : 1.0;
    }

    public long GetRemainingSeconds(long currentUnixTime)
    {
        if (!IsActive(currentUnixTime))
            return 0;

        return _state.BoostEndUnixTime - currentUnixTime;
    }
}
