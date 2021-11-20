public class PlatformLocalNotification
{
    public static void RegisterAllRepeatingNotifications()
    {
        SushiDebug.Log("RegisterAllRepeatingNotifications");
    }

    public static void RemoveAllRepeatingNotifications()
    {
        Platform.instance.ClearAllNotifications();
        SushiDebug.Log("RemoveAllRepeatingNotifications");
    }
}