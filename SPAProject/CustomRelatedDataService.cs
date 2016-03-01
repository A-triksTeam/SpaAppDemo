using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Telerik.Microsoft.Practices.Unity;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.ContentLinks;
using Telerik.Sitefinity.Data.Linq;
using Telerik.Sitefinity.Descriptors;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Builder;
using Telerik.Sitefinity.DynamicModules.Builder.Model;
using Telerik.Sitefinity.DynamicModules.Data;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Metadata.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Model.ContentLinks;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Model;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Services.RelatedData;
using Telerik.Sitefinity.Services.RelatedData.Messages;  
using Telerik.Sitefinity.Services.RelatedData.ResponseBuilders;
using Telerik.Sitefinity.Services.RelatedData.Responses;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Web.UI.Fields.Enums;

namespace SitefinityWebApp
{
    public class CustomRelatedDataService : RelatedDataService
    {
        #region Main service methods called from outside
        
        new public object Put(RelationChangeMessage message)
        {
            if (message.ItemType == null && message.ItemProvider == null)
            {
                message.ItemType = "Telerik.Sitefinity.Security.Model.SitefinityProfile";
                message.ItemProvider = "OpenAccessProfileProvider";
                Type profileType = TypeResolutionService.ResolveType(message.ItemType);

                UserProfileManager mappedManager = ManagerBase.GetMappedManager(profileType, message.ItemProvider) as UserProfileManager;
                if (mappedManager != null)
                {
                    Guid guid = new Guid(message.ItemId);
                    object itemOrDefault = mappedManager.GetItem(profileType, guid);
                    SitefinityProfile temp = itemOrDefault as SitefinityProfile;

                    if (temp != null)
                    {
                        this.SaveRelatedDataChanges(mappedManager, temp, message.RelationChanges, false);
                        mappedManager.SaveChanges();
                    }
                }
            }
            else
            {
                base.Put(message);
            }

            return null;
        }

        new public object Get(ParentItemMessage message)
        {
            if (message.ParentItemType == null && message.ParentProviderName == null)
            {     
                message.ParentItemType = "Telerik.Sitefinity.Security.Model.SitefinityProfile";
                message.ParentProviderName = "OpenAccessProfileProvider";

                Guid guid = new Guid(message.ParentItemId);
                int? nullable = new int?(0);
                IQueryable relatedItems = this.GetRelatedItems(message.ParentItemType, message.ParentProviderName, guid, message.FieldName, message.Status, message.Filter, message.Order, new int?(message.Skip), new int?(message.Take), ref nullable, message.ChildItemType, message.ChildProviderName);
                List<IDataItem> list = relatedItems.OfType<IDataItem>().ToList<IDataItem>();
                IEnumerable<Guid> id =
                    from i in list
                    select i.Id;
                IQueryable<ContentLink> contentLinks =
                    from cl in ContentLinksManager.GetManager().GetContentLinks()
                    where (cl.ParentItemType == message.ParentItemType) && (cl.ParentItemId == guid) && (cl.ParentItemProviderName == message.ParentProviderName) && (cl.ComponentPropertyName == message.FieldName) && id.Contains<Guid>(cl.ChildItemId)
                    select cl;
                IEnumerable<RelatedItemResponse> relatedItemsResponse = this.GetRelatedItemsResponse(list, contentLinks.ToList<ContentLink>());
                RelatedItemsResponse relatedItemsResponse1 = new RelatedItemsResponse()
                {
                    Items = relatedItemsResponse,
                    TotalCount = nullable.Value
                };
                return relatedItemsResponse1;
            }
            else
            {
                return base.Get(message);
            }
        }
        
        #endregion

        #region Private methods extracted from SF source code and modified to suite the SitefinityProfile object

