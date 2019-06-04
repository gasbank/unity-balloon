using UnityEngine;

public class PlatformLocalNotification {
    static public void RegisterAllRepeatingNotifications() {
        SushiDebug.Log("RegisterAllRepeatingNotifications");
    }

    static public void RemoveAllRepeatingNotifications() {
        Platform.instance.ClearAllNotifications();
        SushiDebug.Log("RemoveAllRepeatingNotifications");
    }
}
