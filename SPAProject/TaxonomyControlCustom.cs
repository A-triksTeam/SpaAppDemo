using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Telerik.Sitefinity.Taxonomies.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Web.UI.PublicControls;
using Telerik.Web.UI;

namespace SitefinityWebApp
{
    public class TaxonomyControlCustom : TaxonomyControl
    {
        private string layoutTemplatePath = "~/TaxonomyControlCustomTemplate.ascx";  

        public override string LayoutTemplatePath
        {
            get
            {
                return this.layoutTemplatePath;
            }
            set
            {
                this.layoutTemplatePath = value;
            }
        }
        protected virtual RadTreeView TreeViewHierarchicalList
        {
            get
            {
                return this.Container.GetControl<RadTreeView>("treeview_hierarchicallist", false);
            }
        }

        // We need to override this so that the Repeater control is not required on the template
        protected override Repeater TaxaRepeater
        {
            get
            {
                string lower = this.RenderAs.ToString().ToLower();
                string str = string.Concat("repeater_", lower);
                return this.Container.GetControl<Repeater>(str, false); // We do this by passing false on the GetControl method
            }
        }

        protected override void InitializeTaxaList()
        {
            if (this.RenderAs == RenderTaxonomyAs.HierarchicalList)
            {
                IDictionary<ITaxon, uint> taxaItemsCountForTaxonomy = null;
                if (this.ContentId == Guid.Empty)
                {
                    taxaItemsCountForTaxonomy = this.GetTaxaItemsCountForTaxonomy();
                }
                else
                {
                    taxaItemsCountForTaxonomy = this.GetTaxaFromContentItem();
                }

                int taxonDataCount = 0;
                List<CustomTaxonData> taxonDatas = this.CustomPrepareData(taxaItemsCountForTaxonomy);
                taxonDataCount = taxonDatas.Count;

                // After obtaining the data we configure and bind the tree view.
                this.ConfigureTreeViewHierarchicalListControl(taxonDatas);                

                this.IsEmpty = taxonDataCount == 0;
                if (!this.IsEmpty)
                {
                    this.TitleLabel.Text = this.Title;
                }
            }
            else
            {
                base.InitializeTaxaList();
            }
        }

        private void ConfigureTreeViewHierarchicalListControl(List<CustomTaxonData> taxonDatas)
        {
            this.TreeViewHierarchicalList.Skin = "BlackMetroTouch";
            this.TreeViewHierarchicalList.ExpandAnimation.Type = AnimationType.InQuart;
            this.TreeViewHierarchicalList.ExpandAnimation.Duration = 300;
            this.TreeViewHierarchicalList.CollapseAnimation.Type = AnimationType.InQuart;
            this.TreeViewHierarchicalList.CollapseAnimation.Duration = 200;
            this.TreeViewHierarchicalList.DataSource = taxonDatas;
            this.TreeViewHierarchicalList.DataFieldID = "TaxonId";
            this.TreeViewHierarchicalList.DataFieldParentID = "ParentTaxonId";
            this.TreeViewHierarchicalList.DataNavigateUrlField = "Url";
            this.TreeViewHierarchicalList.DataTextField = "Title";
            this.TreeViewHierarchicalList.DataBind();
        }

        // We can't help but decompile the PrepareData method in order to use it and populate the additional TaxonId and ParentTaxonId properties.
        // This method is invoked only when the tree view is being bound.
        private List<CustomTaxonData> CustomPrepareData(IDictionary<ITaxon, uint> taxaCount)
        {
            double num;
            if (taxaCount.Count == 0)
            {
                return new List<CustomTaxonData>();
            }
            List<double> list = (
                from pair in taxaCount
                select pair.Value into t
                select (double)((float)t)).ToList<double>();
            double num1 = this.StandardDeviation(list, out num);
            List<CustomTaxonData> taxonDatas = new List<CustomTaxonData>();
            foreach (KeyValuePair<ITaxon, uint> keyValuePair in taxaCount)
            {
                int size = this.GetSize((double)((float)keyValuePair.Value), num, num1);
                string str = (keyValuePair.Key is HierarchicalTaxon ? (keyValuePair.Key as HierarchicalTaxon).FullUrl : keyValuePair.Key.UrlName.Value);

                Guid? dummyNull = null; // We need this so we can set null to the nullable ParentTaxonId (in case of a top level taxon).
                CustomTaxonData taxonDatum = new CustomTaxonData()
                {
                    TaxonId = keyValuePair.Key.Id,
                    ParentTaxonId = (keyValuePair.Key as HierarchicalTaxon).Parent != null ? (keyValuePair.Key as HierarchicalTaxon).Parent.Id : dummyNull,
                    Title = keyValuePair.Key.Title,
                    Count = keyValuePair.Value,
                    Size = size,
                    Url = this.BuildUrl(str)
                };
                taxonDatas.Add(taxonDatum);
            }
            if (!this.ShowItemCount)
            {
                taxonDatas.Sort((CustomTaxonData a, CustomTaxonData b) => a.Title.CompareTo(b.Title));
            }

            return taxonDatas;
        }

        public new TaxonomyControlCustom.RenderTaxonomyAs RenderAs
        {
            get;
            set;
        }

        public new enum RenderTaxonomyAs
        {
            HorizontalList = 0,
            VerticalList = 1,
            Cloud = 2,
            HierarchicalList = 3
        }

        // We need a CustomTaxonData class in order to add 2 more properties - TaxonId and ParentTaxonId.
        protected internal class CustomTaxonData
        {
            public Guid TaxonId { get; set; }
            public Guid? ParentTaxonId { get; set; }

            public uint Count { get; set;}
            public int Size { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }

            public CustomTaxonData()
            {
            }
        }
    }
}