using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Microsoft.SharePoint;
using J.SharePoint.Lists.Attributes;
using System.Linq.Expressions;
using J.SharePoint.Lists.Expressions;
using System.Reflection;
using Microsoft.Office.DocumentManagement.DocumentSets;

namespace J.SharePoint.Lists
{
    public abstract class SPTypedList : SPItemEventReceiver
    {
        protected SPList _list = null;
        protected bool _throwFieldErrors;

        public virtual SPEventReceiverType[] EventReceivers
        { get { return new SPEventReceiverType[] { }; } }

        public SPList List
        { get { return _list; } }

        public SPFolder RootFolder
        { get { return List.RootFolder; } }

        public void LoadList(SPWeb web, bool createList = false)
        {
            _list = web.GetList(SPListMetadata.Get(this.GetType()), createList);
        }

        public void EnsureEventReceivers()
        {
            Type thisType = this.GetType();
            List<SPEventReceiverDefinition> registeredReceivers = new List<SPEventReceiverDefinition>();
            foreach (SPEventReceiverDefinition rDef in List.EventReceivers)
            {
                if (rDef.Class.Equals(thisType.FullName, StringComparison.InvariantCultureIgnoreCase))
                {
                    registeredReceivers.Add(rDef);
                }
            }
            registeredReceivers.ForEach(r => r.Delete());

            foreach (SPEventReceiverType rType in EventReceivers)
            {
                List.EventReceivers.Add(rType, thisType.Assembly.FullName, thisType.FullName);
            }
        }
    }

    public class SPTypedList<T> : SPTypedList where T : SPTypedListItem, new()
    {
        public SPTypedListItemCollection<T> Items
        {
            get
            {
                return new SPTypedListItemCollection<T>(List, _throwFieldErrors);
            }
        }

        private SPFieldMetadataCollection<T> _fieldMetadataCollection = new SPFieldMetadataCollection<T>();
        public SPFieldMetadataCollection<T> FieldMetadata
        { get { return _fieldMetadataCollection; } }

        public SPTypedList(bool throwFieldErrors = false)
        {
            _throwFieldErrors = throwFieldErrors;
        }

        public SPTypedList(SPList list, bool throwFieldErrors = false) : this(throwFieldErrors)
        {
            _list = list;
        }

        public T AddItem()
        {
            T item = CreateTypedItem(List.AddItem(), _throwFieldErrors);
            SPContentTypeMetadata ct = SPContentTypeMetadata.Get(typeof(T));
            if (ct != null)
                item.ContentType = ct.Name;

            return item;
        }

        public SPTypedListItemCollection<T> GetItems(SPQuery query)
        {
            return new SPTypedListItemCollection<T>(List, _throwFieldErrors);
        }

        public T GetItemById(int id)
        {
            return CreateTypedItem(_list.GetItemById(id));
        }

        public T GetItemByUniqueId(Guid id)
        {
            return CreateTypedItem(_list.GetItemByUniqueId(id));
        }

        public void EnsureList()
        {
            EnsureContentType();
            EnsureFields();
            EnsureFieldLinks();
            EnsureEventReceivers();
        }

        public void EnsureFields()
        {
            List.Fields.EnsureFields(FieldMetadata);
        }

        public void EnsureContentType()
        {
            List.ContentTypes.EnsureContentType(SPContentTypeMetadata.Get(typeof(T)), List.ParentWeb);
        }

        public void EnsureFieldLinks()
        {
            List.ContentTypes.EnsureFieldLinks(FieldMetadata, List.Fields, List.ParentWeb);
        }

        public static T CreateTypedItem(SPListItem item, bool throwFieldErrors = false)
        {
            return SPTypedListItem.CreateTypedItem<T>(item, throwFieldErrors);
        }
    }
}
