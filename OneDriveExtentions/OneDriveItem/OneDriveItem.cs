using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Live;

namespace OneDriveExtentions
{

    public partial class OneDriveItem
    {

        internal OneDriveItem()
        {
            Type = OneDriveItemType.Unknow;
        }

        public OneDriveItemType Type { get; protected set; }

        [OneDriveItemReflectVisibileAttribute]
        public string Id { get; protected set; }

        [OneDriveItemReflectVisibile]
        public virtual string Name { get; protected set; }

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

        private static IList<PropertyInfo> _propertyInfos;

        protected virtual IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            if (_propertyInfos == null)
            {
                var type = GetType();
                _propertyInfos = type.GetProperties().Where(p => p.GetCustomAttributes(typeof(OneDriveItemReflectVisibileAttribute), true).Length > 0).ToList();
            }
            return _propertyInfos;
        }

    }

    /// <summary>
    /// Parse Item
    /// </summary>
    partial class OneDriveItem
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
                    case "folder":
                        result = new OneDriveFolder();
                        break;
                    case "album":
                        result = new OneDriveAlbum();
                        break;
                    case "file":
                        result = new OneDriveFile();
                        break;
                    case "video":
                        result = new OneDriveVideo();
                        break;
                    case "photo":
                        result = new OneDrivePhoto();
                        break;
                    default:
                        foreach (var property in properties)
                        {
                            Debug.WriteLine("{0}  :  {1}", property.Key, property.Value);
                        }
                        result = new OneDriveItem();
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

    }

    #region NoteBook

    public class OneDriveNoteBook : OneDriveItem
    {

        internal OneDriveNoteBook()
        {
            Type = OneDriveItemType.NoteBook;
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

    #endregion

    public abstract class OneDriveSupportUploadItem : OneDriveItem
    {

        [OneDriveItemReflectVisibileAttribute]
        public string Upload_Location { get; protected set; }

    }

    #region Folder

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

        [OneDriveItemReflectVisibile]
        public int Count { get; protected set; }

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
                _propertyInfos = type.GetProperties().Where(p => p.GetCustomAttributes(typeof (OneDriveItemReflectVisibileAttribute), true).Length > 0).ToList();
            }
            return _propertyInfos;
        }

    }

    #endregion

    #region File

    public class OneDriveFile : OneDriveSupportUploadItem
    {

        internal OneDriveFile()
        {
            Type = OneDriveItemType.File;
        }

        [OneDriveItemReflectVisibile]
        public string Source { get; protected set; }

        public string FileExtention
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    var segments = Name.Split('.');
                    if (segments.Length > 1)
                    {
                        return segments.Last();
                    }
                }
                return string.Empty;
            }
        }

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

    public abstract class OneDriveMediaFileBase : OneDriveFile
    {

        [OneDriveItemReflectVisibile]
        public int Tags_Count { get; protected set; }

        [OneDriveItemReflectVisibile]
        public bool Tags_Enabled { get; protected set; }

        [OneDriveItemReflectVisibile]
        public string Picture { get; protected set; }

        [OneDriveItemReflectVisibile]
        public int Height { get; protected set; }

        [OneDriveItemReflectVisibile]
        public int Width { get; protected set; }

    }

    public class OneDrivePhoto : OneDriveMediaFileBase
    {

        internal OneDrivePhoto()
        {
            Type = OneDriveItemType.Photo;
        }

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

    public class OneDriveVideo : OneDriveMediaFileBase
    {

        internal OneDriveVideo()
        {
            Type = OneDriveItemType.Video;
        }

        [OneDriveItemReflectVisibile]
        public long Duration { get; protected set; }

        [OneDriveItemReflectVisibile]
        public long BitRate { get; protected set; }

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

    #endregion

}
