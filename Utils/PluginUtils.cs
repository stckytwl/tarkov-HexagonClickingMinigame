using BepInEx.Logging;
using EFT.Communications;

namespace stckytwl.OSU
{
    public static class PluginUtils
    {
        public static ManualLogSource Logger;

        public static void DisplayMessageNotification(string message)
        {
            NotificationManagerClass.DisplayWarningNotification(message);
        }

        public static void DisplayWarningNotification(string message)
        {
            NotificationManagerClass.DisplayWarningNotification(message, ENotificationDurationType.Long);
        }
    }
}