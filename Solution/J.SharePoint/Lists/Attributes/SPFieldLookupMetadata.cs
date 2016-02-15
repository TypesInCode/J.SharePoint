using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint.Lists.Attributes
{
    public class SPFieldLookupMetadata : SPFieldMetadata
    {
        public bool AllowMultipleValues
        { get; set; }

        public SPFieldLookupMetadata()
        {
            Type = SPFieldType.Lookup;
        }

        public override void AddFieldTo(SPFieldCollection fieldCollection)
        {
            throw new NotImplementedException();
        }
    }
}
