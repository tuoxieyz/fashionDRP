using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Telerik.Windows.Controls.Data.DataFilter;

namespace View.Extension
{
    public class ExtEditorTemplateSelector : DataTemplateSelector
    {
        List<EditorTemplateRule> _editorTemplateRules;

        /// <summary>
        /// When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate"/> based on custom logic.
        /// </summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>
        /// Returns a <see cref="T:System.Windows.DataTemplate"/> or null. The default value is null.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ItemPropertyDefinition propertyDefinition = (ItemPropertyDefinition)item;

            foreach (EditorTemplateRule rule in this.EditorTemplateRules)
            {
                // Select the appropriate template for each property.
                if (rule.PropertyName == propertyDefinition.PropertyName)
                {
                    return rule.DataTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }

        /// <summary>
        /// Gets the rules.
        /// </summary>
        /// <value>The rules.</value>
        public List<EditorTemplateRule> EditorTemplateRules
        {
            get
            {
                if (this._editorTemplateRules == null)
                {
                    this._editorTemplateRules = new List<EditorTemplateRule>();
                }

                return this._editorTemplateRules;
            }
        }
    }

}
