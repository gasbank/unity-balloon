using UnityEngine;

[DisallowMultipleComponent]
public class BalloonSpawner : MonoBehaviour
{
    public static BalloonSpawner instance;
    public bool cheatMode;
    public ScInt lastConsumedServiceIndex;
    internal bool loadedAtLeastOnce;
    public int playTimeSec;
    public ScInt userPseudoId;

    [field: SerializeField]
    public int LastBalloonLevel { get; }

    [field: SerializeField]
    public int Gem { get; }

    [field: SerializeField]
    public int RiceGatheringAmountPerSec { get; }
}