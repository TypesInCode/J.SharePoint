using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using J.SharePoint;
using J.SharePoint.Lists;
using J.SharePoint.Lists.Attributes;
using J.SharePoint.Lists.QueryExtensions;

namespace J.SharePoint.Test
{
    [SPListMetadata(Title = "Form Submission List")]
    public class FormSubmissionList : SPTypedList<FormSubmissionItem>
    {
    }

    public class FormSubmissionItem : SPTypedListItem
    {
        [SPFieldTextMetadata(Title = "Submit Value", InternalName = "SubmitValue")]
        public string SubmitValue
        { get; set; }

        [SPFieldTextMetadata(Title = "COP ID", InternalName = "COPID")]
        public string COPID
        { get; set; }
    }

    [TestClass]
    public class UnitTest1
    {
        private static string _siteUrl = "https://dev.almond.local";

        [TestMethod]
        public void TestMethod1()
        {
            using( SPCtx ctx = new SPCtx(_siteUrl) )
            {
                FormSubmissionList list = ctx.Web.GetList<FormSubmissionList>();
                var items = list.Items.Where(i => 
                    (i.Modified.QueryLeq("<Today />", true) && i.Title == "Testing Thing") ||
                    (i.Created.QueryLt(DateTime.Now, true) && i.Title == "Future item?")).GetItems();
                var item = items[0];
                items = items.NextPage();
                item = items[0];
                int total = items.Count;
            }
        }
    }
}
