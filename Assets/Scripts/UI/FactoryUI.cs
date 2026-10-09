using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FactoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Bootstrapper bootstrapper;
    [SerializeField] private Transform machinesContainer;
    [SerializeField] private MachineCardUI machineCardPrefab;
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text totalProductionText;
    
    [Header("Boost UI")]
    [SerializeField] private UnityEngine.UI.Button boostButton;
    [SerializeField] private TMP_Text boostTimerText;
    
    private bool wasBoostActive;

    private readonly List<MachineCardUI> cards =
        new List<MachineCardUI>();

    private void Start()
    {
        if (bootstrapper == null ||
            machinesContainer == null ||
            machineCardPrefab == null)
        {
            Debug.LogError(
                "FactoryUI: Assign Bootstrapper, " +
                "Machines Container and Machine Card Prefab.");
            enabled = false;
            return;
        }

        CreateMachineCards();
        
        if (boostButton != null)
        {
            boostButton.onClick.AddListener(OnBoostClicked);
        }
        
        RefreshUI();
    }

    private void Update()
    {
        RefreshUI();
    }

    private void CreateMachineCards()
    {
        foreach (Transform child in machinesContainer)
        {
            Destroy(child.gameObject);
        }

        cards.Clear();

        GameConfig config = bootstrapper.GameConfig;

        if (config == null || config.Machines == null)
            return;

        foreach (MachineConfig machine in config.Machines)
        {
            if (machine == null)
                continue;

            MachineCardUI card = Instantiate(
                machineCardPrefab,
                machinesContainer);

            cards.Add(card);
        }
    }

    private void RefreshUI()
    {
        if (bootstrapper == null ||
            bootstrapper.State == null ||
            bootstrapper.Factory == null)
            return;

        FactoryState state = bootstrapper.State;
        GameConfig config = bootstrapper.GameConfig;

        if (balanceText != null)
            balanceText.text = $"Coins: {state.Currency:F1}";

        if (totalProductionText != null)
            totalProductionText.text =
                $"Production: {bootstrapper.Factory.GetTotalProduction():F1}/sec";

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

            cards[cardIndex].Setup(
                machine.MachineId,
                machine.DisplayName,
                machineState.Level,
                unlocked,
                production,
                cost,
                OnMachineAction);

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
            FindMachineState(bootstrapper.State, machineId);

        if (machineState == null)
            return;

        bool wasUnlocked = machineState.IsUnlocked;
        bool success;

        if (wasUnlocked)
            success = bootstrapper.TryUpgradeMachine(machineId);
        else
            success = bootstrapper.TryUnlockMachine(machineId);

        if (success)
        {
            bootstrapper.TrackEvent(
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
        bool started = bootstrapper.TryStartBoost();

        if (started)
        {
            bootstrapper.TrackEvent("boost_started");
            RefreshUI();
        }
    }

    private void RefreshBoostUI()
    {
        if (bootstrapper == null || bootstrapper.Boost == null)
            return;

        long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        bool active = bootstrapper.Boost.IsActive(now);

        if (boostTimerText != null)
        {
            if (active)
            {
                long remaining = bootstrapper.Boost.GetRemainingSeconds(now);
                boostTimerText.text = $"BOOST x{bootstrapper.GameConfig.BoostMultiplier:F1} — {remaining}s";
            }
            else
            {
                boostTimerText.text = bootstrapper.GameConfig.BoostEnabled
                    ? "Boost ready"
                    : "Boost disabled";
            }
        }

        if (boostButton != null)
        {
            boostButton.interactable =
                bootstrapper.GameConfig.BoostEnabled && !active;
        }
        
        if (wasBoostActive && !active)
        {
            bootstrapper.TrackEvent("boost_finished");
        }

        wasBoostActive = active;
    }
}
