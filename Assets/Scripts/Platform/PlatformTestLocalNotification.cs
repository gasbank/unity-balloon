using UnityEngine;

[DisallowMultipleComponent]
public class PlatformTestLocalNotification : MonoBehaviour
{
    public void ScheduleNotification5000ms()
    {
        ScheduleNotification(1, 5000, "icon1024_2_gray");
    }

    public void ScheduleNotification30000ms()
    {
        ScheduleNotification(2, 30000, "icon1024_2_gray");
    }

    public void ScheduleNotification120000ms()
    {
        ScheduleNotification(3, 120000, "icon1024_2_gray");
    }

    void ScheduleNotification(int id, int afterMs, string largeIcon)
    {
        Platform.instance.RegisterSingleNotification("Test", string.Format("ID: {0}, {1} ms after", id, afterMs),
            afterMs, largeIcon);
    }

    public void ClearNotification()
    {
        Platform.instance.ClearAllNotifications();
    }
}