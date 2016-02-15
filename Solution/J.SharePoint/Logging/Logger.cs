using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint.Logging
{
    public abstract class Logger
    {
        protected abstract string ProductName
        { get; }

        protected abstract string LogCategory
        { get; }

        private DiagnosticsService _current;
        public DiagnosticsService Current
        {
            get
            {
                if (_current == null)
                    _current = DiagnosticsService.Create(ProductName, LogCategory);

                return _current;
            }
        }

        public void Info(string source, string message, params object[] args)
        {
            Current.LogInfo(source, message, args);
        }

        public void Warning(string source, string message, params object[] args)
        {
            Current.LogWarning(source, message, args);
        }

        public void Error(string source, string message, params object[] args)
        {
            Current.LogError(source, message, args);
        }

        public void Provision()
        {
            _current = null;
            Current.Update();
            if (Current.Status != SPObjectStatus.Online)
                Current.Provision();

            _current = null;
        }

        public void Delete()
        {
            _current = null;
            Current.Delete();
            _current = null;
        }
    }

    public abstract class Logger<T> : Logger where T : Logger, new()
    {
        private static Logger _current;
        protected static Logger Current
        {
            get
            {
                if (_current == null)
                    _current = new T();

                return _current;
            }
        }

        public static void Info(string source, string message, params object[] args)
        {
            Current.Info(source, message, args);
        }

        public static void Warning(string source, string message, params object[] args)
        {
            Current.Warning(source, message, args);
        }

        public static void Error(string source, string message, params object[] args)
        {
            Current.Error(source, message, args);
        }

        public static void Provision()
        {
            Current.Provision();
        }

        public static void Delete()
        {
            Current.Delete();
        }
    }
}
