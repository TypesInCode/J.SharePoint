using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint.Lists.Attributes
{
    public class SPFieldMultiLineMetadata : SPFieldMetadata
    {
        public bool AppendOnly
        { get; set; }

        public bool RichText
        { get; set; }

        public SPRichTextMode RichTextMode
        { get; set; }

        public SPFieldMultiLineMetadata()
        {
            Type = SPFieldType.Note;
        }

        public override void AddFieldTo(SPFieldCollection fieldCollection)
        {
            AddFieldTo(fieldCollection, f =>
            {
                SPFieldMultiLineText field = (SPFieldMultiLineText)f;
                field.AppendOnly = AppendOnly;
                field.RichText = RichText;
                field.RichTextMode = RichTextMode;
            });
        }
    }
}
