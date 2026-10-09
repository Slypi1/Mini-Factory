using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private GameConfig _gameConfig;
    
    public GameConfig GameConfig => _gameConfig;
    public FactoryState State => _state;
    public FactoryService Factory => _factory;
    public BoostService Boost => _boost;
 

    private FactoryState _state;
    private FactoryService _factory;
    private ProductionService _production;
    private BoostService _boost;
    
    private ISaveService _saveService;
    private IAnalyticsService _analytics;
    
    private void Start()
    {
        if (_gameConfig == null)
        {
            Debug.LogError("Bootstrapper: GameConfig is not assigned.");
            enabled = false;
            return;
        }
        
        _analytics = new AnalyticsService();
        _analytics.TrackEvent("game_started");

        ISaveService saveService = new SaveService();

        _state = saveService.Load();

        if (_state == null)
            _state = FactoryInitializer.CreateNewGame(_gameConfig);

        EconomyService economy = new EconomyService();
        _factory = new FactoryService(_gameConfig, _state, economy);
        _production = new ProductionService(_factory);
        _boost = new BoostService(_gameConfig, _state);

        if (saveService.HasSave())
        {
            OfflineProgressService offlineService =
                new OfflineProgressService(_gameConfig, _factory);

            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            double offlineIncome =
                offlineService.ApplyOfflineProgress(_state, now);

            if (offlineIncome > 0)
            {
                TrackEvent(
                    "offline_income_applied",
                    new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "income", offlineIncome }
                    });
            }
            
        }

        FactoryLoop loop = GetComponent<FactoryLoop>();

        if (loop == null)
            loop = gameObject.AddComponent<FactoryLoop>();

        loop.Initialize(_production, _boost);
        
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
        if (_state == null)
            return;

        if (_saveService == null)
            _saveService = new SaveService();

        _saveService.Save(_state);
    }
    
    
    public bool TryUnlockMachine(string machineId)
    {
        bool success = _factory != null &&
                       _factory.UnlockMachine(machineId);

        if (success)
            SaveGame();

        return success;
    }

    public bool TryUpgradeMachine(string machineId)
    {
        bool success = _factory != null &&
                       _factory.UpgradeMachine(machineId);

        if (success)
            SaveGame();

        return success;
    }

    public bool TryStartBoost()
    {
        if (_boost == null)
            return false;

        bool success = _boost.TryStart(
            System.DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        if (success)
            SaveGame();

        return success;
    }
    
    public void TrackEvent(
        string eventName,
        System.Collections.Generic.Dictionary<string, object> parameters = null)
    {
        _analytics?.TrackEvent(eventName, parameters);
    }
    
    public void GrantCurrency(double amount)
    {
        if (_factory == null)
            return;

        _factory.AddCurrency(amount);
        SaveGame();
    }
    
}