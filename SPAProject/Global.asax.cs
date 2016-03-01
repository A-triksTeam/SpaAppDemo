using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Telerik.Microsoft.Practices.Unity;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Abstractions.VirtualPath;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.ModuleEditor.Web.UI;
using Telerik.Sitefinity.Modules.Forms.Events;
using Telerik.Sitefinity.Mvc.Rendering;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Web.UI;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Data.Linq.Dynamic;
using Telerik.Sitefinity.GenericContent.Model;
using System.Text.RegularExpressions;
using Telerik.Sitefinity.Workflow;


namespace SitefinityWebApp
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            Bootstrapper.Initialized += Bootstrapper_Initialized; 
        }

        private void Bootstrapper_Initialized(object sender, Telerik.Sitefinity.Data.ExecutedEventArgs e)
        {
            if (e.CommandName == "Bootstrapped")
            {
                ObjectFactory.Container.RegisterType(typeof(DialogBase), typeof(CustomFieldWizardDialog), typeof(FieldWizardDialog).Name, new HttpRequestLifetimeManager());
                SystemManager.RegisterServiceStackPlugin(new RelatedDataServiceCustomPlugin(), true);
                EventHub.Subscribe<IFormEntryCreatedEvent>(evt => FormsEventHandler(evt));
            }

            if (e.CommandName == "RegisterRoutes")
            {
                RegisterRoutes(RouteTable.Routes);
            }
        }

        private void FormsEventHandler(IFormEntryCreatedEvent evt)
        {
            var entryControls = evt.Controls;
            var title = entryControls.Where(c => c.Title == "Promotion title").FirstOrDefault().Value.ToString();
            var content = entryControls.Where(c => c.Title == "Promotion text").FirstOrDefault().Value.ToString();
            var authorId = evt.UserId;
            var formId = evt.FormId;
            CreatePromotion(title, content, authorId);
        }

        public void CreatePromotion(string title, string content, Guid authorId, object image = null)
        {
            // Set the provider name for the DynamicModuleManager here. All available providers are listed in
            // Administration -> Settings -> Advanced -> DynamicModules -> Providers
            var providerName = String.Empty;

            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(providerName);
            Type promotionType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.Promotion.Promotion");
            DynamicContent promotionItem = dynamicModuleManager.CreateDataItem(promotionType);

            // This is how values for the properties are set
            promotionItem.SetValue("Title", title);
            promotionItem.SetValue("Description", content);

            promotionItem.SetString("UrlName", Regex.Replace(title, @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-"));
            promotionItem.SetValue("Owner", authorId);

            // FIRST Check-out the Item
            dynamicModuleManager.Lifecycle.CheckOut(promotionItem);

            // THEN Save the changes
            dynamicModuleManager.SaveChanges();

            // Send the Item for approval
            var bag = new Dictionary<string, string>();
            bag.Add("ContentType", promotionItem.GetType().FullName);
            WorkflowManager.MessageWorkflow(promotionItem.Id, promotionItem.GetType(),
            dynamicModuleManager.Provider.ToString(), "SendForApproval", true, bag);
        }


        private void RegisterRoutes(RouteCollection routeCollection)
        {
            routeCollection.Ignore("{resource}.axd/{*pathInfo}");
            routeCollection.Ignore("js/{*pathInfo}");
            routeCollection.Ignore("markup/{*pathInfo}");
            routeCollection.Ignore("sitefinity/{*pathInfo}");
            routeCollection.Ignore("restapi/{*pathInfo}");

            System.Web.Mvc.RouteCollectionExtensions.MapRoute(System.Web.Routing.RouteTable.Routes,

                 "Classic",
                 "{controller}/{action}/{id}",
                 new { controller = "HomeController", action = "Index" }
                 );
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}