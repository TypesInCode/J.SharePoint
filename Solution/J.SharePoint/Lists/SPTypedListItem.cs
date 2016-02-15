using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using J.SharePoint.Lists.Attributes;
using System.Reflection;

namespace J.SharePoint.Lists
{
    public class SPTypedListItem
    {
        public const string TitleInternal = "Title";
        public const string ContentTypeInternal = "ContentType";
        public const string ContentTypeIdInternal = "ContentTypeId";
        public const string CreatedInternal = "Created";
        public const string ModifiedInternal = "Modified";
        public const string AuthorInternal = "Author";
        public const string EditorInternal = "Editor";

        private SPListItem _item;
        private bool _throwFieldErrors;

        public SPListItem Item
        { 
            get 
            { return _item; }
            protected set
            {
                _item = value;
                ReadFromListItem();
            }
        }

        public int ID
        { get; private set; }

        [SPFieldTextMetadata(InternalName = TitleInternal)]
        public string Title
        { get; set; }

        [SPFieldDateTimeMetadata(InternalName = CreatedInternal)]
        public DateTime Created
        { get; set; }

        [SPFieldDateTimeMetadata(InternalName = ModifiedInternal)]
        public DateTime Modified
        { get; set; }

        [SPFieldUserMetadata(InternalName = AuthorInternal)]
        public string CreatedBy
        { get; set; }

        [SPFieldUserMetadata(InternalName = EditorInternal)]
        public string ModifiedBy
        { get; set; }

        [SPFieldTextMetadata(InternalName = ContentTypeInternal)]
        public string ContentType
        { get; set; }

        [SPFieldTextMetadata(InternalName = ContentTypeIdInternal)]
        public SPContentTypeId ContentTypeId
        { get; set; }

        public SPTypedListItem()
        { }

        public SPTypedListItem(SPListItem item, bool throwFieldErrors = false)
        {
            _throwFieldErrors = throwFieldErrors;
            Item = item;
        }

        public void Update()
        {
            WriteToListItem();
            _item.Update();
        }

        public void SystemUpdate(bool incrementListVersion = false)
        {
            WriteToListItem();
            _item.SystemUpdate(incrementListVersion);
        }

        public void Refresh()
        {
            _item = _item.ParentList.GetItemById(ID);
            ReadFromListItem();
        }

        private void ReadFromListItem()
        {
            ID = _item.ID;
            foreach(PropertyInfo pInfo in SPFieldMetadata.GetProperties(this.GetType()))
            {
                try
                {
                    SPFieldMetadata metadata = SPFieldMetadata.Get(pInfo);
                    object value;
                    try { value = metadata.GetFieldValue(_item); }
                    catch (ArgumentException e)
                    {
                        if (_throwFieldErrors)
                            throw e;

                        continue;
                    }
                    pInfo.GetSetMethod(true).Invoke(this, new[] { value });
                }
                catch(Exception e)
                {
                    throw new Exception(string.Format("Error processing list item. Property: {0} -- Item: {1} -- Inner Exception: {2}", pInfo.Name, _item != null ? _item.Title : "NULL", e.ToString()));
                }
            }
        }

        private void WriteToListItem()
        {
            foreach(PropertyInfo pInfo in SPFieldMetadata.GetProperties(this.GetType()))
            {
                SPFieldMetadata metadata = SPFieldMetadata.Get(pInfo);
                try { metadata.SetFieldValue(_item, pInfo.GetGetMethod(true).Invoke(this, null)); }
                catch(ArgumentException e)
                {
                    if (_throwFieldErrors)
                        throw e;
                }
            }
        }

        public static T CreateTypedItem<T>(SPListItem item, bool throwFieldErrors = false) where T : SPTypedListItem, new()
        {
            T typedItem = new T();
            typedItem._throwFieldErrors = throwFieldErrors;
            typedItem.Item = item;
            return typedItem;
        }
    }
}
