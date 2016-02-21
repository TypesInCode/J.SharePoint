using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using J.SharePoint;
using J.SharePoint.Lists;
using J.SharePoint.Lists.Attributes;
using J.SharePoint.Lists.QueryExtensions;

namespace J.SharePoint.Test
{
    [SPListMetadata(Title="Form Submission List", ListTemplateType=SPListTemplateType.GenericList, ContentTypesEnabled=true)]
    public class FormSubmissionList : SPTypedList<FormSubmissionItem>
    {
        public FormSubmissionList(bool throwFieldErrors = false) : base(throwFieldErrors)
        {}
    }

    [SPContentTypeMetadata(Name="TestItem", ParentContentType="Item")]
    public class FormSubmissionItem : SPTypedListItem
    {
        [SPFieldTextMetadata(Title = "Submit Value", InternalName = "SubmitValue", ContentType="TestItem")]
        public string SubmitValue
        { get; set; }

        [SPFieldTextMetadata(Title = "COP ID", InternalName = "COPID", ContentType="TestItem")]
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
                ctx.Web.ContentTypes.EnsureContentType(SPContentTypeMetadata.Get(typeof(FormSubmissionItem)), ctx.Web);
                ctx.Web.Fields.EnsureFields(SPFieldMetadata.GetMetadata(typeof(FormSubmissionItem)));
                ctx.Web.ContentTypes.EnsureFieldLinks(SPFieldMetadata.GetMetadata(typeof(FormSubmissionItem)), ctx.Web.Fields);
                //FormSubmissionList list = ctx.Web.GetList<FormSubmissionList>(true);
                FormSubmissionList list = new FormSubmissionList(true);
                list.LoadList(ctx.Web, true);
                list.EnsureList();

                /* var item = list.AddItem();
                Console.WriteLine(item.ContentType);
                
                
                item.SubmitValue = "whatever";
                item.Update();
                Console.WriteLine(item.ContentType); */
            }
        }
    }
}
