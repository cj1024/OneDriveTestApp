using System;
namespace OneDriveExtentions
{

    internal class OneDriveItemTypeFlags
    {
        internal const int IsNoteBook = 1;
        internal const int IsFolder = 0 << 1;
        internal const int IsFile = 1 << 1;
        internal const int IsPhotoRelate = 1 << 2;
    }

    [Flags]
    public enum OneDriveItemType
    {
        NoteBook = OneDriveItemTypeFlags.IsNoteBook,
        Folder = OneDriveItemTypeFlags.IsFolder,
        Album = OneDriveItemTypeFlags.IsFolder | OneDriveItemTypeFlags.IsPhotoRelate,
        File = OneDriveItemTypeFlags.IsFile,
        Photo = OneDriveItemTypeFlags.IsFile | OneDriveItemTypeFlags.IsPhotoRelate,
    }

}
