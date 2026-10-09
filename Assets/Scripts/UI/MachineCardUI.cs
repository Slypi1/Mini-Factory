using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MachineCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _machineNameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _productionText;
    [SerializeField] private TMP_Text _actionButtonText;
    [SerializeField] private Button _actionButton;

    private string machineId;
    private System.Action<string> onActionClicked;

    public void Setup(
        string id,
        string displayName,
        int level,
        bool isUnlocked,
        double production,
        double cost,
        System.Action<string> actionCallback,
        bool canAfford)
    {
        if (_actionButton != null)
        {
            _actionButton.interactable = canAfford;
        }
        
        machineId = id;
        onActionClicked = actionCallback;

        _machineNameText.text = displayName;
        _levelText.text = isUnlocked
            ? $"Level: {level}"
            : "Locked";

        _productionText.text = isUnlocked
            ? $"Production: {production:F1}/sec"
            : $"Unlock cost: {cost:F0}";

        _actionButtonText.text = isUnlocked
            ? $"Upgrade ({cost:F0})"
            : $"Unlock ({cost:F0})";

        _actionButton.interactable = true;
        _actionButton.onClick.RemoveAllListeners();
        _actionButton.onClick.AddListener(
            () => onActionClicked?.Invoke(machineId));
    }
}
