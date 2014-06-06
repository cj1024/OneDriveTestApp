using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Live;

namespace OneDriveExtentions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OneDriveItemReflectVisibileAttribute : Attribute
    {

    }

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

    public abstract class OneDriveItem
    {

        public static bool IsItems(LiveOperationResult result)
        {
            return result.Result.ContainsKey("data") && result.Result["data"] is IEnumerable<object>;
        }
        
        public static bool IsItem(LiveOperationResult result)
        {
            return result.Result.ContainsKey("id") && result.Result.ContainsKey("type");
        }

        public static IList<OneDriveItem> GetItems(LiveOperationResult result)
        {
            var results = new List<OneDriveItem>();
            if (IsItems(result))
            {
                var data = (IEnumerable<object>)result.Result["data"];
                results.AddRange(data.Cast<IDictionary<string, object>>().Select(GetItem).Where(item => item != null));
            }
            return results;
        }

        public static OneDriveItem GetItem(LiveOperationResult result)
        {
            if (IsItem(result))
            {
                return GetItem(result.Result);
            }
            return null;
        }

        private static OneDriveItem GetItem(IDictionary<string, object> properties)
        {
            OneDriveItem result = null;
            if (properties.ContainsKey("type"))
            {
                var type = properties["type"].ToString().ToLower();
                switch (type)
                {
                    case "notebook":
                        result = new OneDriveNoteBook();
                        break;
                    case "album":
                        result = new OneDriveAlbum();
                        break;
                    case "folder":
                        result = new OneDriveFolder();
                        break;
                    case "photo":
                        result = new OneDrivePhoto();
                        break;
                    case "file":
                        result = new OneDriveFile();
                        break;
                    default:
                        foreach (var property in properties)
                        {
                            Debug.WriteLine("{0}  :  {1}", property.Key, property.Value);
                        }
                        result = new OneDriveFile();
                        break;
                }
                foreach (var pinfo in result.GetPropertyInfos())
                {
                    //存在则反射
                    if (properties.ContainsKey(pinfo.Name.ToLower()))
                    {
                        var value = properties[pinfo.Name.ToLower()];
                        if (pinfo.PropertyType == typeof(DateTime))
                        {
                            value = DateTime.Parse(properties[pinfo.Name.ToLower()].ToString());
                        }
                        try
                        {
                            pinfo.SetValue(result, value, null);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("No Such Property {0} In Dictionary", pinfo.Name);
                    }
                }
            }
            return result;
        }

        protected OneDriveItemType Type { get; set; }

        public bool IsNoteBook
        {
            get { return ((int)Type & OneDriveItemTypeFlags.IsNoteBook) == OneDriveItemTypeFlags.IsNoteBook; }
        }

        public bool IsFolder
        {
            get { return ((int)Type & OneDriveItemTypeFlags.IsFile) != OneDriveItemTypeFlags.IsFile; }
        }

        public bool IsPhotoRelate
        {
            get { return ((int)Type & OneDriveItemTypeFlags.IsPhotoRelate) == OneDriveItemTypeFlags.IsPhotoRelate; }
        }

        [OneDriveItemReflectVisibileAttribute]
        public string Id { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public string Name { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public string Description { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public string Parent_Id { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public long Size { get; protected set; }
        
        [OneDriveItemReflectVisibileAttribute]
        public int Comments_Count { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public bool Comments_Enabled { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public bool Is_Embeddable { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public string Link { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public DateTime Created_Time { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public DateTime Updated_Time { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public DateTime Client_Updated_Time { get; protected set; }

        protected abstract IEnumerable<PropertyInfo> GetPropertyInfos();

    }

    public abstract class OneDriveSupportUploadItem : OneDriveItem
    {
        [OneDriveItemReflectVisibileAttribute]
        public string Upload_Location { get; protected set; }
    }

    public class OneDriveNoteBook : OneDriveItem
    {
        private static IList<PropertyInfo> _propertyInfos;

        protected override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            if (_propertyInfos == null)
            {
                var type = GetType();
                _propertyInfos = type.GetProperties().Where(p => p.GetCustomAttributes(typeof(OneDriveItemReflectVisibileAttribute), true).Length > 0).ToList();
            }
            return _propertyInfos;
        }

    }

    public class OneDriveFolder : OneDriveSupportUploadItem
    {

        private static readonly OneDriveFolder _rootFolder = new OneDriveFolder
                                                             {
                                                                 Type = OneDriveItemType.Folder,
                                                                 Id = "me/skydrive"
                                                             };
        public static OneDriveFolder RootFolder
        {
            get { return _rootFolder; }
        }

        internal OneDriveFolder()
        {
            Type = OneDriveItemType.Folder;
        }

        [OneDriveItemReflectVisibileAttribute]
        public int Count { get; protected set; }

        private static IList<PropertyInfo> _propertyInfos;

        protected override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            if (_propertyInfos == null)
            {
                var type = GetType();
                _propertyInfos = type.GetProperties().Where(p => p.GetCustomAttributes(typeof(OneDriveItemReflectVisibileAttribute), true).Length > 0).ToList();
            }
            return _propertyInfos;
        }

    }

    public class OneDriveAlbum : OneDriveFolder
    {

        internal OneDriveAlbum()
        {
            Type = OneDriveItemType.Album;
        }

        private static IList<PropertyInfo> _propertyInfos;

        protected override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            if (_propertyInfos == null)
            {
                var type = GetType();
                _propertyInfos = type.GetProperties().Where(p => p.GetCustomAttributes(typeof(OneDriveItemReflectVisibileAttribute), true).Length > 0).ToList();
            }
            return _propertyInfos;
        }

    }

    public class OneDriveFile : OneDriveSupportUploadItem
    {

        internal OneDriveFile()
        {
            Type = OneDriveItemType.File;
        }

        [OneDriveItemReflectVisibileAttribute]
        public string Source { get; protected set; }

        private static IList<PropertyInfo> _propertyInfos;

        protected override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            if (_propertyInfos == null)
            {
                var type = GetType();
                _propertyInfos = type.GetProperties().Where(p => p.GetCustomAttributes(typeof (OneDriveItemReflectVisibileAttribute), true).Length > 0).ToList();
            }
            return _propertyInfos;
        }

    }

    public class OneDrivePhoto : OneDriveFile
    {
        internal OneDrivePhoto()
        {
            Type = OneDriveItemType.Photo;
        }

        [OneDriveItemReflectVisibileAttribute]
        public int Tags_Count { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public bool Tags_Enabled { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public string Picture { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public int Height { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public int Width { get; protected set; }

        private static IList<PropertyInfo> _propertyInfos;

        protected override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            if (_propertyInfos == null)
            {
                var type = GetType();
                _propertyInfos = type.GetProperties().Where(p => p.GetCustomAttributes(typeof(OneDriveItemReflectVisibileAttribute), true).Length > 0).ToList();
            }
            return _propertyInfos;
        }

    }

}
