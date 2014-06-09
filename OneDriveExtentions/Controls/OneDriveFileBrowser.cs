using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OneDriveExtentions.Controls
{

    public class OneDriveFileBrowserItem
    {
        internal OneDriveFileBrowserItem(OneDriveItem item, IOneDriveFileBrowserThemeProvider themeProvider)
        {
            Item = item;
            Count = item.ItemType.HasFlag(OneDriveItemType.Folder) ? (int?)((OneDriveFolder) item).Count : null;
            if (themeProvider != null)
            {
                IconBrush = themeProvider.GetBrushForItem(item);
                IconSize = themeProvider.GetIconSizeForItem(item);
            }
        }

        public OneDriveItem Item { get; private set; }

        public int? Count { get; private set; }

        public Brush IconBrush { get; private set; }

        public Size IconSize { get; private set; }

    }

    [TemplatePart(Name = SignInButtonName, Type = typeof(OneDriveSignInButton))]
    [TemplatePart(Name = FileListName, Type = typeof(ListBox))]
    [TemplatePart(Name = LoadingViewName, Type = typeof(UIElement))]
    [TemplatePart(Name = ProgressViewName, Type = typeof(ProgressBar))]
    [TemplatePart(Name = ControlsPanelName, Type = typeof(UIElement))]
    [TemplatePart(Name = HomeButtonName, Type = typeof(Button))]
    [TemplatePart(Name = BackButtonName, Type = typeof(Button))]
    [TemplatePart(Name = RefreshButtonName, Type = typeof(Button))]
    public sealed partial class OneDriveFileBrowser : Control
    {
        private const string SignInButtonName = "SignInButton";
        private const string FileListName = "FileList";
        private const string LoadingViewName = "LoadingView";
        private const string ProgressViewName = "ProgressView";
        private const string ControlsPanelName = "ControlsPanel";
        private const string HomeButtonName = "HomeButton";
        private const string BackButtonName = "BackButton";
        private const string RefreshButtonName = "RefreshButton";

        private OneDriveSignInButton SignInButton { get; set; }

        private ListBox FileList { get; set; }

        private UIElement LoadingView { get; set; }

        private ProgressBar ProgressView { get; set; }

        private UIElement ControlsPanel { get; set; }

        private Button HomeButton { get; set; }

        private Button BackButton { get; set; }

        private Button RefreshButton { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SignInButton = GetTemplateChild(SignInButtonName) as OneDriveSignInButton;
            FileList = GetTemplateChild(FileListName) as ListBox;
            LoadingView = GetTemplateChild(LoadingViewName) as UIElement;
            ProgressView = GetTemplateChild(ProgressViewName) as ProgressBar;
            ControlsPanel = GetTemplateChild(ControlsPanelName) as UIElement;
            HomeButton = GetTemplateChild(HomeButtonName) as Button;
            BackButton = GetTemplateChild(BackButtonName) as Button;
            RefreshButton = GetTemplateChild(RefreshButtonName) as Button;
            UpdateBasicComponentsVisibility();
            if (FileList != null)
            {
                FileList.SelectionChanged += FileList_SelectionChanged;
            }
            if (HomeButton != null)
            {
                HomeButton.Click += (sender, e) => GoToHome();
            }
            if (BackButton != null)
            {
                BackButton.Click += (sender, e) => GoBack();
            }
            if (RefreshButton != null)
            {
                RefreshButton.Click += (sender, e) => Refresh();
            }
        }

        public OneDriveFileBrowser()
        {
            DefaultStyleKey = typeof(OneDriveFileBrowser);
            ItemsSource = new ObservableCollection<OneDriveFileBrowserItem>();
            OneDriveSession.LiveSessionChanged += OneDriveSession_LiveSessionChanged;
            Loaded += OneDriveFileBrowser_Loaded;
        }

        void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var item = (OneDriveFileBrowserItem) e.AddedItems[0];
                var folder = item.Item as OneDriveFolder;
                if (folder != null)
                {
                    TryGetItems(folder);
                }
                else
                {
                    if (OneDriveFileSelected != null)
                    {
                        var file = item.Item as OneDriveFile;
                        if (file != null)
                        {
                            OneDriveFileSelected.Invoke(this, file);
                        }
                    }
                }
                ((ListBox) sender).SelectedIndex = -1;
            }
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
            if (ControlsPanel != null)
            {
                ControlsPanel.Visibility = OneDriveSession.IsLogged ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        void UpdateOnLoading(bool isOnLoading)
        {
            if (FileList != null)
            {
                FileList.IsHitTestVisible = !isOnLoading;
            }
            if (LoadingView != null)
            {
                LoadingView.Visibility = isOnLoading ? Visibility.Visible : Visibility.Collapsed;
            }
            if (ProgressView != null)
            {
                ProgressView.IsEnabled = isOnLoading;
            }
        }

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
            private set { SetValue(ItemsSourceProperty, value); }
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

        public event EventHandler<OneDriveFile> OneDriveFileSelected;

    }

    /// <summary>
    /// 导航相关
    /// </summary>
    partial class OneDriveFileBrowser
    {
        
        private readonly Stack<OneDriveFolder> _backStack = new Stack<OneDriveFolder>();

        public IReadOnlyList<OneDriveFolder> BackStack
        {
            get { return _backStack.ToArray(); }
        }

        public void GoToHome()
        {
            _backStack.Clear();
            CurrentDisplayFolder = DesiredFolder = null;
            TryGetItems();
        }

        public bool CanGoBack
        {
            get { return _backStack.Any(); }
        }

        public void GoBack()
        {
            if (CanGoBack)
            {
                var back = _backStack.Pop();
                CurrentDisplayFolder = null;
                DesiredFolder = back;
                TryGetItems();
            }
            else
            {
                GoToHome();
            }
        }

        public void Refresh()
        {
            CurrentDisplayFolder = DesiredFolder;
            TryGetItems(DesiredFolder);
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

        private Token _tryGetItemsCancelToken;

        private class Token
        {
            internal bool IsCancelled { get; set; } 
        }

        async void TryGetItems(OneDriveFolder desiredFolder)
        {
            UpdateOnLoading(true);
            ItemsSource.Clear();
            DesiredFolder = desiredFolder;
            if (_tryGetItemsCancelToken != null)
            {
                _tryGetItemsCancelToken.IsCancelled = true;
            }
            var token = _tryGetItemsCancelToken = new Token();
            var result = await OneDriveSession.GetLoggedClient().GetItemsInFolderAsync(DesiredFolder.Id);
            if (token.IsCancelled)
            {
                return;
            }
            if (result.IsSuccessful)
            {
                foreach (var oneDriveItem in result.Items)
                {
                    ItemsSource.Add(new OneDriveFileBrowserItem(oneDriveItem, ThemeProvider));
                }
                if (CurrentDisplayFolder != null && CurrentDisplayFolder != DesiredFolder)
                {
                    _backStack.Push(CurrentDisplayFolder);
                }
                CurrentDisplayFolder = DesiredFolder;
            }
            else
            {
                if (NotifyNavigationError!=null)
                {
                    NotifyNavigationError.Invoke(this, EventArgs.Empty);
                }
            }
            UpdateOnLoading(false);
            if (BackButton != null)
            {
                BackButton.IsEnabled = CanGoBack;
            }
        }

        private OneDriveFolder CurrentDisplayFolder { get; set; }

        private OneDriveFolder DesiredFolder { get; set; }

        public event EventHandler NotifyNavigationError;

    }

}
