using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint
{
    public class SPCtx : IDisposable
    {
        private SPSite _site;
        private SPWeb _web;
        private SPList _list;

        private Guid _siteId;
        private Guid _webId;
        private Guid _listId;

        private string _siteUrl;
        private string _listName;

        public SPSite Site
        {
            get
            {
                if (_site != null)
                    return _site;

                if (_siteId != Guid.Empty)
                    _site = new SPSite(_siteId);
                else if (!string.IsNullOrEmpty(_siteUrl))
                    _site = new SPSite(_siteUrl);

                return _site;
            }
        }

        public SPWeb Web
        {
            get
            {
                if (_web != null)
                    return _web;

                if (_webId != Guid.Empty)
                    _web = Site.OpenWeb(_webId);
                else if (!string.IsNullOrEmpty(_siteUrl))
                    _web = Site.OpenWeb();

                return _web;
            }
        }

        public SPList List
        {
            get
            {
                if (_list != null)
                    return _list;

                if (_listId != Guid.Empty)
                    _list = Web.Lists[_listId];
                else if (!string.IsNullOrEmpty(_listName))
                    _list = Web.Lists[_listName];

                return _list;
            }
        }

        public SPCtx(Guid siteId)
        {
            _siteId = siteId;
        }

        public SPCtx(Guid siteId, Guid webId)
            : this(siteId)
        {
            _webId = webId;
        }

        public SPCtx(Guid siteId, Guid webId, Guid listId)
            : this(siteId, webId)
        {
            _listId = listId;
        }

        public SPCtx(string url)
        {
            _siteUrl = url;
        }

        public SPCtx(string url, string listName)
            : this(url)
        {
            _listName = listName;
        }

        public void Dispose()
        {
            if (_site != null)
                _site.Dispose();

            if (_web != null)
                _web.Dispose();
        }

        public static void Elevated(string url, Action<SPCtx> ctxAction)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using(SPCtx ctx = new SPCtx(url))
                {
                    ctxAction(ctx);
                }
            });
        }

        public static void Elevated(string url, string listName, Action<SPCtx> ctxAction)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using(SPCtx ctx = new SPCtx(url, listName))
                {
                    ctxAction(ctx);
                }
            });
        }
    }
}
