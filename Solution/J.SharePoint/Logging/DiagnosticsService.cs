using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint.Logging
{
    public class DiagnosticsService : SPDiagnosticsServiceBase
    {
        private const string NameFormat = "{0}_{1}";

        [Persisted]
        private string LogCategory;

        [Persisted]
        private string ProductName;

        [Persisted]
        private TraceSeverity DefaultTraceLevel;

        [Persisted]
        private EventSeverity DefaultEventLevel;

        public DiagnosticsService()
            : base("", SPFarm.Local)
        { }

        protected DiagnosticsService(string productName, string logCategory, TraceSeverity defaultTraceLevel, EventSeverity defaultEventLevel) :
            base(string.Format(NameFormat, productName, logCategory), SPFarm.Local)
        {
            ProductName = productName;
            LogCategory = logCategory;
            DefaultTraceLevel = defaultTraceLevel;
            DefaultEventLevel = defaultEventLevel;
        }

        protected DiagnosticsService(string productName, string logCategory) : 
            this(productName, logCategory, TraceSeverity.Medium, EventSeverity.Error)
        { }

        protected override IEnumerable<SPDiagnosticsArea> ProvideAreas()
        {
            var areas = new List<SPDiagnosticsArea>
                {
                    new SPDiagnosticsArea(ProductName, new List<SPDiagnosticsCategory>
                        {
                            new SPDiagnosticsCategory(LogCategory, DefaultTraceLevel, DefaultEventLevel)
                        })
                };
            return areas;
        }

        protected void LogEvent(EventSeverity level, string source, string message, object[] args)
        {
            SPDiagnosticsCategory diagCategory = Areas[ProductName].Categories[LogCategory];
            WriteEvent(0, diagCategory, level, string.Format("{0}::{1}", source, message), args);
        }

        protected void LogTrace(TraceSeverity level, string source, string message, object[] args)
        {
            SPDiagnosticsCategory diagCategory = Areas[ProductName].Categories[LogCategory];
            WriteTrace(0, diagCategory, level, string.Format("{0}::{1}", source, message), args);
        }

        public void LogInfo(string source, string message, params object[] args)
        {
            //LogEvent(EventSeverity.Information, source, message, args);
            LogTrace(TraceSeverity.Verbose, source, message, args);
        }

        public void LogWarning(string source, string message, params object[] args)
        {
            //LogEvent(EventSeverity.Warning, source, message, args);
            LogTrace(TraceSeverity.High, source, message, args);
        }

        public void LogError(string source, string message, params object[] args)
        {
            //LogEvent(EventSeverity.Error, source, message, args);
            LogTrace(TraceSeverity.Unexpected, source, message, args);
        }

        public static DiagnosticsService Create(string productName, string logCategory)
        {
            return SPFarm.Local.Services.GetValue<DiagnosticsService>(string.Format(NameFormat, productName, logCategory)) ??
                new DiagnosticsService(productName, logCategory);
        }

        public static DiagnosticsService Create(string productName, string logCategory, TraceSeverity defaultTraceLevel, EventSeverity defaultEventLevel)
        {
            return SPFarm.Local.Services.GetValue<DiagnosticsService>(string.Format(NameFormat, productName, logCategory)) ??
                new DiagnosticsService(productName, logCategory, defaultTraceLevel, defaultEventLevel);
        }
    }
}
