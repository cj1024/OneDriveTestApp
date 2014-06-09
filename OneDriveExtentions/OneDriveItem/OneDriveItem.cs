using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Live;

namespace OneDriveExtentions
{

    public abstract class OneDriveProperty
    {
        
    }

    public class OneDrivePropertyFrom : OneDriveProperty
    {

        [OneDriveItemReflectVisibile]
        public string Name { get; protected set; }

        [OneDriveItemReflectVisibile]
        public string Id { get; protected set; }
    }

    public class OneDrivePropertyShare : OneDriveProperty
    {

        [OneDriveItemReflectVisibile]
        public string Access { get; protected set; }

    }

    internal abstract class OneDriveReflector
    {

        private readonly static IDictionary<Type, IList<PropertyInfo>> _propertyInfos = new Dictionary<Type, IList<PropertyInfo>>();

        private static IEnumerable<PropertyInfo> GetPropertyInfos<T>(T target)
        {
            var type = target.GetType();
            if (!_propertyInfos.ContainsKey(type))
            {
                var propertyInfos = type.GetProperties().Where(p => p.GetCustomAttributes(typeof(OneDriveItemReflectVisibileAttribute), true).Length > 0).ToList();
                _propertyInfos.Add(type, propertyInfos);
            }
            return _propertyInfos[type];
        }

        internal static void ReflectProperties<T>(ref T target, IDictionary<string, object> properties)
        {
            foreach (var pinfo in GetPropertyInfos(target))
            {
                //存在则反射
                if (properties.ContainsKey(pinfo.Name.ToLower()))
                {
                    var value = properties[pinfo.Name.ToLower()];
                    if (pinfo.PropertyType == typeof(DateTime))
                    {
                        value = DateTime.Parse(properties[pinfo.Name.ToLower()].ToString());
                    }
                    else if (pinfo.PropertyType.IsSubclassOf(typeof(OneDriveProperty)) && value is IDictionary<string, object>)
                    {
                        object realValue = Activator.CreateInstance(pinfo.PropertyType);
                        ReflectProperties(ref realValue, (IDictionary<string, object>)value);
                        value = realValue;
                    }
                    try
                    {
                        pinfo.SetValue(target, value, null);
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

    }

    /// <summary>
    /// Parse Item
    /// </summary>
    public partial class OneDriveItem
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
                    case "photo":
                        result = new OneDrivePhoto();
                        break;
                    case "audio":
                        result = new OneDriveAudio();
                        break;
                    case "video":
                        result = new OneDriveVideo();
                        break;
                    default:
                        foreach (var property in properties)
                        {
                            Debug.WriteLine("{0}  :  {1}", property.Key, property.Value);
                        }
                        result = new OneDriveItem();
                        break;
                }
                OneDriveReflector.ReflectProperties(ref result, properties);
            }
            return result;
        }

    }

    /// <summary>
    /// Properties
    /// </summary>
    partial class OneDriveItem
    {

        internal OneDriveItem()
        {
            ItemType = OneDriveItemType.Unknow;
        }

        public OneDriveItemType ItemType { get; protected set; }

        [OneDriveItemReflectVisibile]
        public string Id { get; protected set; }

        [OneDriveItemReflectVisibile]
        public OneDrivePropertyFrom From { get; protected set; }

        [OneDriveItemReflectVisibile]
        public string Name { get; protected set; }

        [OneDriveItemReflectVisibile]
        public string Description { get; protected set; }

        [OneDriveItemReflectVisibile]
        public string Parent_Id { get; protected set; }

        [OneDriveItemReflectVisibile]
        public long Size { get; protected set; }

        [OneDriveItemReflectVisibile]
        public int Comments_Count { get; protected set; }

        [OneDriveItemReflectVisibile]
        public bool Comments_Enabled { get; protected set; }

        [OneDriveItemReflectVisibile]
        public bool Is_Embeddable { get; protected set; }