        private IQueryable GetRelatedItems(string parentItemTypeName, string parentItemProviderName, Guid parentItemId, string fieldName, ContentLifecycleStatus? status, string filterExpression, string orderExpression, int? skip, int? take, ref int? totalCount, string childItemTypeName = null, string childItemProviderName = null)
        {
            IQueryable relatedItemsViaContains = null;
            if (childItemTypeName == null || childItemProviderName == null)
            {
                Type type = TypeResolutionService.ResolveType(parentItemTypeName);
                RelatedDataPropertyDescriptor item = TypeDescriptor.GetProperties(type)[fieldName] as RelatedDataPropertyDescriptor;
                if (item != null)
                {
                    MetaFieldAttributeAttribute metaFieldAttributeAttribute = item.Attributes[typeof(MetaFieldAttributeAttribute)] as MetaFieldAttributeAttribute;
                    if (metaFieldAttributeAttribute != null)
                    {
                        metaFieldAttributeAttribute.Attributes.TryGetValue("RelatedType", out childItemTypeName);
                        metaFieldAttributeAttribute.Attributes.TryGetValue("RelatedProviders", out childItemProviderName);
                    }
                }
            }

            //Most probably will not work in multisite due to this
            //if (childItemProviderName == "sf-site-default-provider")
            //{
            //    //childItemProviderName = this.ResolveProvider(childItemTypeName);
            //}

            if (childItemTypeName != null && childItemProviderName != null)
            {
                Type type1 = TypeResolutionService.ResolveType(childItemTypeName, false);
                IManager mappedManager = ManagerBase.GetMappedManager(type1, childItemProviderName);
                if (mappedManager is IRelatedDataSource)
                {
                    IRelatedDataSource relatedDataSource = mappedManager as IRelatedDataSource;
                    relatedItemsViaContains = this.GetRelatedItems(mappedManager.Provider as OpenAccessDynamicModuleProvider, parentItemTypeName, parentItemProviderName, parentItemId, fieldName, type1, status, filterExpression, orderExpression, skip, take, ref totalCount, RelationDirection.Child);
                }
            }
            return relatedItemsViaContains;
        }

        public virtual IQueryable GetRelatedItems(OpenAccessDynamicModuleProvider provider, string itemType, string itemProviderName, Guid itemId, string fieldName, Type relatedItemsType, ContentLifecycleStatus? status, string filterExpression, string orderExpression, int? skip, int? take, ref int? totalCount, RelationDirection relationDirection = 0)
        {
            if (relationDirection == RelationDirection.Child)
            {
                return this.GetRelatedItems(provider, itemType, itemProviderName, itemId, fieldName, relatedItemsType, status, filterExpression, orderExpression, skip, take, ref totalCount);
            }
            return this.GetRelatingItems(provider, itemType, itemProviderName, itemId, fieldName, relatedItemsType, status, filterExpression, orderExpression, skip, take, ref totalCount);
        }

        private IQueryable GetRelatingItems(OpenAccessDynamicModuleProvider provider, string childItemType, string childItemProviderName, Guid childItemId, string fieldName, Type itemType, ContentLifecycleStatus? status, string filterExpression, string orderExpression, int? skip, int? take, ref int? totalCount)
        {
            IQueryable<ContentLink> contentLinks =
                from cl in SitefinityQuery.Get<ContentLink>(provider)
                where (cl.ChildItemType == childItemType) && (cl.ChildItemProviderName == childItemProviderName) && (cl.ParentItemProviderName == provider.Name)
                select cl;
            if (childItemId != Guid.Empty)
            {
                contentLinks =
                    from cl in contentLinks
                    where cl.ChildItemId == childItemId
                    select cl;
            }
            //contentLinks = RelatedDataHelper.ApplyDeletedLinksFilters(contentLinks, RelationDirection.Parent);
            //contentLinks = RelatedDataHelper.ApplyLinksFilters(contentLinks, fieldName, status);
            IQueryable<DynamicContent> dynamicContents = null;
            IQueryable<DynamicContent> dataItems = provider.GetDataItems(itemType);
            //dynamicContents = RelatedDataHelper.JoinRelatingItems<DynamicContent>(dataItems, contentLinks, status);

            //filtering and ordering will not work
            //if (!string.IsNullOrEmpty(filterExpression))
            //{
            //    dynamicContents = dynamicContents.Where<DynamicContent>(filterExpression, new object[0]);
            //}
            totalCount = new int?(dynamicContents.Count<DynamicContent>());
            //if (!string.IsNullOrEmpty(orderExpression))
            //{
            //    dynamicContents = dynamicContents.OrderBy<DynamicContent>(orderExpression, new object[0]);
            //}
            if (skip.HasValue)
            {
                int? nullable = skip;
                if ((nullable.GetValueOrDefault() <= 0 ? false : nullable.HasValue))
                {
                    dynamicContents = dynamicContents.Skip<DynamicContent>(skip.Value);
                }
            }
            if (take.HasValue)
            {
                int? nullable1 = take;
                if ((nullable1.GetValueOrDefault() <= 0 ? false : nullable1.HasValue))
                {
                    dynamicContents = dynamicContents.Take<DynamicContent>(take.Value);
                }
            }
            return dynamicContents;
        }

