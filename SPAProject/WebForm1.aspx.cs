using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Fluent.Pages;

namespace SitefinityWebApp
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var test = App.WorkWith().Pages().LocatedIn(PageLocation.Frontend).ThatArePublished().Get();
            rep.DataSource = test;
            rep.DataBind();
        }
    }
}