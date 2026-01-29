using System;
using System.Configuration;


namespace C8.eServices.Mvc.Helpers
{
    public static class OfflineData
    {
        private static readonly Lazy<bool> _isOffline = new Lazy<bool>(() =>
        {
            bool result;
            return bool.TryParse(
                ConfigurationManager.AppSettings["is_offline"], out result) && result;
        });

        private static readonly Lazy<string> _message = new Lazy<string>(() => ConfigurationManager.AppSettings["offline_message"]);

        public static bool IsOffline
        {
            get { return _isOffline.Value; }
        }

        public static bool IsOnline
        {
            get { return !_isOffline.Value; }
        }

        public static string Message
        {
            get { return _message.Value; }
        }

        public static bool HasMessage
        {
            get { return !string.IsNullOrWhiteSpace(Message); }
        }

        public static void Execute(Action online, Action offline)
        {
            if (IsOffline)
                offline();
            else
                online();
        }
    }
}