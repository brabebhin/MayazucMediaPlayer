using MayazucMediaPlayer.Settings;
using System;

namespace MayazucMediaPlayer
{
    public static class PopupHelper
    {
        public static event EventHandler<PopupRequestData>? NotificationRequest;
        /// <summary>
        /// displays a generic "success" message dialog, for confirming succesful completing of actions
        /// </summary>
        /// <returns></returns>
        public static void ShowSuccessDialog()
        {
            if (SettingsService.Instance.ShowConfirmationMessages)
            {
                NotificationRequest?.Invoke(null, new PopupRequestData("Success"));
            }
        }

        /// <summary>
        /// always show the message dialog regardless of settings
        /// </summary>
        /// <param name="content"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static void ShowInfoMessage(string content, string title)
        {
            if (SettingsService.Instance.ShowConfirmationMessages)
            {
                NotificationRequest?.Invoke(null, new PopupRequestData(title, content));
            }
        }
        /// <summary>
        /// always show the message dialog regardless of settings
        /// </summary>
        /// <param name="content"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static void ShowInfoMessage(string content)
        {
            if (SettingsService.Instance.ShowConfirmationMessages)
            {
                NotificationRequest?.Invoke(null, new PopupRequestData(content));
            }
        }
    }

    public class PopupRequestData : EventArgs
    {
        public string Title { get; private set; }

        public string Subtitle { get; private set; }

        public PopupGravityLevel Level { get; private set; }

        public PopupRequestData(string title, string subtitle, PopupGravityLevel level)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Subtitle = subtitle ?? throw new ArgumentNullException(nameof(subtitle));
            Level = level;
        }

        public PopupRequestData(string subtitle, PopupGravityLevel level)
        {
            Title = string.Empty;
            Subtitle = subtitle ?? throw new ArgumentNullException(nameof(subtitle));
            Level = level;
        }

        public PopupRequestData(string subtitle)
        {
            Title = string.Empty;
            Subtitle = subtitle ?? throw new ArgumentNullException(nameof(subtitle));
            Level = PopupGravityLevel.Information;
        }

        public PopupRequestData(string title, string subtitle)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Subtitle = subtitle ?? throw new ArgumentNullException(nameof(subtitle));
        }
    }

    public enum PopupGravityLevel
    {
        Information = 0,
        Warning = 1,
        Error = 2
    }
}
