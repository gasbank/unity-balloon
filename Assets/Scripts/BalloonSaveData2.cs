using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class BalloonSaveData2 {
    public ScInt version = 0;
    public ScInt playTimeSec = 0;
    // Config
    public bool muteBgmAudioSource = false;
    public bool muteSfxAudioSource = false;
    public bool notchSupport = false;
    public bool performanceMode = false;
    public bool alwaysOn = false;
    public ScInt userPseudoId = 0;
    public ScInt lastConsumedServiceIndex = 0;
    public BalloonLanguageCode languageCode = BalloonLanguageCode.Ko;
    public NoticeData noticeData = null;
    public bool gatherStoredMaxSfxEnabled = false;
    public ScInt[] whacACatStageClearLevelList = null;

    public Dictionary<ScString,ScInt> purchasedProductDict = null;
    public bool cheatMode = false;
}