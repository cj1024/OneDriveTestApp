using System;
using System.Diagnostics;
using Microsoft.Live;

namespace OneDriveExtentions
{
 
    public class OneDriveSession
    {

        public static string ClientId { get; set; }

        public static string Scopes { get; set; }

        public static async void InitialAsync()
        {
            if (string.IsNullOrEmpty(ClientId))
            {
                throw new ArgumentNullException("OneDriveSession.ClientId", "ClientID Is Not Set");
            }
            if (string.IsNullOrEmpty(Scopes))
            {
                throw new ArgumentNullException("OneDriveSession.Scopes", "Scopes Is Not Set");
            }
            if (IsLogged)
            {
                Logout();
            }
            var authClient = new LiveAuthClient(ClientId);
            var result = await authClient.InitializeAsync();
            if (result.Status == LiveConnectSessionStatus.Connected && !IsLogged)
            {
                Login(result.Session);
            }
        }

        private static LiveConnectClient _loggedClient;

        private static LiveConnectClient LoggedClient
        {
            get { return _loggedClient; }
            set
            {
                _loggedClient = value;
                if (LiveSessionChanged != null)
                {
                    LiveSessionChanged.Invoke(null, LoggedClient);
                }
            }
        }

        public static bool IsLogged
        {
            get { return LoggedClient != null; }
        }

        public static LiveConnectClient GetLoggedClient()
        {
            return LoggedClient;
        }

        public static LiveConnectClient Login(LiveConnectSession session)
        {
            Debug.Assert(session != null, "Session Is Empty!");
            LoggedClient = new LiveConnectClient(session);
            OneDriveFileSyncPool.NotifyTryStartOneTask();
            return GetLoggedClient();
        }

        public static void Logout()
        {
            OneDriveFileSyncPool.ClearQueue();
            LoggedClient = null;
        }

        public static event EventHandler<LiveConnectClient> LiveSessionChanged;
        
    }

}
