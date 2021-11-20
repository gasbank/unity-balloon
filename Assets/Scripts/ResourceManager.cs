using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;

    public Dictionary<string, int> RedeemedCouponCode = new Dictionary<string, int>();

    public int accountLevel => BalloonSpawner.instance.LastBalloonLevel;

    public int accountLevelExp // unused
        =>
            1;

    public int accountGem => BalloonSpawner.instance.Gem;
    public int accountRiceRate => BalloonSpawner.instance.RiceGatheringAmountPerSec;
}