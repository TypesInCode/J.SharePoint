using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint.Lists.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class SPFieldMetadata : Attribute
    {
        public string Title
        { get; set; }

        public string InternalName
        { get; set; }

        public string Description
        { get; set; }

        public string Group
        { get; set; }

        public string ContentType
        { get; set; }

        public string[] ContentTypes
        { get; set; }

        public SPFieldType Type
        { get; set; }

        public string Guid
        { get; set; }

        public bool Required
        { get; set; }

        public bool ReadOnly
        { get; set; }

        protected string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(InternalName))
                    return InternalName;

                return Title;
            }
        }

        private Guid ID
        {
            get
            {
                if( !string.IsNullOrEmpty(Guid) )
                    return new Guid(Guid);

                return System.Guid.Empty;
            }
        }

        protected virtual SPField CreateField(SPFieldCollection fieldCollection)
        {
            fieldCollection.Add(Name, Type, Required);
            return fieldCollection[Name];
        }

        protected virtual void AddFieldTo(SPFieldCollection fieldCollection, Action<SPField> fieldAction)
        {
            SPField newField = null;
            if (ID != System.Guid.Empty && fieldCollection.Web.AvailableFields.Contains(ID))
            {
                SPField field = fieldCollection.Web.AvailableFields[ID];
                fieldCollection.Add(field);
                newField = fieldCollection[ID];
            }
            else
                newField = CreateField(fieldCollection);

            if (fieldAction != null)
                fieldAction(newField);

            if (!string.IsNullOrEmpty(Title))
                newField.Title = Title;

            if (!string.IsNullOrEmpty(Description))
                newField.Description = Description;

            newField.Update();
        }

        public virtual void AddFieldTo(SPFieldCollection fieldCollection)
        {
            AddFieldTo(fieldCollection, null);
        }

        public void AddFieldTo(SPContentType contentType, SPFieldCollection sourceFields, SPWeb parentWeb = null)
        {
            if (contentType != null && !contentType.Fields.ContainsField(this))
            {
                SPFieldLink link = null;
                if (sourceFields.ContainsField(this))
                    link = new SPFieldLink(sourceFields.GetField(this));
                else if (parentWeb.AvailableFields.ContainsField(this))
                    link = new SPFieldLink(parentWeb.AvailableFields.GetField(this));

                if (link != null)
                {
                    contentType.FieldLinks.Add(link);
                    contentType.Update();
                }
            }
        }

        public virtual object GetFieldValue(SPListItem item)
        {
            if (ID != System.Guid.Empty)
                return item[ID];

            return item[Name];
        }

        public virtual void SetFieldValue(SPListItem item, object value)
        {
            item[Name] = value;
        }

        public SPField GetField(SPFieldCollection fieldCollection)
        {
            if (ID != System.Guid.Empty)
                return fieldCollection[ID];

            try { return fieldCollection[Name]; }
            catch(ArgumentException)
            { return fieldCollection[Title]; }
        }

        public bool IsIn(SPFieldCollection fieldCollection)
        {
            return (ID != System.Guid.Empty && fieldCollection.Contains(ID)) ||
                (!string.IsNullOrEmpty(Name) && fieldCollection.ContainsField(Name));
        }

        public static IEnumerable<PropertyInfo> GetProperties(Type t)
        {
            return t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                                .Where(p => p.GetCustomAttributes<SPFieldMetadata>().Any());
        }

        public static IEnumerable<SPFieldMetadata> GetMetadata(Type t)
        {
            return GetProperties(t).Select(p => p.GetCustomAttributes<SPFieldMetadata>().First());
        }

        public static SPFieldMetadata Get(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes<SPFieldMetadata>().First();
        }
    }
}