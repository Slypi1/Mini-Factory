using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;
    
    public GameConfig GameConfig => gameConfig;
    public FactoryState State => state;
    public FactoryService Factory => factory;
    public BoostService Boost => boost;
 

    private FactoryState state;
    private FactoryService factory;
    private ProductionService production;
    private BoostService boost;
    
    private ISaveService saveService;
    private IAnalyticsService analytics;
    
    private void Start()
    {
        if (gameConfig == null)
        {
            Debug.LogError("Bootstrapper: GameConfig is not assigned.");
            enabled = false;
            return;
        }
        
        analytics = new AnalyticsService();
        analytics.TrackEvent("game_started");

        ISaveService saveService = new SaveService();

        state = saveService.Load();

        if (state == null)
            state = FactoryInitializer.CreateNewGame(gameConfig);

        EconomyService economy = new EconomyService();
        factory = new FactoryService(gameConfig, state, economy);
        production = new ProductionService(factory);
        boost = new BoostService(gameConfig, state);

        if (saveService.HasSave())
        {
            OfflineProgressService offlineService =
                new OfflineProgressService(gameConfig, factory);

            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            double offlineIncome =
                offlineService.ApplyOfflineProgress(state, now);

            if (offlineIncome > 0)
            {
                TrackEvent(
                    "offline_income_applied",
                    new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "income", offlineIncome }
                    });
            }

            Debug.Log($"Offline income applied: {offlineIncome:F2}");
        }

        FactoryLoop loop = GetComponent<FactoryLoop>();

        if (loop == null)
            loop = gameObject.AddComponent<FactoryLoop>();

        loop.Initialize(production, boost);

        Debug.Log(
            $"Factory started. Balance: {state.Currency:F2}, " +
            $"Production: {factory.GetTotalProduction():F2} coins/sec");
    }
    
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void SaveGame()
    {
        if (state == null)
            return;

        if (saveService == null)
            saveService = new SaveService();

        saveService.Save(state);
    }
    
    
    public bool TryUnlockMachine(string machineId)
    {
        bool success = factory != null &&
                       factory.UnlockMachine(machineId);

        if (success)
            SaveGame();

        return success;
    }

    public bool TryUpgradeMachine(string machineId)
    {
        bool success = factory != null &&
                       factory.UpgradeMachine(machineId);

        if (success)
            SaveGame();

        return success;
    }

    public bool TryStartBoost()
    {
        if (boost == null)
            return false;

        bool success = boost.TryStart(
            System.DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        if (success)
            SaveGame();

        return success;
    }
    
    public void TrackEvent(
        string eventName,
        System.Collections.Generic.Dictionary<string, object> parameters = null)
    {
        analytics?.TrackEvent(eventName, parameters);
    }
    
    public void GrantCurrency(double amount)
    {
        if (factory == null)
            return;

        factory.AddCurrency(amount);
        SaveGame();
    }
    
}