using System;
using UnityEngine;

public class OfflineProgressService
{
    private readonly GameConfig _config;
    private readonly FactoryService _factory;

    public OfflineProgressService(
        GameConfig config,
        FactoryService factory)
    {
        this._config = config ? config : throw new ArgumentNullException(nameof(config));

        this._factory = factory
                       ?? throw new ArgumentNullException(nameof(factory));
    }

    public double ApplyOfflineProgress(FactoryState state, long currentUnixTime)
    {
        if (state == null || currentUnixTime <= state.LastSaveUnixTime)
            return 0;

        double elapsedSeconds = currentUnixTime - state.LastSaveUnixTime;
        elapsedSeconds = Math.Min(
            elapsedSeconds,
            Math.Max(0, _config.MaxOfflineDurationSeconds));

        if (elapsedSeconds <= 0)
        {
            state.LastSaveUnixTime = currentUnixTime;
            return 0;
        }

       
        double offlineStartTime = currentUnixTime - elapsedSeconds;

       
        double boostStart = state.BoostStartUnixTime;
        double boostEnd = state.BoostEndUnixTime;

        double boostSeconds = 0;

        if (_config.BoostEnabled && boostEnd > boostStart)
        {
            boostSeconds = Math.Max(
                0,
                Math.Min(currentUnixTime, boostEnd)
                - Math.Max(offlineStartTime, boostStart));

            boostSeconds = Math.Min(boostSeconds, elapsedSeconds);
        }

        double normalSeconds = elapsedSeconds - boostSeconds;
        double production = _factory.GetTotalProduction();

        double income = production * (
            normalSeconds + boostSeconds * _config.BoostMultiplier);
      

        _factory.AddCurrency(income);

       
        state.LastSaveUnixTime = currentUnixTime;

        return income;
    }
}