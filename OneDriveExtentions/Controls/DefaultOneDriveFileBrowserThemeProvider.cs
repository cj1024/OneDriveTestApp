using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OneDriveExtentions.Controls
{

    partial class DefaultOneDriveFileBrowserThemeProvider
    {

        private static readonly IDictionary<string, Brush> FileIconBrushesDictionary = new Dictionary<string, Brush>
        {
            { "doc", new ImageBrush { ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileTypeIcons/doc.png", UriKind.Relative)), Stretch=Stretch.UniformToFill } },
            { "docx", new ImageBrush { ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileTypeIcons/docx.png", UriKind.Relative)), Stretch=Stretch.UniformToFill } },
            { "xls", new ImageBrush { ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileTypeIcons/xls.png", UriKind.Relative)), Stretch=Stretch.UniformToFill } },
            { "xlsx", new ImageBrush { ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileTypeIcons/xlsx.png", UriKind.Relative)), Stretch=Stretch.UniformToFill } },
            { "ppt", new ImageBrush { ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileTypeIcons/ppt.png", UriKind.Relative)), Stretch=Stretch.UniformToFill } },
            { "pptx", new ImageBrush { ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileTypeIcons/pptx.png", UriKind.Relative)), Stretch=Stretch.UniformToFill } },
            { "txt", new ImageBrush { ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileTypeIcons/note.png", UriKind.Relative)), Stretch=Stretch.UniformToFill } },
            { "xml", new ImageBrush { ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileTypeIcons/code.png", UriKind.Relative)), Stretch=Stretch.UniformToFill } },
            { "php", new ImageBrush { ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileTypeIcons/web.png", UriKind.Relative)), Stretch=Stretch.UniformToFill } },
            { "htm", new ImageBrush { ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileTypeIcons/web.png", UriKind.Relative)), Stretch=Stretch.UniformToFill } },
            { "html", new ImageBrush { ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileTypeIcons/web.png", UriKind.Relative)), Stretch=Stretch.UniformToFill } },
        };

        protected static Brush GetIconBrushForFileType(OneDriveFile item)
        {
            var extention = item.FileExtention.ToLower();
            if (FileIconBrushesDictionary.ContainsKey(extention))
            {
                return FileIconBrushesDictionary[extention];
            }
            return _fileBrush;
        }

    }

    public partial class DefaultOneDriveFileBrowserThemeProvider : IOneDriveFileBrowserThemeProvider
    {

        static DefaultOneDriveFileBrowserThemeProvider()
        {
        }

        private static readonly Brush _fileBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FileIcon.png", UriKind.Relative)),
            Stretch = Stretch.UniformToFill
        };
        private static readonly Brush _filePhotoBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/PhotoIcon.png", UriKind.Relative)),
            Stretch = Stretch.UniformToFill
        };
        private static readonly Brush _fileVideoBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/VideoIcon.png", UriKind.Relative)),
            Stretch = Stretch.UniformToFill
        };
        private static readonly Brush _folderBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/FolderIcon.png", UriKind.Relative)),
            Stretch = Stretch.UniformToFill
        };
        private static readonly Brush _folderEmptyBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/EmptyFolderIcon.png", UriKind.Relative)),
            Stretch = Stretch.UniformToFill
        };
        private static readonly Brush _folderAlbumBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/AlbumFolderIcon.png", UriKind.Relative)),
            Stretch = Stretch.UniformToFill
        };
        private static readonly Brush _notebookBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/NoteBookIcon.png", UriKind.Relative)),
            Stretch = Stretch.UniformToFill
        };
        private static readonly Brush _unknownBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri("/OneDriveExtentions;component/Icons/UnknownIcon.png", UriKind.Relative)),
            Stretch = Stretch.UniformToFill
        };

        public Brush GetBrushForItem(OneDriveItem item)
        {
            switch (item.Type)
            {
                case OneDriveItemType.NoteBook:
                    return _notebookBrush;
                case OneDriveItemType.Folder:
                    return ((OneDriveFolder)item).Count > 0 ? _folderBrush : _folderEmptyBrush;
                case OneDriveItemType.Album:
                    return _folderAlbumBrush;
                case OneDriveItemType.File:
                    return GetIconBrushForFileType((OneDriveFile)item);
                case OneDriveItemType.Photo:
                    return _filePhotoBrush;
                case OneDriveItemType.Video:
                    return _fileVideoBrush;
                default:
                    return _unknownBrush;
            }
        }

        private static readonly Size _iconSize = new Size(48, 48);

        public Size GetIconSizeForItem(OneDriveItem item)
        {
            return _iconSize;
        }

    }

}
