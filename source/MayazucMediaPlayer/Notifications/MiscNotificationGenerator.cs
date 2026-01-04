namespace MayazucMediaPlayer.Notifications
{
    public static class MiscNotificationGenerator
    {
        public static void ShowToast(string msg, string subMsg = "", bool supressPopup = true)
        {
            PopupHelper.ShowInfoMessage(subMsg, msg);
        }
    }
}
