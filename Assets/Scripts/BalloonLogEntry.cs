using System;

[Serializable]
public struct BalloonLogEntry
{
    public enum Type
    {
        DummyLogRecord,
        GameCloudLoadBegin,
        GameCloudSaveBegin,
        GameOpenLeaderboard,
        GameOpenAchievements,
        GameCloudLoadFailure,
        GameCloudSaveFailure,
        GameCloudLoadEnd,
        GameCloudSaveEnd,
        GameSaved,
        GameSaveFailure,
        GameLoadFailure,
        GameLoaded,
        GameCriticalError,
        GemToZero,
        GameReset
    }

    public long ticks;
    public int type;
    public int arg1;
    public long arg2;
}