using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MachineCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text machineNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text productionText;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button actionButton;

    private string machineId;
    private System.Action<string> onActionClicked;

    public void Setup(
        string id,
        string displayName,
        int level,
        bool isUnlocked,
        double production,
        double cost,
        System.Action<string> actionCallback)
    {
        machineId = id;
        onActionClicked = actionCallback;

        machineNameText.text = displayName;
        levelText.text = isUnlocked
            ? $"Level: {level}"
            : "Locked";

        productionText.text = isUnlocked
            ? $"Production: {production:F1}/sec"
            : $"Unlock cost: {cost:F0}";

        actionButtonText.text = isUnlocked
            ? $"Upgrade ({cost:F0})"
            : $"Unlock ({cost:F0})";

        actionButton.interactable = true;
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(
            () => onActionClicked?.Invoke(machineId));
    }
}
