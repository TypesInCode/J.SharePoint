using Microsoft.SharePoint.Taxonomy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace J.SharePoint.Lists.Attributes
{
    public class TaxonomyFieldMetadata : SPFieldLookupMetadata
    {
        private const string FieldType = "TaxonomyFieldType";

        public string TermGroup
        { get; set; }

        public string TermSet
        { get; set; }

        public string TermStoreId
        { get; set; }

        private Guid TermStoreGuid
        { get { return !string.IsNullOrEmpty(TermStoreId) ? new Guid(TermStoreId) : System.Guid.Empty; } }

        public string TermSetId
        { get; set; }

        private Guid TermSetGuid
        { get { return !string.IsNullOrEmpty(TermSetId) ? new Guid(TermSetId) : System.Guid.Empty; } }

        public TaxonomyFieldMetadata()
        { }

        protected override SPField CreateField(Microsoft.SharePoint.SPFieldCollection fieldCollection)
        {
            TaxonomyField field = (TaxonomyField)fieldCollection.CreateNewField(FieldType, Name);
            fieldCollection.Add(field);
            return fieldCollection[Name];
        }

        public override void AddFieldTo(SPFieldCollection fieldCollection)
        {
            AddFieldTo(fieldCollection, f =>
            {
                TaxonomyField field = (TaxonomyField)f;

                if( TermStoreGuid != System.Guid.Empty && TermSetGuid != System.Guid.Empty )
                {
                    field.SspId = TermStoreGuid;
                    field.TermSetId = TermSetGuid;
                }
                else
                {
                    TaxonomySession session = new TaxonomySession(field.ParentList.ParentWeb.Site);
                    TermStore store = session.DefaultSiteCollectionTermStore != null ? session.DefaultSiteCollectionTermStore : session.TermStores[0];
                    Group group = store.Groups[TermGroup];
                    TermSet set = group.TermSets[TermSet];

                    field.SspId = store.Id;
                    field.TermSetId = set.Id;
                }

                field.AllowMultipleValues = AllowMultipleValues;
            });
        }
    }
}
