using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using OneDriveExtentions;

namespace OneDriveTestApp
{

    public partial class MainPage
    {
        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            TryButton.IsEnabled = OneDriveSession.IsLogged;
            OneDriveSession.LiveSessionChanged += OneDriveSession_LiveSessionChanged;
            OneDriveFileSyncPool.FileSynced += OneDriveFileSyncPool_FileSynced;
            // 用于本地化 ApplicationBar 的示例代码
            //BuildLocalizedApplicationBar();
        }

        void OneDriveSession_LiveSessionChanged(object sender, Microsoft.Live.LiveConnectClient e)
        {
            TryButton.IsEnabled = OneDriveSession.IsLogged;
        }

        private async void TryButton_OnClick(object sender, RoutedEventArgs e)
        {
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("TestFolder", CreationCollisionOption.OpenIfExists);
            for (int i = 0; i < 10; i++)
            {
                var name = string.Format("{0:yyyyMMdd_HHmmssfff}.txt", DateTime.Now);
                var file = await folder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    var buffer = Encoding.UTF8.GetBytes(file.Name);
                    stream.Write(buffer, 0, buffer.Length);
                }
                CreateResult.Items.Add(string.Format("Created : {0}", file.Name));
            }
            await Task.Delay(1000);
            OneDriveInfoResult targetFolder;
            do
            {
                targetFolder = await OneDriveSession.GetLoggedClient().GetFolderInFolder("SyncTest");

            } while (!targetFolder.IsSuccessful);
            OneDriveFileSyncPool.ClearQueue();
            OneDriveFileSync.GetInstance().SyncFolderAsync(folder, targetFolder.Item.Id);
        }

        private async void ClearLocalFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("TestFolder", CreationCollisionOption.OpenIfExists);
            await folder.DeleteAsync();
        }

        void OneDriveFileSyncPool_FileSynced(object sender, IStorageFile e)
        {
            UploadResult.Items.Add(string.Format("Uploaded : {0}", e.Name));
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (!e.Cancel && MainPivot.SelectedIndex == 0 && OneDriveFileBrowser.CanGoBack)
            {
                e.Cancel = true;
                OneDriveFileBrowser.GoBack();
            }
        }

    }

}