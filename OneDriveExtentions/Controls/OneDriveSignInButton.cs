using Microsoft.Live;
using Microsoft.Live.Controls;

namespace OneDriveExtentions.Controls
{

    public sealed class OneDriveSignInButton : SignInButton
    {

        public OneDriveSignInButton()
        {
            Branding = BrandingType.Skydrive;
            ClientId = OneDriveSession.ClientId;
            Scopes = OneDriveSession.Scopes;
            SessionChanged += OneDriveSignInButton_SessionChanged;
        }

        void OneDriveSignInButton_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        {
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                OneDriveSession.Login(e.Session);
            }
            else
            {
                OneDriveSession.Logout();
            }
        }

    }
}