        private IQueryable GetRelatedItems(OpenAccessDynamicModuleProvider provider, string parentItemType, string parentItemProviderName, Guid parentItemId, string fieldName, Type itemType, ContentLifecycleStatus? status, string filterExpression, string orderExpression, int? skip, int? take, ref int? totalCount)
        {
            IQueryable<ContentLink> ordinal =
                from cl in SitefinityQuery.Get<ContentLink>(provider)
                where (cl.ParentItemType == parentItemType) && (cl.ParentItemProviderName == parentItemProviderName) && (cl.ChildItemProviderName == provider.Name)
                select cl;
            if (parentItemId != Guid.Empty)
            {
                ordinal =
                    from cl in ordinal
                    where cl.ParentItemId == parentItemId
                    select cl;
            }

            //ordinal = RelatedDataHelper.ApplyDeletedLinksFilters(ordinal, RelationDirection.Child);
            //ordinal = RelatedDataHelper.ApplyLinksFilters(ordinal, fieldName, status);
            ordinal =
                from cl in ordinal
                orderby cl.Ordinal
                select cl;
            IQueryable<DynamicContent> dynamicContents = null;
            IQueryable<DynamicContent> dataItems = provider.GetDataItems(itemType);
            dynamicContents = this.JoinRelatedItems<DynamicContent>(dataItems, ordinal, status);

            //filtering and ordering will not work
            //if (!string.IsNullOrEmpty(filterExpression))
            //{
            //    dynamicContents = dynamicContents.Where<DynamicContent>(filterExpression, new object[0]);
            //}
            totalCount = new int?(dynamicContents.Count<DynamicContent>());
            //if (!string.IsNullOrEmpty(orderExpression))
            //{
            //    dynamicContents = dynamicContents.OrderBy<DynamicContent>(orderExpression, new object[0]);
            //}
            if (skip.HasValue)
            {
                int? nullable = skip;
                if ((nullable.GetValueOrDefault() <= 0 ? false : nullable.HasValue))
                {
                    dynamicContents = dynamicContents.Skip<DynamicContent>(skip.Value);
                }
            }
            if (take.HasValue)
            {
                int? nullable1 = take;
                if ((nullable1.GetValueOrDefault() <= 0 ? false : nullable1.HasValue))
                {
                    dynamicContents = dynamicContents.Take<DynamicContent>(take.Value);
                }
            }
            return dynamicContents;
        }

        private IQueryable<T> JoinRelatedItems<T>(IQueryable<T> items, IQueryable<ContentLink> links, ContentLifecycleStatus? status)
        where T : ILifecycleDataItemGeneric
        {
            if (status.HasValue)
            {
                ContentLifecycleStatus? nullable = status;
                if ((nullable.GetValueOrDefault() != ContentLifecycleStatus.Live ? false : nullable.HasValue))
                {
                    return
                        from i in items
                        join cl in links on ((ILifecycleDataItemGeneric)i).OriginalContentId equals cl.ChildItemId
                        select i;
                }
            }
            return
                from i in items
                join cl in links on ((IDataItem)i).Id equals cl.ChildItemId
                select i;
        }

