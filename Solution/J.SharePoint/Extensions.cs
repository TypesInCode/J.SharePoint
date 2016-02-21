using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using J.SharePoint.Lists.Attributes;
using J.SharePoint.Lists;

namespace J.SharePoint
{
    public static partial class Extensions
    {
        public static SPField GetField(this SPFieldCollection fieldCollection, SPFieldMetadata fieldMetadata)
        {
            return fieldMetadata.GetField(fieldCollection);
        }

        public static SPContentType GetContentType(this SPContentTypeCollection contentTypeCollection, SPContentTypeMetadata contentTypeMetadata)
        {
            return contentTypeMetadata.GetContentType(contentTypeCollection);
        }

        public static SPList GetList(this SPWeb web, SPListMetadata listMetadata, bool createList = false)
        {
            return listMetadata.GetList(web, createList);
        }

        public static void EnsureContentType<T>(this SPWeb web) where T : SPTypedListItem, new()
        {
            web.ContentTypes.EnsureContentType(SPContentTypeMetadata.Get(typeof(T)), web);
            web.Fields.EnsureFields(SPFieldMetadata.GetMetadata(typeof(T)));
            web.ContentTypes.EnsureFieldLinks(SPFieldMetadata.GetMetadata(typeof(T)), web.Fields);
        }

        public static T GetList<T>(this SPWeb web, bool createList = false) where T : SPTypedList, new()
        {
            T typedList = new T();
            typedList.LoadList(web, createList);
            return typedList;
        }

        public static void Add(this SPFieldCollection fieldCollection, SPFieldMetadata fieldMetadata)
        {
            fieldMetadata.AddFieldTo(fieldCollection);
        }

        public static void Add(this SPListCollection listCollection, SPListMetadata listMetadata)
        {
            listMetadata.AddListTo(listCollection);
        }

        public static void Add(this SPContentTypeCollection contentTypeCollection, SPContentTypeMetadata contentTypeMetadata, SPWeb parentWeb = null)
        {
            contentTypeMetadata.AddContentTypeTo(contentTypeCollection, parentWeb);
        }

        public static void EnsureFields(this SPFieldCollection fieldCollection, IEnumerable<SPFieldMetadata> metadataCollection)
        {
            foreach (SPFieldMetadata metadata in metadataCollection)
            {
                if (!fieldCollection.ContainsField(metadata))
                {
                    fieldCollection.Add(metadata);
                }
            }
        }

        public static void EnsureContentType(this SPContentTypeCollection contentTypeCollection, SPContentTypeMetadata contentTypeMetadata, SPWeb parentWeb = null)
        {
            if (contentTypeCollection.GetContentType(contentTypeMetadata) == null)
            {
                contentTypeCollection.Add(contentTypeMetadata, parentWeb);
            }
        }

        public static void EnsureFieldLinks(this SPContentTypeCollection contentTypeCollection, IEnumerable<SPFieldMetadata> fieldMetadataCollection, SPFieldCollection sourceFields, SPWeb parentWeb = null)
        {
            foreach (SPFieldMetadata fieldMetadata in fieldMetadataCollection.Where(
                fmd => !string.IsNullOrEmpty(fmd.ContentType) || 
                       (fmd.ContentTypes != null && fmd.ContentTypes.Length > 0)))
            {
                if( !string.IsNullOrEmpty(fieldMetadata.ContentType))
                    fieldMetadata.AddFieldTo(contentTypeCollection[fieldMetadata.ContentType], sourceFields, parentWeb);

                if( fieldMetadata.ContentTypes != null )
                {
                    foreach (string contentType in fieldMetadata.ContentTypes)
                    {
                        fieldMetadata.AddFieldTo(contentTypeCollection[contentType], sourceFields, parentWeb);
                    }
                }
            }
        }

        public static bool ContainsField(this SPFieldCollection fieldCollection, SPFieldMetadata fieldMetadata)
        {
            return fieldMetadata.IsIn(fieldCollection);
        }

        public static void ReadFrom(this SPFieldMultiChoiceValue choiceValue, string[] values)
        {
            if (values != null)
            {
                for (int x = 0; x < values.Length; x++)
                {
                    choiceValue.Add(values[x]);
                }
            }
        }

        public static string[] ToArray(this SPFieldMultiChoiceValue choiceValue)
        {
            List<string> list = new List<string>();
            if (choiceValue != null)
            {
                for (int x = 0; x < choiceValue.Count; x++)
                {
                    list.Add(choiceValue[x]);
                }
            }
            return list.ToArray();
        }
    }
}
