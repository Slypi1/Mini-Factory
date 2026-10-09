using NUnit.Framework;
using UnityEngine;

public class BoostServiceTests
{
    private GameConfig config;
    private FactoryState state;
    private BoostService boost;

    [SetUp]
    public void SetUp()
    {
        config = ScriptableObject.CreateInstance<GameConfig>();
        config.BoostEnabled = true;
        config.BoostMultiplier = 2f;
        config.BoostDurationSeconds = 60f;

        state = new FactoryState();
        boost = new BoostService(config, state);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(config);
    }

    [Test]
    public void TryStart_ActivatesBoostForConfiguredDuration()
    {
        bool started = boost.TryStart(1000);

        Assert.IsTrue(started);
        Assert.IsTrue(boost.IsActive(1000));
        Assert.AreEqual(60, boost.GetRemainingSeconds(1000));
        Assert.AreEqual(2.0, boost.GetMultiplier(1000));
    }

    [Test]
    public void Boost_ExpiresAfterDuration()
    {
        boost.TryStart(1000);

        Assert.IsFalse(boost.IsActive(1060));
        Assert.AreEqual(1.0, boost.GetMultiplier(1060));
    }

    [Test]
    public void TryStart_DoesNotRestartActiveBoost()
    {
        boost.TryStart(1000);

        Assert.IsFalse(boost.TryStart(1020));
        Assert.AreEqual(40, boost.GetRemainingSeconds(1020));
    }
}