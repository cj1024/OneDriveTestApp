using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
            return await client.CreateFolderInFolderAsync(desiredFolderName, rootFolderId, CancellationToken.None);
        }

        internal static async Task<OneDriveInfoResult> CreateFolderInFolderAsync(this LiveConnectClient client, string desiredFolderName, string rootFolderId, CancellationToken cancellationToken)
        {
            var folderData = new Dictionary<string, object> { { "name", desiredFolderName } };
            var result = await client.PostAsync(rootFolderId, folderData, cancellationToken);
            return new OneDriveInfoResult(true, OneDriveItem.GetItem(result));
        }

        public static async Task<OneDriveInfoResult> GetFolderInFolder(this LiveConnectClient client, string desiredFolderName, string rootFolderId = RootFolderName)
        {
            return await client.GetFolderInFolder(desiredFolderName, rootFolderId, CancellationToken.None);
        }

        public static async Task<OneDriveInfoResult> GetFolderInFolder(this LiveConnectClient client, string desiredFolderName, string rootFolderId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await client.GetItemsInFolderAsync(rootFolderId, cancellationToken);
                if (!result.IsSuccessful)
                {
                    OneDriveItem empty = new OneDriveFolder();
                    return new OneDriveInfoResult(false, empty);
                }
                var items = result.Items;
                var existedFolder = items.FirstOrDefault(item => item.Name.ToUpper() == desiredFolderName.ToUpper() && item.ItemType.HasFlag(OneDriveItemType.Folder));
                if (existedFolder != null)
                {
                    return new OneDriveInfoResult(true, existedFolder);
                }
                return await CreateFolderInFolderAsync(client, desiredFolderName, rootFolderId, cancellationToken);
            }
            catch (TaskCanceledException e)
            {
                Debug.WriteLine(e.Message);
                OneDriveItem empty = new OneDriveFolder();
                return new OneDriveInfoResult(false, empty);
            }
        }

        public static async Task<OneDriveInfoResult> GetItemsInFolderAsync(this LiveConnectClient client, string rootFolderId = ListFileCommandName)
        {
            return await client.GetItemsInFolderAsync(rootFolderId, CancellationToken.None);
        }

        public static async Task<OneDriveInfoResult> GetItemsInFolderAsync(this LiveConnectClient client, string rootFolderId, CancellationToken cancellationToken)
        {
            try
            {
                try
                {
                    var result = await client.GetAsync(rootFolderId + ListFileCommandName, cancellationToken);
                    return new OneDriveInfoResult(true, OneDriveItem.GetItems(result));
                }
                catch (TaskCanceledException e)
                {
                    Debug.WriteLine(e.Message);
                    IList<OneDriveItem> empty = new OneDriveItem[0];
                    return new OneDriveInfoResult(false, empty);
                }
            }
            catch (LiveConnectException e)
            {
                Debug.WriteLine(e.Message);
                IList<OneDriveItem> empty = new OneDriveItem[0];
                return new OneDriveInfoResult(false, empty);
            }
        }
        
    }

}

