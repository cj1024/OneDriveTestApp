using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OneDriveExtentions.Controls
{

    public sealed class OneDriveMediaFileListItem : Control
    {

        public OneDriveMediaFileListItem()
        {
            DefaultStyleKey = typeof (OneDriveMediaFileListItem);
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(OneDriveFileBrowserItem), typeof(OneDriveMediaFileListItem), new PropertyMetadata(default(OneDriveFileBrowserItem), OnContentChanged));

        public OneDriveFileBrowserItem Content
        {
            get { return (OneDriveFileBrowserItem) GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource", typeof(BitmapSource), typeof(OneDriveMediaFileListItem), new PropertyMetadata(default(Uri)));

        public BitmapSource ImageSource
        {
            get { return (BitmapSource)GetValue(ImageSourceProperty); }
            private set { SetValue(ImageSourceProperty, value); }
        }

        static void OnContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((OneDriveMediaFileListItem)obj).HandleOnContentChanged();
        }

        void HandleOnContentChanged()
        {
            Uri result = null;
            if (Content != null && Content.Item is OneDriveMediaFileBase)
            {
                if (!Uri.TryCreate(((OneDriveMediaFileBase)Content.Item).Picture, UriKind.Absolute, out result))
                {
                    Uri.TryCreate(((OneDriveMediaFileBase)Content.Item).Picture, UriKind.Absolute, out result);
                }
            }
            ImageSource = result != null ? new BitmapImage(result) : null;
        }

    }

}
