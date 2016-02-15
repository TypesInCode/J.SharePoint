using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint.Lists.Attributes
{
    public class SPFieldMultiChoiceMetadata : SPFieldMetadata
    {
        public string[] Choices
        { get; set; }

        public SPFieldMultiChoiceMetadata()
        {
            Type = SPFieldType.MultiChoice;
        }

        public override void AddFieldTo(SPFieldCollection fieldCollection)
        {
            AddFieldTo(fieldCollection, f =>
            {
                SPFieldMultiChoice field = (SPFieldMultiChoice)f;
                field.Choices.Clear();
                field.Choices.AddRange(Choices);
            });
        }
    }
}
