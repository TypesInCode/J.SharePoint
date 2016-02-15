using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace J.SharePoint.Lists.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class SPContentTypeMetadata : Attribute
    {
        public string ContentTypeId
        { get; set; }

        public string ParentContentType
        { get; set; }

        public string Name
        { get; set; }

        public string Group
        { get; set; }

        public string Description
        { get; set; }

        public static IEnumerable<SPContentTypeMetadata> Get(Type type)
        {
            return type.GetCustomAttributes(typeof(SPContentTypeMetadata), true).Cast<SPContentTypeMetadata>();
        }

        public SPContentType GetContentType(SPContentTypeCollection contentTypeCollection)
        {
            if (!string.IsNullOrEmpty(ContentTypeId))
                return contentTypeCollection[new SPContentTypeId(ContentTypeId)];

            return contentTypeCollection[Name];
        }

        public void AddContentTypeTo(SPContentTypeCollection contentTypeCollection, SPWeb parentWeb = null)
        {
            SPContentType newCt = GetContentType(contentTypeCollection);
            if (newCt != null)
                return;

            if (parentWeb != null && ((newCt = GetContentType(parentWeb.Site.RootWeb.AvailableContentTypes)) != null))
            {
                contentTypeCollection.Add(newCt);
            }
            else
            {
                if (!string.IsNullOrEmpty(ContentTypeId))
                {
                    newCt = new SPContentType(new SPContentTypeId(ContentTypeId), contentTypeCollection, Name);
                }
                else
                {
                    SPContentType parentCt = parentWeb.AvailableContentTypes[ParentContentType];
                    newCt = new SPContentType(parentCt, contentTypeCollection, Name);
                }

                if (!string.IsNullOrEmpty(Group))
                    newCt.Group = Group;
                if (!string.IsNullOrEmpty(Description))
                    newCt.Description = Description;

                try { contentTypeCollection.Add(newCt); }
                catch (SPException)
                {
                    parentWeb.Site.RootWeb.ContentTypes.Add(newCt);
                    newCt = parentWeb.Site.RootWeb.ContentTypes[newCt.Id];
                    contentTypeCollection.Add(newCt);
                }
            }
        }
    }
}
