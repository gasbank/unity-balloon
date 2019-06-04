using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ResourceManager : MonoBehaviour {

	public static ResourceManager instance;

	public int accountLevel { get { return BalloonSpawner.instance.LastBalloonLevel; } }
	public int accountLevelExp { get { return 1; } } // unused
	public int accountGem { get { return BalloonSpawner.instance.Gem; } }
	public int accountRiceRate { get { return BalloonSpawner.instance.RiceGatheringAmountPerSec; } }

	public Dictionary<string, int> RedeemedCouponCode = new Dictionary<string, int>();
}