        [OneDriveItemReflectVisibile]
        public string Link { get; protected set; }

        [OneDriveItemReflectVisibile]
        public string Type { get; protected set; }

        [OneDriveItemReflectVisibile]
        public OneDrivePropertyShare Shared_With { get; protected set; }

        [OneDriveItemReflectVisibile]
        public DateTime Created_Time { get; protected set; }

        [OneDriveItemReflectVisibile]
        public DateTime Updated_Time { get; protected set; }

        [OneDriveItemReflectVisibile]
        public DateTime Client_Updated_Time { get; protected set; }

    }

    #region NoteBook

    public class OneDriveNoteBook : OneDriveItem
    {

        internal OneDriveNoteBook()
        {
            ItemType = OneDriveItemType.NoteBook;
        }

    }

    #endregion

    public abstract class OneDriveSupportUploadItem : OneDriveItem
    {

        [OneDriveItemReflectVisibile]
        public string Upload_Location { get; protected set; }

    }

    #region Folder

    public class OneDriveFolder : OneDriveSupportUploadItem
    {

        private static readonly OneDriveFolder _rootFolder = new OneDriveFolder
        {
            ItemType = OneDriveItemType.Folder,
            Id = "me/skydrive"
        };

        public static OneDriveFolder RootFolder
        {
            get { return _rootFolder; }
        }

        internal OneDriveFolder()
        {
            ItemType = OneDriveItemType.Folder;
        }

        [OneDriveItemReflectVisibile]
        public int Count { get; protected set; }

    }

    public class OneDriveAlbum : OneDriveFolder
    {

        internal OneDriveAlbum()
        {
            ItemType = OneDriveItemType.Album;
        }

    }

    #endregion

    #region File

    public class OneDriveFile : OneDriveSupportUploadItem
    {

        internal OneDriveFile()
        {
            ItemType = OneDriveItemType.File;
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

    }

    public abstract class OneDriveMediaFileBase : OneDriveFile
    {

        [OneDriveItemReflectVisibile]
        public string Picture { get; protected set; }

    }

    public class OneDrivePhoto : OneDriveMediaFileBase
    {

        internal OneDrivePhoto()
        {
            ItemType = OneDriveItemType.Photo;
        }

        [OneDriveItemReflectVisibile]
        public int Tags_Count { get; protected set; }

        [OneDriveItemReflectVisibile]
        public bool Tags_Enabled { get; protected set; }

        [OneDriveItemReflectVisibile]
        public int Height { get; protected set; }

        [OneDriveItemReflectVisibile]
        public int Width { get; protected set; }

    }

    public class OneDriveAudio : OneDriveMediaFileBase
    {

        internal OneDriveAudio()
        {
            ItemType = OneDriveItemType.Audio;
        }

        [OneDriveItemReflectVisibile]
        public string Title { get; protected set; }

        [OneDriveItemReflectVisibile]
        public string Artist { get; protected set; }

        [OneDriveItemReflectVisibile]
        public string Album { get; protected set; }

        [OneDriveItemReflectVisibile]
        public string Album_Artist { get; protected set; }

        [OneDriveItemReflectVisibile]
        public string Genre { get; protected set; }

        [OneDriveItemReflectVisibile]
        public long Duration { get; protected set; }

    }

    public class OneDriveVideo : OneDriveMediaFileBase
    {

        internal OneDriveVideo()
        {
            ItemType = OneDriveItemType.Video;
        }

        [OneDriveItemReflectVisibile]
        public int Tags_Count { get; protected set; }

        [OneDriveItemReflectVisibile]
        public bool Tags_Enabled { get; protected set; }

        [OneDriveItemReflectVisibile]
        public int Height { get; protected set; }

        [OneDriveItemReflectVisibile]
        public int Width { get; protected set; }

        [OneDriveItemReflectVisibile]
        public long Duration { get; protected set; }

        [OneDriveItemReflectVisibile]
        public long BitRate { get; protected set; }

    }

    #endregion

}
