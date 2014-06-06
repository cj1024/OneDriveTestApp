using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Live;

namespace OneDriveExtentions
{

    public class OneDriveInfoResult
    {

        internal OneDriveInfoResult(bool isSuccessful, IList<OneDriveItem> items)
        {
            _isSuccessful = isSuccessful;
            _isItems = true;
            _items = items;
        }

        internal OneDriveInfoResult(bool isSuccessful, OneDriveItem item)
        {
            _isSuccessful = isSuccessful;
            _isItems = false;
            _item = item;
        }

        internal OneDriveInfoResult(bool isSuccessful, LiveOperationResult result)
        {
            _isSuccessful = isSuccessful;
            if (result != null)
            {
                if (OneDriveItem.IsItems(result))
                {
                    _isItems = true;
                    _items = OneDriveItem.GetItems(result).ToList();
                }
                else if (OneDriveItem.IsItem(result))
                {
                    _item = OneDriveItem.GetItem(result);
                }
            }
        }

        private readonly bool _isSuccessful;
        public bool IsSuccessful { get { return _isSuccessful; } }
        private readonly bool _isItems;
        public bool IsItems { get { return _isItems; } }
        private readonly OneDriveItem _item;
        public OneDriveItem Item { get { return _item; } }
        private readonly IList<OneDriveItem> _items;
        public IList<OneDriveItem> Items { get { return _items; } }
    }

    public static class OneDriveInfoHelper
    {
        
        internal const string RootFolderName = "me/skydrive";
        private const string ListFileCommandName = "/files";

        internal static async Task<OneDriveInfoResult> CreateFolderInFolderAsync(this LiveConnectClient client, string desiredFolderName, string rootFolderId = RootFolderName)
        {
            var folderData = new Dictionary<string, object> { { "name", desiredFolderName } };
            var result = await client.PostAsync(rootFolderId, folderData);
            return new OneDriveInfoResult(true, OneDriveItem.GetItem(result));
        }

        public static async Task<OneDriveInfoResult> GetFolderInFolder(this LiveConnectClient client, string desiredFolderName, string rootFolderId = RootFolderName)
        {
            var result = await client.GetItemsInFolderAsync(rootFolderId);
            if (!result.IsSuccessful)
            {
                OneDriveItem empty = new OneDriveFolder();
                return new OneDriveInfoResult(false, empty);
            }
            var items = result.Items;
            var existedFolder = items.FirstOrDefault(item => item.Name.ToUpper() == desiredFolderName.ToUpper() && item.IsFolder);
            if (existedFolder != null)
            {
                return new OneDriveInfoResult(true, existedFolder);
            }
            return await CreateFolderInFolderAsync(client, desiredFolderName, rootFolderId);
        }

        public static async Task<OneDriveInfoResult> GetItemsInFolderAsync(this LiveConnectClient client, string rootFolderId = RootFolderName)
        {
            try
            {
                var result = await client.GetAsync(rootFolderId + ListFileCommandName);
                return new OneDriveInfoResult(true, OneDriveItem.GetItems(result));
            }
            catch (LiveConnectException)
            {
                IList<OneDriveItem> empty = new OneDriveItem[0];
                return new OneDriveInfoResult(false, empty);
            }
        }
        
    }

}

