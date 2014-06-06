using System.Windows;
using System.Windows.Media;

namespace OneDriveExtentions.Controls
{

    public interface IOneDriveFileBrowserThemeProvider
    {

        Brush GetBrushForItem(OneDriveItem item);

        Size GetIconSizeForItem(OneDriveItem item);

    }

}
