﻿using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint.Lists.Attributes
{
    public class SPFieldNumberMetadata : SPFieldMetadata
    {
        public SPFieldNumberMetadata()
        {
            Type = SPFieldType.Number;
        }
    }
}
