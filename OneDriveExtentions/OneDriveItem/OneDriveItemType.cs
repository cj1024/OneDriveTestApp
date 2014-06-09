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
        Album = Folder | (1 << 3),
        Photo = File | (1 << 3),
        Audio = File | (1 << 4),
        Video = File | (1 << 5),
    }

}
