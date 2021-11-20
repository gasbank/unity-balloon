using System;
using System.Collections.Generic;

[Serializable]
public class BalloonSaveData2
{
    public bool alwaysOn;
    public bool cheatMode;
    public bool gatherStoredMaxSfxEnabled;
    public BalloonLanguageCode languageCode = BalloonLanguageCode.Ko;

    public ScInt lastConsumedServiceIndex = 0;

    // Config
    public bool muteBgmAudioSource;
    public bool muteSfxAudioSource;
    public bool notchSupport;
    public NoticeData noticeData;
    public bool performanceMode;
    public ScInt playTimeSec = 0;

    public Dictionary<ScString, ScInt> purchasedProductDict;
    public ScInt userPseudoId = 0;
    public ScInt version = 0;
    public ScInt[] whacACatStageClearLevelList;
}