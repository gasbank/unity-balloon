using System.Diagnostics;

public static class BalloonDebug
{
    [Conditional("BALLOON_DEBUG")]
    public static void Log(object message) => UnityEngine.Debug.Log(message);

    [Conditional("BALLOON_DEBUG")]
    public static void LogFormat(string format, params object[] args) => UnityEngine.Debug.LogFormat(format, args);
}
