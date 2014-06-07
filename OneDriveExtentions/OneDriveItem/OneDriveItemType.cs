using System;
namespace OneDriveExtentions
{

    [Flags]
    public enum OneDriveItemType
    {
        Unknow = 0,
        NoteBook = 1,
        Folder = 1 << 1,
        File = 1 << 2,
        Album = Folder | 1,
        Photo = File | 1,
        Video = Photo | (1 << 1),
    }

}
