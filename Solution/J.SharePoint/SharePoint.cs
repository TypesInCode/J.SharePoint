using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint
{
    public static class SharePoint
    {
        public static SPQuery EmptyQuery
        {
            get
            {
                return new SPQuery();
            }
        }
    }
}
