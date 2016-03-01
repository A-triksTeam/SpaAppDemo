using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;

namespace SitefinityWebApp.Mvc.Controllers
{
    public class DashboardDummyControl
    {
        public string WidgetName { get; set; }
        public string WidgetType { get; set; }
        public bool IsWidgetDropped { get; set; }
        public string WidgetThumbnail { get; set; }
        public string PartialLocation { get; set; }
        public int Position { get; set; }
    }
}