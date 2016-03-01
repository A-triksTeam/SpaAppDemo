using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.ModuleEditor.Configuration;
using Telerik.Sitefinity.ModuleEditor.Web.UI;
using Telerik.Sitefinity.Security.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Web.UI.Fields;

namespace SitefinityWebApp
{
    public class CustomFieldWizardDialog : FieldWizardDialog  
    {
        private ICollection<FieldTypeElement> customFieldTypes;

        private ICollection<FieldTypeElement> CustomFieldTypes
        {
            get
            {
                if (this.customFieldTypes == null)
                {
                    this.customFieldTypes = Config.Get<CustomFieldsConfig>().FieldTypes.Values;
                }
                return this.customFieldTypes;
            }
        }
         
        protected override void InitializeControls(Telerik.Sitefinity.Web.UI.GenericContainer container)
        {
            base.InitializeControls(container);
            this.BindFieldTypesCustom();
        }

        private void BindFieldTypesCustom() 
        {
            this.FieldTypes.Choices.Clear(); 
            List<FieldTypeElement> list = this.CustomFieldTypes.ToList<FieldTypeElement>(); 
            string item = HttpContext.Current.Request.Params["componentType"];
            Type type = TypeResolutionService.ResolveType(item, false);
            if (type != null && typeof(UserProfile).IsAssignableFrom(type))
            {
                list = this.CustomFieldTypes.Where<FieldTypeElement>((FieldTypeElement cf) => 
                {
                    if (cf.Title == "RelatedData")
                    {
                        return false;
                    }
                    return cf.Title != "RelatedMedia";
                }).ToList<FieldTypeElement>();
            }
            foreach (FieldTypeElement fieldTypeElement in list)
            {
                if (!this.AllowContentLinks && fieldTypeElement.Name == "Image")
                {
                    continue;
                }
                string title = fieldTypeElement.Title;
                if (!string.IsNullOrEmpty(fieldTypeElement.ResourceClassId))
                {
                    title = Res.Get(fieldTypeElement.ResourceClassId, title);
                }
                Collection<ChoiceItem> choices = this.FieldTypes.Choices;
                ChoiceItem choiceItem = new ChoiceItem()
                {
                    Text = title,
                    Value = fieldTypeElement.Name
                };
                choices.Add(choiceItem);
            }
        } 
    }
}