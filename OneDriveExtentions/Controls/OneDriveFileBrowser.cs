using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OneDriveExtentions.Controls
{

    public interface IOneDriveFileBrowserThemeProvider
    {
        Brush GetBrushForItem(OneDriveItem item);

        Size GetIconSizeForItem(OneDriveItem item);

    }

    public class DefaultOneDriveFileBrowserThemeProvider : IOneDriveFileBrowserThemeProvider
    {

        static DefaultOneDriveFileBrowserThemeProvider()
        {
        }

        private static readonly Brush _fileBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileIcon.png", UriKind.Relative))
        };
        private static readonly Brush _filePhotoBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/PhotoIcon.png", UriKind.Relative))
        };
        private static readonly Brush _folderBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FolderIcon.png", UriKind.Relative))
        };

        private static readonly Brush _folderEmptyBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/EmptyFolderIcon.png", UriKind.Relative))
        };
        private static readonly Brush _folderAlbumBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/AlbumFolderIcon.png", UriKind.Relative))
        };

        public Brush GetBrushForItem(OneDriveItem item)
        {
            return item.IsFolder ? (item.IsPhotoRelate ? _folderAlbumBrush : ((OneDriveFolder) item).Count > 0 ? _folderBrush : _folderEmptyBrush) : (item.IsPhotoRelate ? _filePhotoBrush : _fileBrush);
        }

        private static readonly Size _iconSize = new Size(48, 48);

        public Size GetIconSizeForItem(OneDriveItem item)
        {
            return _iconSize;
        }

    }

    public class OneDriveFileBrowserItem
    {
        internal OneDriveFileBrowserItem(OneDriveItem item, IOneDriveFileBrowserThemeProvider themeProvider)
        {
            Item = item;
            if (themeProvider != null)
            {
                IconBrush = themeProvider.GetBrushForItem(item);
                IconSize = themeProvider.GetIconSizeForItem(item);
            }
        }

        public OneDriveItem Item { get; private set; }

        public Brush IconBrush { get; private set; }

        public Size IconSize { get; private set; }

    }

    [TemplatePart(Name = SignInButtonName, Type = typeof(OneDriveSignInButton))]
    [TemplatePart(Name = FileListName, Type = typeof(ListBox))]
    public sealed class OneDriveFileBrowser : Control
    {
        private const string SignInButtonName = "SignInButton";
        private const string FileListName = "FileList";

        private OneDriveSignInButton SignInButton { get; set; }

        private ListBox FileList { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SignInButton = GetTemplateChild(SignInButtonName) as OneDriveSignInButton;
            FileList = GetTemplateChild(FileListName) as ListBox;
            UpdateBasicComponentsVisibility();
            if (FileList != null)
            {
                FileList.SelectionChanged += FileList_SelectionChanged;
            }
        }

        void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var item = (OneDriveFileBrowserItem) e.AddedItems[0];
                if (item.Item is OneDriveFolder)
                {
                    TryGetItems((OneDriveFolder) item.Item);
                }
                ((ListBox) sender).SelectedIndex = -1;
            }
        }

        public OneDriveFileBrowser()
        {
            DefaultStyleKey = typeof (OneDriveFileBrowser);
            ItemsSource = new ObservableCollection<OneDriveFileBrowserItem>();
            OneDriveSession.LiveSessionChanged+=OneDriveSession_LiveSessionChanged;
            Loaded += OneDriveFileBrowser_Loaded;
        }

        void OneDriveFileBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBasicComponentsVisibility();
        }

        private void OneDriveSession_LiveSessionChanged(object sender, Microsoft.Live.LiveConnectClient e)
        {
            UpdateBasicComponentsVisibility();
            TryGetItems();
        }

        void UpdateBasicComponentsVisibility()
        {
            if (SignInButton != null)
            {
                SignInButton.Visibility = OneDriveSession.IsLogged ? Visibility.Collapsed:Visibility.Visible;
            }
            if (FileList != null)
            {
                FileList.Visibility = OneDriveSession.IsLogged ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        void TryGetItems()
        {
            if (OneDriveSession.IsLogged)
            {
                if (DesiredFolder == null)
                {
                    TryGetItems(OneDriveFolder.RootFolder);
                }
                else if (CurrentDisplayFolder == null || CurrentDisplayFolder.Id != DesiredFolder.Id)
                {
                    TryGetItems(DesiredFolder);
                }
            }
        }

        async void TryGetItems(OneDriveFolder desiredFolder)
        {
            DesiredFolder = desiredFolder;
            var result = await OneDriveSession.GetLoggedClient().GetItemsInFolderAsync(DesiredFolder.Id);
            ItemsSource.Clear();
            if (result.IsSuccessful)
            {
                CurrentDisplayFolder = DesiredFolder;
                foreach (var oneDriveItem in result.Items)
                {
                    ItemsSource.Add(new OneDriveFileBrowserItem(oneDriveItem, ThemeProvider));
                }
            }
            else
            {
                
            }
        }

        private OneDriveFolder CurrentDisplayFolder { get; set; }

        private OneDriveFolder DesiredFolder { get; set; }

        public static readonly DependencyProperty ThemeProviderProperty = DependencyProperty.Register(
            "ThemeProvider", typeof(IOneDriveFileBrowserThemeProvider), typeof(OneDriveFileBrowser), new PropertyMetadata(default(IOneDriveFileBrowserThemeProvider)));

        public IOneDriveFileBrowserThemeProvider ThemeProvider
        {
            get { return (IOneDriveFileBrowserThemeProvider)GetValue(ThemeProviderProperty); }
            set { SetValue(ThemeProviderProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof (ObservableCollection<OneDriveFileBrowserItem>), typeof (OneDriveFileBrowser), new PropertyMetadata(default(ObservableCollection<OneDriveFileBrowserItem>)));

        public ObservableCollection<OneDriveFileBrowserItem> ItemsSource
        {
            get { return (ObservableCollection<OneDriveFileBrowserItem>) GetValue(ItemsSourceProperty); }
            protected set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof (DataTemplate), typeof (OneDriveFileBrowser), new PropertyMetadata(default(DataTemplate)));

        /// <summary>
        /// 列表项样式，详细数据属性可见于：
        /// OneDriveExtentions.Controls.OneDriveFileBrowserItem
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate) GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

    }

}
