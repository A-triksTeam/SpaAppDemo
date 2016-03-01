using LitJson;
using SitefinityWebApp.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitefinityWebApp.Mvc.Models.DashboardModels
{
    public class DashboardModel
    {
        public IList<DashboardDummyControl> controlsCollection { get; set; }
        public IList<DashboardDummyControl> dashboardControlsCollection { get; set; }
    }
}