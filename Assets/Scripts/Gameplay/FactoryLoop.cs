using UnityEngine;
using System;

public class FactoryLoop : MonoBehaviour
{
    private ProductionService production;
    private BoostService boost;
    private bool isInitialized;
    private double fractionalUnixTime;

    public void Initialize(
        ProductionService productionService,
        BoostService boostService)
    {
        production = productionService;
        boost = boostService;
        isInitialized = production != null && boost != null;
        fractionalUnixTime = 0;
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        double remainingFrameTime = Time.deltaTime;

        while (remainingFrameTime > 0)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (boost.IsActive(now))
            {
                double secondsToBoostEnd =
                    boost.GetRemainingSeconds(now) - fractionalUnixTime;

                if (secondsToBoostEnd <= 0)
                    secondsToBoostEnd = remainingFrameTime;

                double boostedTime =
                    Math.Min(remainingFrameTime, secondsToBoostEnd);

                production.ApplyIncome(
                    boostedTime,
                    boost.GetMultiplier(now));

                remainingFrameTime -= boostedTime;
            }
            else
            {
                production.ApplyIncome(remainingFrameTime);
                remainingFrameTime = 0;
            }
        }

        fractionalUnixTime += Time.deltaTime;
        if (fractionalUnixTime >= 1)
            fractionalUnixTime %= 1;
    }
}
