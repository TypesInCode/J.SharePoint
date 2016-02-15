using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace J.SharePoint.Lists.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class SPListMetadata : Attribute
    {
        public string Title
        { get; set; }

        public string Url
        { get; set; }

        public string Description
        { get; set; }

        public SPListTemplateType ListTemplateType
        { get; set; }

        public bool ContentTypesEnabled
        { get; set; }

        public bool EnableVersioning
        { get; set; }

        private string InitialName
        {
            get
            {
                if (!string.IsNullOrEmpty(Url))
                    return Url;

                return Title;
            }
        }

        public SPListMetadata()
        {
            ListTemplateType = SPListTemplateType.GenericList;
        }

        public virtual void AddListTo(SPListCollection listCollection)
        {
            Guid guid = listCollection.Add(InitialName, Description, ListTemplateType);
            SPList list = listCollection[guid];
            list.Title = Title;
            list.ContentTypesEnabled = ContentTypesEnabled;
            list.EnableVersioning = EnableVersioning;
            list.Update();
        }

        public static SPListMetadata Get(Type listMetadataType)
        {
            return (SPListMetadata)listMetadataType.GetCustomAttributes(typeof(SPListMetadata), true).FirstOrDefault();
        }

        public SPList GetList(SPWeb web, bool create = false)
        {
            SPList list = null;
            try { list = web.Lists[Title]; }
            catch (ArgumentException)
            {
                if (create)
                {
                    AddListTo(web.Lists);
                    list = web.Lists[Title];
                }
            }

            return list;
        }
    }
}
