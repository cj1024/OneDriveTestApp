using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;
using Microsoft.Live;

namespace OneDriveExtentions
{

    public class OneDriveFileSync
    {
        private OneDriveFileSync(){}

        static OneDriveFileSync(){}

        private static readonly OneDriveFileSync _instance = new OneDriveFileSync();

        internal static OneDriveFileSync GetInstance()
        {
            return _instance;
        }

        /// <summary>
        /// 文件同步
        /// </summary>
        /// <param name="file">文件</param>
        /// <param name="targetFolderId">OneDrive文件夹Id</param>
        /// <param name="targetFileName">OneDrive目标文件名</param>
        /// <param name="queue">同步队列</param>
        internal void SyncFile(IStorageFile file, string targetFolderId, string targetFileName, OneDriveFileSyncQueue queue)
        {
            if (queue == null)
            {
                queue = OneDriveFileSyncPool.CreateQueue();
            }
            queue.Enqueue(new OneDriveFileSyncTask
                          {
                              File = file,
                              FileName = targetFileName,
                              FolderId = targetFolderId
                          });
            OneDriveFileSyncPool.NotifyTryStartOneTask();
        }

        /// <summary>
        /// 文件夹同步
        /// </summary>
        /// <param name="folder">需要同步的文件夹</param>
        /// <param name="targetFolderId">OneDrive上的目录id</param>
        /// <param name="isRecursive">是否递归同步子文件夹，会较慢</param>
        /// <returns>同步结果</returns>
        /// <param name="queue">同步队列</param>
        internal async void SyncFolderAsync(IStorageFolder folder, string targetFolderId = OneDriveInfoHelper.RootFolderName, bool isRecursive = false, OneDriveFileSyncQueue queue = null)
        {
            if (!OneDriveSession.IsLogged)
            {
                return;
            }
            var client = OneDriveSession.GetLoggedClient();
            var onlineResult = await client.GetItemsInFolderAsync(targetFolderId);
            if (!onlineResult.IsSuccessful)
            {
                return;
            }
            if (queue == null)
            {
                queue = OneDriveFileSyncPool.CreateQueue();
            }
            var onlineItems = onlineResult.Items;
            var items = await folder.GetItemsAsync();
            foreach (var item in items)
            {
                var realItem = (IStorageItem)item;
                if (realItem.IsOfType(StorageItemTypes.Folder))
                {
                    var desiredFolder = (IStorageFolder)realItem;
                    if (isRecursive)
                    {
                        var existedFolder = onlineItems.FirstOrDefault(oitem => oitem.Name.ToUpper() == desiredFolder.Name.ToUpper() && oitem.Type.HasFlag(OneDriveItemType.Folder));
                        if (existedFolder != null)
                        {
                            SyncFolderAsync(desiredFolder, existedFolder.Id, true);
                        }
                        else
                        {
                            var createdFolderResult = await client.CreateFolderInFolderAsync(desiredFolder.Name, targetFolderId);
                            if (createdFolderResult.IsSuccessful)
                            {
                                SyncFolderAsync(desiredFolder, createdFolderResult.Item.Id, true, queue);
                            }
                        }
                    }
                }
                else if (realItem.IsOfType(StorageItemTypes.File))
                {
                    var desiredItem = (IStorageFile)realItem;
                    var existedItem = onlineItems.FirstOrDefault(oitem => oitem.Name.ToUpper() == desiredItem.Name.ToUpper() && !oitem.Type.HasFlag(OneDriveItemType.Folder));
                    if (existedItem != null)
                    {
                        SyncFile(desiredItem, targetFolderId, desiredItem.Name, queue);
                    }
                }
            }
        }
        
    }

    public class OneDriveFileSyncTask
    {
        internal IStorageFile File { get; set; }
        internal string FolderId { get; set; }
        internal string FileName { get; set; }

        internal async void RunTaskAsync(LiveConnectClient client, Action<bool> callback)
        {
            bool succeeded = true;
            using (var stream = await File.OpenStreamForReadAsync())
            {
                try
                {
                    await client.UploadAsync(FolderId, FileName, stream, OverwriteOption.Overwrite);
                }
                catch (Exception)
                {
                    succeeded = false;
                }
            }
            if (callback != null)
            {
                callback(succeeded);
            }
        }

    }

    public class OneDriveFileSyncQueue : Queue<OneDriveFileSyncTask>
    {
        internal OneDriveFileSyncQueue() { }

    }

    public class OneDriveFileSyncPool
    {

        private static readonly IList<OneDriveFileSyncQueue> _pool = new List<OneDriveFileSyncQueue>();

        internal static OneDriveFileSyncQueue CreateQueue()
        {
            lock (_pool)
            {
                var result = new OneDriveFileSyncQueue();
                _pool.Add(result);
                return result;
            }
        }

        public static IReadOnlyList<OneDriveFileSyncQueue> GetAllQueue()
        {
            lock (_pool)
            {
                return _pool.ToArray();
            }
        }

        public static void RemoveQueue(OneDriveFileSyncQueue queue)
        {
            lock (_pool)
            {
                if (_pool.Contains(queue))
                {
                    _pool.Remove(queue);
                }
            }
        }

        public static void ClearQueue()
        {
            lock (_pool)
            {
                _pool.Clear();
            }
        }

        internal static void NotifyTryStartOneTask()
        {
            if (OneDriveSession.IsLogged)
            {
                var client = OneDriveSession.GetLoggedClient();
                var _queueEnumerator = _pool.GetEnumerator();
                if (_pool.Any())
                {
                    while (_queueEnumerator.MoveNext())
                    {
                        if (_queueEnumerator.Current.Any())
                        {
                            var task = _queueEnumerator.Current.Dequeue();
                            task.RunTaskAsync(client, succeeded =>
                                                 {
                                                     if (succeeded)
                                                     {
                                                         if (FileSynced != null)
                                                         {
                                                             FileSynced.Invoke(task, task.File);
                                                         }
                                                     }
                                                     else
                                                     {
                                                         //失败，则重新加入队列
                                                         _queueEnumerator.Current.Enqueue(task);
                                                     }
                                                     NotifyTryStartOneTask();
                                                 });
                            return;
                        }
                    }
                }
            }
        }

        public static event EventHandler<IStorageFile> FileSynced;

    }


}
