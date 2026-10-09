using UnityEngine;
using System;

public class FactoryLoop : MonoBehaviour
{
    private ProductionService _production;
    private BoostService _boost;
    private bool _isInitialized;
    private double _fractionalUnixTime;

    public void Initialize(
        ProductionService productionService,
        BoostService boostService)
    {
        _production = productionService;
        _boost = boostService;
        _isInitialized = _production != null && _boost != null;
        _fractionalUnixTime = 0;
    }

    private void Update()
    {
        if (!_isInitialized)
            return;

        double remainingFrameTime = Time.deltaTime;

        while (remainingFrameTime > 0)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (_boost.IsActive(now))
            {
                double secondsToBoostEnd =
                    _boost.GetRemainingSeconds(now) - _fractionalUnixTime;

                if (secondsToBoostEnd <= 0)
                    secondsToBoostEnd = remainingFrameTime;

                double boostedTime =
                    Math.Min(remainingFrameTime, secondsToBoostEnd);

                _production.ApplyIncome(
                    boostedTime,
                    _boost.GetMultiplier(now));

                remainingFrameTime -= boostedTime;
            }
            else
            {
                _production.ApplyIncome(remainingFrameTime);
                remainingFrameTime = 0;
            }
        }

        _fractionalUnixTime += Time.deltaTime;
        if (_fractionalUnixTime >= 1)
            _fractionalUnixTime %= 1;
    }
}