        private IEnumerable<RelatedItemResponse> GetRelatedItemsResponse(List<IDataItem> relatedItems, List<ContentLink> contentLinks)
        {
            IEnumerable<RelatedItemResponse> response = Enumerable.Empty<RelatedItemResponse>();
            if (relatedItems != null)
            {
                IEnumerable<IResponseBuilder> responseBuilders = ObjectFactory.Container.ResolveAll(typeof(IResponseBuilder), new ResolverOverride[0]).Cast<IResponseBuilder>();
                IDataItem dataItem = relatedItems.FirstOrDefault<IDataItem>();
                if (dataItem != null)
                {
                    ILifecycleManager mappedManager = ManagerBase.GetMappedManager(dataItem.GetType()) as ILifecycleManager;
                    IResponseBuilder responseBuilder = responseBuilders.FirstOrDefault<IResponseBuilder>((IResponseBuilder rb) => rb.GetItemType().IsAssignableFrom(dataItem.GetType()));
                    if (responseBuilder != null)
                    {
                        response = responseBuilder.GetResponse(relatedItems, contentLinks, mappedManager);
                    }
                }
            }
            return response;
        }

        private void SaveRelatedDataChanges(IManager manager, IDataItem item, ContentLinkChange[] changedRelations, bool copyTempRelations = false)
        {
            if (item != null)
            {
                IManager mappedRelatedManager = ManagerBase.GetMappedManager(typeof(ContentLink), string.Empty);//manager.GetMappedRelatedManager<ContentLink>(string.Empty);
                OpenAccessContentLinksProvider provider = mappedRelatedManager.Provider as OpenAccessContentLinksProvider;
                string name = manager.Provider.Name;
                string fullName = item.GetType().FullName;
                Guid id = item.Id;
                ContentLifecycleStatus status = ContentLifecycleStatus.Live;
                if (changedRelations != null && (int)changedRelations.Length > 0)
                {
                    if (item is ILifecycleDataItemGeneric)
                    {
                        ILifecycleDataItemGeneric lifecycleDataItemGeneric = item as ILifecycleDataItemGeneric;
                        status = lifecycleDataItemGeneric.Status;
                        id = (status == ContentLifecycleStatus.Master ? lifecycleDataItemGeneric.Id : lifecycleDataItemGeneric.OriginalContentId);
                    }
                    this.SaveItemRelations(item, changedRelations, new ContentLifecycleStatus?(status), mappedRelatedManager as IContentLinksManager, name, fullName, id);
                }
                if (copyTempRelations && provider != null)
                {
                    string applicationName = provider.ApplicationName;
                    IEnumerable<ContentLink> contentLinks = (
                        from c in provider.GetDirtyItems().OfType<ContentLink>()
                        where c.ApplicationName == applicationName
                        select c).Where<ContentLink>((ContentLink c) =>
                        {
                            if (!(c.ParentItemId == id) || !(c.ParentItemType == fullName))
                            {
                                return false;
                            }
                            return c.ParentItemProviderName == name;
                        });
                    this.CopyTempRelations(provider, contentLinks);
                    IQueryable<ContentLink> contentLinks1 =
                        from c in (mappedRelatedManager as IContentLinksManager).GetContentLinks()
                        where (c.ParentItemId == id) && (c.ParentItemType == fullName) && (c.ParentItemProviderName == name)
                        select c;
                    this.CopyTempRelations(provider, contentLinks1);
                }

                mappedRelatedManager.SaveChanges();
            }
        }

        private void CopyTempRelations(OpenAccessContentLinksProvider contentLinksProvider, IEnumerable<ContentLink> contentRelations)
        {
            foreach (ContentLink contentRelation in contentRelations)
            {
                bool item = contentRelation[ContentLifecycleStatus.Temp];
                contentRelation[ContentLifecycleStatus.Master] = item;
                contentRelation[ContentLifecycleStatus.Live] = item;
                if (contentRelation.AvailableForLive || contentRelation.AvailableForMaster || contentRelation.AvailableForTemp)
                {
                    continue;
                }
                contentLinksProvider.Delete(contentRelation);
            }
        }

