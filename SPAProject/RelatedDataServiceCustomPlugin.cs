using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telerik.Sitefinity.Services.RelatedData.Messages;

namespace SitefinityWebApp
{
    public class RelatedDataServiceCustomPlugin : IPlugin
    {
        public void Register(IAppHost appHost)
        {
            appHost.RegisterService(typeof(CustomRelatedDataService));
            appHost.Routes
                //.Add<ChildItemMessage>(String.Concat(RelatedDataServiceRoute, "/", "parent-items"), "GET")
                .Add<ParentItemMessage>(String.Concat(RelatedDataServiceRoute, "/", "child-items"), "GET")
                //.Add<DataTypeMessage>(String.Concat(RelatedDataServiceRoute, "/", "data-types"), "GET")
                .Add<RelationChangeMessage>(String.Concat(RelatedDataServiceRoute, "/", "relations"), "PUT");
        }

        private const string RelatedDataServiceRoute = "/sitefinity/related-data";   
    }
}