using SitefinityWebApp.Mvc.Models.DashboardModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Telerik.Sitefinity.Mvc;
using Telerik.Sitefinity.Mvc.ActionFilters;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Claims;
using Telerik.Sitefinity.Security.Model;
using Telerik.Sitefinity.Model;
using System.Text;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Modules.Pages.Configuration;
using Telerik.Sitefinity.Modules.GenericContent.Web.UI;

namespace SitefinityWebApp.Mvc.Controllers
{

    [ControllerToolboxItem(Name = "Dashboard", Title = "Dashboard", SectionName = "Custom")]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            var config = "UserDashboardConfig";
            var identity = ClaimsManager.GetCurrentIdentity();
            var userId = identity.UserId;
            var dashboardConfigString = string.Empty;

            if (userId != null && userId != Guid.Empty)
            {
                var userProfile = this.GetUserProfileByUserId(userId);
                dashboardConfigString = userProfile.GetValue<string>(config);
            }

            var model = new DashboardModel();

            //init all the controls available
            List<DashboardDummyControl> types = new List<DashboardDummyControl> 
                { 
                    new DashboardDummyControl { IsWidgetDropped = false, WidgetName = "Cool widget", WidgetType="My.Cool.Widget", WidgetThumbnail="http://localhost:60876/images/default-source/default-album/htismpc.tmb-custom.jpg", PartialLocation="/GetResult" },
                    new DashboardDummyControl { IsWidgetDropped = false, WidgetName = "Wicked widget", WidgetType="My.Wicked.Widget", WidgetThumbnail="http://localhost:60876/images/default-source/default-album/427442.tmb-custom.jpg", PartialLocation="/AnotherResult" },
                    new DashboardDummyControl { IsWidgetDropped = false, WidgetName = "Awesome widget", WidgetType="My.Awesome.Widget", WidgetThumbnail="http://localhost:60876/images/default-source/default-album/radiobuttonsvslist.tmb-custom.png", PartialLocation="/ThirdResult" }
                };
            
            if (!string.IsNullOrEmpty(dashboardConfigString))
            {
                var userDashboardControls = this.DeserializeDashboardString(dashboardConfigString).OrderBy(c => c.Position).ToList();
                model.dashboardControlsCollection = userDashboardControls;

                //doing this to disable the side panel widgets in case they are dropped on the dashboard
                foreach (var control in types)
                {
                    if (userDashboardControls.Any(c => c.PartialLocation == control.PartialLocation))
                    {
                        control.IsWidgetDropped = true;
                    }
                }
            }
            else
            {
                model.dashboardControlsCollection = new List<DashboardDummyControl>();
            }

            model.controlsCollection = types;
            return View(model);
        }

        private string SerializeControlsToString(List<DashboardDummyControl> controls)
        {
            StringBuilder dashboardSb = new StringBuilder();
            var propertyDelimiter = "*";
            var controlDelimiter = "|";

            foreach (var control in controls)
            {
                dashboardSb.Append(control.WidgetType);
                dashboardSb.Append(propertyDelimiter);
                dashboardSb.Append(control.WidgetThumbnail);
                dashboardSb.Append(propertyDelimiter);
                dashboardSb.Append(control.Position);
                dashboardSb.Append(propertyDelimiter);
                dashboardSb.Append(control.PartialLocation);
                dashboardSb.Append(controlDelimiter);
            }

            string dasboardString = dashboardSb.ToString();
            if (!string.IsNullOrEmpty(dasboardString))
            {
                dasboardString = dasboardString.Remove(dasboardString.Length - 1);
            }

            return dasboardString;
        }

        private List<DashboardDummyControl> DeserializeDashboardString(string dashboardString)
        {
            List<DashboardDummyControl> controls = new List<DashboardDummyControl>();
            var propertyDelimiter = "*";
            var controlDelimiter = "|";

            string[] stringControls = dashboardString.Split(new string[] { controlDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var stringControl in stringControls)
            {
                string[] controlProps = stringControl.Split(new string[] {propertyDelimiter}, StringSplitOptions.RemoveEmptyEntries);
                var control = new DashboardDummyControl()
                {
                    WidgetType = controlProps[0],
                    WidgetThumbnail = controlProps[1],
                    Position = int.Parse(controlProps[2]),
                    PartialLocation = controlProps[3]
                };

                controls.Add(control);
            }

            return controls;
        }

        private SitefinityProfile GetUserProfileByUserId(Guid userId)
        {
            UserProfileManager profileManager = UserProfileManager.GetManager();
            UserManager userManager = UserManager.GetManager();

            User user = userManager.GetUser(userId);

            SitefinityProfile profile = null;

            if (user != null)
            {
                profile = profileManager.GetUserProfile<SitefinityProfile>(user);
            }

            return profile;
        }

        [HttpGet]
        [StandaloneResponseFilter]
        public PartialViewResult GetResult()
        {
            var contentBlock = new ContentBlock();
            contentBlock.Html = "<p>wtf is this man</p>";
            ViewBag.data = "Some manipulated data";
            return PartialView("_myPartial");
        }

        [HttpGet]
        [StandaloneResponseFilter]
        public PartialViewResult AnotherResult()
        {
            ViewBag.data = "Other manipulated data";
            return PartialView("_anotherPartial");
        }

        [HttpGet]
        [StandaloneResponseFilter]
        public PartialViewResult ThirdResult()
        {
            ViewBag.data = "Third manipulated data";
            return PartialView("_thirdPartial");
        }

        [HttpPost]
        [StandaloneResponseFilter]
        public PartialViewResult Submit(DashboardDummyControl[] controls)
        {
            var config = "UserDashboardConfig";
            var dashboardString = string.Empty;
            var manager = UserProfileManager.GetManager();
            var identity = ClaimsManager.GetCurrentIdentity();
            var userId = identity.UserId;

            if (userId != null && userId != Guid.Empty)
            {
                var userProfile = this.GetUserProfileByUserId(userId);

                if (controls != null && controls.Length > 0)
                {
                    dashboardString = this.SerializeControlsToString(controls.ToList());
                }

                manager.Provider.SuppressSecurityChecks = true;
                userProfile.SetValue(config, dashboardString);
                manager.SaveChanges();
                manager.Provider.SuppressSecurityChecks = false;

                return PartialView("_submit");
            }
            else
            {
                return PartialView("_unauthenticated");
            }
        }
    }
}