using UnityEngine;

[CreateAssetMenu(
    fileName = "GameConfig",
    menuName = "Mini Factory/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Machines")]
    public MachineConfig[] Machines;

    [Header("Boost")]
    public bool BoostEnabled = true;

    [Min(1)]
    public float BoostMultiplier = 2f;

    [Min(1)]
    public float BoostDurationSeconds = 60f;

    [Header("Offline Progress")]
    [Min(0)]
    public float MaxOfflineDurationSeconds = 8 * 60 * 60;
}

