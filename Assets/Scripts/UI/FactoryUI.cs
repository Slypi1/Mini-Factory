using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FactoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Bootstrapper _bootstrapper;
    [SerializeField] private Transform _machinesContainer;
    [SerializeField] private MachineCardUI _machineCardPrefab;
    [SerializeField] private TMP_Text _balanceText;
    [SerializeField] private TMP_Text _totalProductionText;
    
    [Header("Boost UI")]
    [SerializeField] private UnityEngine.UI.Button _boostButton;
    [SerializeField] private TMP_Text _boostTimerText;
    
    private bool wasBoostActive;

    private readonly List<MachineCardUI> cards =
        new List<MachineCardUI>();

    private void Start()
    {
        if (_bootstrapper == null ||
            _machinesContainer == null ||
            _machineCardPrefab == null)
        {
            Debug.LogError(
                "FactoryUI: Assign Bootstrapper, " +
                "Machines Container and Machine Card Prefab.");
            enabled = false;
            return;
        }

        CreateMachineCards();
        
        if (_boostButton != null)
        {
            _boostButton.onClick.AddListener(OnBoostClicked);
        }
        
        RefreshUI();
    }

    private void Update()
    {
        RefreshUI();
    }

    private void CreateMachineCards()
    {
        foreach (Transform child in _machinesContainer)
        {
            Destroy(child.gameObject);
        }

        cards.Clear();

        GameConfig config = _bootstrapper.GameConfig;

        if (config == null || config.Machines == null)
            return;

        foreach (MachineConfig machine in config.Machines)
        {
            if (machine == null)
                continue;

            MachineCardUI card = Instantiate(
                _machineCardPrefab,
                _machinesContainer);

            cards.Add(card);
        }
    }

    private void RefreshUI()
    {
        if (_bootstrapper == null ||
            _bootstrapper.State == null ||
            _bootstrapper.Factory == null)
            return;

        FactoryState state = _bootstrapper.State;
        GameConfig config = _bootstrapper.GameConfig;

        if (_balanceText != null)
            _balanceText.text = ((int)state.Currency).ToString();

        if (_totalProductionText != null)
            _totalProductionText.text = _bootstrapper.Factory.GetTotalProduction().ToString();

        int cardIndex = 0;

        foreach (MachineConfig machine in config.Machines)
        {
            if (machine == null)
                continue;

            if (cardIndex >= cards.Count)
                break;

            MachineState machineState =
                FindMachineState(state, machine.MachineId);

            if (machineState == null)
            {
                cardIndex++;
                continue;
            }

            bool unlocked = machineState.IsUnlocked;

            double production = unlocked
                ? machine.GetProduction(machineState.Level)
                : 0;

            double cost = unlocked
                ? machine.GetUpgradeCost(machineState.Level)
                : machine.UnlockCost;

            bool canAfford = state.Currency >= cost;

            cards[cardIndex].Setup(
                machine.MachineId,
                machine.DisplayName,
                machineState.Level,
                unlocked,
                production,
                cost,
                OnMachineAction,
                canAfford);

            cardIndex++;
        }
        
        RefreshBoostUI();
    }

    private MachineState FindMachineState(
        FactoryState state,
        string machineId)
    {
        foreach (MachineState machine in state.Machines)
        {
            if (machine.MachineId == machineId)
                return machine;
        }

        return null;
    }

    private void OnMachineAction(string machineId)
    {
        MachineState machineState =
            FindMachineState(_bootstrapper.State, machineId);

        if (machineState == null)
            return;

        bool wasUnlocked = machineState.IsUnlocked;
        bool success;

        if (wasUnlocked)
            success = _bootstrapper.TryUpgradeMachine(machineId);
        else
            success = _bootstrapper.TryUnlockMachine(machineId);

        if (success)
        {
            _bootstrapper.TrackEvent(
                wasUnlocked ? "machine_upgraded" : "machine_unlocked",
                new System.Collections.Generic.Dictionary<string, object>
                {
                    { "machine_id", machineId },
                    { "level", machineState.Level }
                });
        }

        RefreshUI();
    }
    
    private void OnBoostClicked()
    {
        bool started = _bootstrapper.TryStartBoost();

        if (started)
        {
            _bootstrapper.TrackEvent("boost_started");
            RefreshUI();
        }
    }

    private void RefreshBoostUI()
    {
        if (_bootstrapper == null || _bootstrapper.Boost == null)
            return;

        long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        bool active = _bootstrapper.Boost.IsActive(now);

        if (_boostTimerText != null)
        {
            if (active)
            {
                long remaining = _bootstrapper.Boost.GetRemainingSeconds(now);
                _boostTimerText.text = $"BOOST x{_bootstrapper.GameConfig.BoostMultiplier:F1} — {remaining}s";
            }
            else
            {
                _boostTimerText.text = _bootstrapper.GameConfig.BoostEnabled
                    ? "Boost ready"
                    : "Boost disabled";
            }
        }

        if (_boostButton != null)
        {
            _boostButton.interactable =
                _bootstrapper.GameConfig.BoostEnabled && !active;
        }
        
        if (wasBoostActive && !active)
        {
            _bootstrapper.TrackEvent("boost_finished");
        }

        wasBoostActive = active;
    }
}
