using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Model;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Data.Linq;
using Telerik.Sitefinity.Model.ContentLinks;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Services.RelatedData.Messages;
using Telerik.Sitefinity.Services.RelatedData.Responses;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace SitefinityWebApp
{
    public partial class RelatedPromotions : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            relateBtn.Click += relateBtn_Click;

            var manager = UserManager.GetManager();
            var user = manager.GetUser("Test");

            var profManager = UserProfileManager.GetManager();

            SitefinityProfile profile = null;

            if (user != null)
            {
                profile = profManager.GetUserProfile<SitefinityProfile>(user);
                var relatedItems = profile.GetRelatedItems<DynamicContent>("UserFavoritePromotions").Where(i => i.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Live && i.Visible == true);
                if (relatedItems.Count() == 0)
                {
                    var serv = new CustomRelatedDataService();
                    var message = new ParentItemMessage();
                    message.ParentItemId = profile.Id.ToString();
                    message.Skip = 0;
                    message.Take = 30;
                    message.FieldName = "UserFavoritePromotions";
                    var test = serv.Get(message) as RelatedItemsResponse;
                    relatedPromotionsRptr.DataSource = test.Items;
                    
                }
                else
                {
                    relatedPromotionsRptr.DataSource = relatedItems;
                }
                relatedPromotionsRptr.DataBind();
            }
        }


        void relateBtn_Click(object sender, EventArgs e)
        {
            var manager = UserManager.GetManager();
            var user = manager.GetUser("Test");

            var profManager = UserProfileManager.GetManager();

            SitefinityProfile profile = null;

            if (user != null)
            {
                profile = profManager.GetUserProfile<SitefinityProfile>(user);

                var providerName = String.Empty;
                DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(providerName);
                Type promotionType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.Promotion.Promotion");

                var myCollection = dynamicModuleManager.GetDataItems(promotionType).Where(i => i.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master);

                foreach (var item in myCollection)
                {
                    profile.CreateRelation(item, "UserFavoritePromotions");
                    profManager.SaveChanges();
                }
            }
        }
    }
}