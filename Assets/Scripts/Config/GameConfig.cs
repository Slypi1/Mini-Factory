using UnityEngine;

[CreateAssetMenu(
    fileName = "GameConfig",
    menuName = "Mini Factory/Game Config", order = -1000)]
public class GameConfig : ScriptableObject
{
    [Header("Machines")]
    public MachineConfig[] Machines;

    [Header("Boost")]
    public bool BoostEnabled;

    [Min(1)]
    public float BoostMultiplier;

    [Min(1)]
    public float BoostDurationSeconds;

    [Header("Offline Progress")]
    [Min(0)]
    public float MaxOfflineDurationSeconds;
}

