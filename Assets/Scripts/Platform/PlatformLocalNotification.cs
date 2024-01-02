public class PlatformLocalNotification
{
    public static void RegisterAllRepeatingNotifications()
    {
        BalloonDebug.Log("RegisterAllRepeatingNotifications");
    }

    public static void RemoveAllRepeatingNotifications()
    {
        Platform.instance.ClearAllNotifications();
        BalloonDebug.Log("RemoveAllRepeatingNotifications");
    }
}