        private void SaveItemRelations(IDataItem item, ContentLinkChange[] changedRelations, ContentLifecycleStatus? parentItemStatus, IContentLinksManager contentLinksManager, string parentProviderName, string parentItemType, Guid parentItemId)
        {
            ContentLinkChange[] contentLinkChangeArray = changedRelations;
            for (int i = 0; i < (int)contentLinkChangeArray.Length; i++)
            {
                ContentLinkChange contentLinkChange = contentLinkChangeArray[i];
                ContentLink childItemAdditionalInfo = (
                    from c in contentLinksManager.GetContentLinks()
                    where (c.ParentItemId == parentItemId) && (c.ParentItemType == parentItemType) && (c.ParentItemProviderName == parentProviderName) && (c.ComponentPropertyName == contentLinkChange.ComponentPropertyName) && (c.ChildItemId == contentLinkChange.ChildItemId) && (c.ChildItemProviderName == contentLinkChange.ChildItemProviderName) && (c.ChildItemType == contentLinkChange.ChildItemType)
                    select c).FirstOrDefault<ContentLink>();
                if (childItemAdditionalInfo == null)
                {
                    if (contentLinkChange.State == ContentLinkChangeState.Added)
                    {
                        childItemAdditionalInfo = contentLinksManager.CreateContentLink(contentLinkChange.ComponentPropertyName, parentItemId, contentLinkChange.ChildItemId, parentProviderName, contentLinkChange.ChildItemProviderName, item.GetType().FullName, contentLinkChange.ChildItemType);
                        childItemAdditionalInfo.ChildItemAdditionalInfo = contentLinkChange.ChildItemAdditionalInfo;
                        this.SetContentLinkOrdinal(childItemAdditionalInfo, contentLinkChange, contentLinksManager, parentProviderName, parentItemType, parentItemId);
                        this.SetContentLinkStatus(item, childItemAdditionalInfo, parentItemStatus, true);
                    }
                }
                else if (contentLinkChange.State == ContentLinkChangeState.Removed)
                {
                    this.SetContentLinkStatus(item, childItemAdditionalInfo, parentItemStatus, false);
                }
                else if (contentLinkChange.State == ContentLinkChangeState.Added)
                {
                    this.SetContentLinkStatus(item, childItemAdditionalInfo, parentItemStatus, true);
                }
                else if (contentLinkChange.State == ContentLinkChangeState.Updated)
                {
                    this.SetContentLinkOrdinal(childItemAdditionalInfo, contentLinkChange, contentLinksManager, parentProviderName, parentItemType, parentItemId);
                }

                if (childItemAdditionalInfo != null)
                {
                    childItemAdditionalInfo.AvailableForMaster = false;
                    childItemAdditionalInfo.AvailableForTemp = false;
                }

                if (childItemAdditionalInfo != null && !childItemAdditionalInfo.AvailableForLive && !childItemAdditionalInfo.AvailableForMaster && !childItemAdditionalInfo.AvailableForTemp)
                {
                    contentLinksManager.Delete(childItemAdditionalInfo);
                }

                contentLinksManager.SaveChanges();
            }
        }

        private void SetContentLinkStatus(IDataItem item, ContentLink contentLink, ContentLifecycleStatus? parentItemStatus, bool value)
        {
            if (parentItemStatus.HasValue)
            {
                contentLink[parentItemStatus.Value] = value;
                return;
            }
            if (!(item is ILifecycleDataItem))
            {
                contentLink[ContentLifecycleStatus.Temp] = value;
            }
        }

        private void SetContentLinkOrdinal(ContentLink contentLink, ContentLinkChange contentLinkContext, IContentLinksManager contentLinksManager, string parentProviderName, string parentItemType, Guid parentItemId)
        {
            if (contentLinkContext.Ordinal.HasValue)
            {
                contentLink.Ordinal = contentLinkContext.Ordinal.Value;
                return;
            }
            float single = (
                from c in contentLinksManager.GetContentLinks()
                where (c.ParentItemId == parentItemId) && (c.ParentItemType == parentItemType) && (c.ParentItemProviderName == parentProviderName) && (c.ComponentPropertyName == contentLinkContext.ComponentPropertyName)
                select c.Ordinal).Max<float>();
            contentLink.Ordinal = single + 1f;
        }

        #endregion
    }
}