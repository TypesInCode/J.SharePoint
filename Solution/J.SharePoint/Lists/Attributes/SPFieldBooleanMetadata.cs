using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint.Lists.Attributes
{
    public class SPFieldBooleanMetadata : SPFieldMetadata
    {
        public SPFieldBooleanMetadata()
        {
            Type = Microsoft.SharePoint.SPFieldType.Boolean;
        }
    }
}
