using System.Collections.Generic;
using System.Windows;
using Microsoft.Phone.Controls;
using OneDriveExtentions;

namespace OneDriveTestApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            TryButton.IsEnabled = OneDriveExtentions.OneDriveSession.IsLogged;
            OneDriveExtentions.OneDriveSession.LiveSessionChanged += OneDriveSession_LiveSessionChanged;
            // 用于本地化 ApplicationBar 的示例代码
            //BuildLocalizedApplicationBar();
        }

        void OneDriveSession_LiveSessionChanged(object sender, Microsoft.Live.LiveConnectClient e)
        {
            TryButton.IsEnabled = OneDriveExtentions.OneDriveSession.IsLogged;
        }

        private async void Try(object sender, RoutedEventArgs e)
        {
            var result = await OneDriveSession.GetLoggedClient().GetFolderInFolder("pathTest");
        }

    }
}