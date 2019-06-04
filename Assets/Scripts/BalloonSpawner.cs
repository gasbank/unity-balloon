using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System;
using System.Threading.Tasks;

[DisallowMultipleComponent]
public class BalloonSpawner : MonoBehaviour {
    static public BalloonSpawner instance;
    public bool cheatMode;
    public ScInt userPseudoId;
    public int playTimeSec;
    internal bool loadedAtLeastOnce;
    [SerializeField] int lastBalloonLevel = 0;
    [SerializeField] int gem = 0;
    [SerializeField] int riceGatheringAmountPerSec = 0;
    public int LastBalloonLevel => lastBalloonLevel;
    public int Gem => gem;
    public int RiceGatheringAmountPerSec => riceGatheringAmountPerSec;
    public ScInt lastConsumedServiceIndex;
}
