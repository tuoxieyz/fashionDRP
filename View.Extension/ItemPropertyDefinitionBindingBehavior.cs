using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Controls;

namespace View.Extension
{
    public class ItemPropertyDefinitionBindingBehavior
    {
        public static readonly DependencyProperty ItemPropertyDefinitionsProperty
            = DependencyProperty.RegisterAttached("ItemPropertyDefinitions", typeof(IEnumerable<ItemPropertyDefinition>), typeof(ItemPropertyDefinitionBindingBehavior),
                new PropertyMetadata(new PropertyChangedCallback(OnItemPropertyDefinitionsPropertyChanged)));

        public static void SetItemPropertyDefinitions(DependencyObject dependencyObject, IEnumerable<ItemPropertyDefinition> descriptors)
        {
            dependencyObject.SetValue(ItemPropertyDefinitionsProperty, descriptors);
        }

        public static IEnumerable<ItemPropertyDefinition> GetItemPropertyDefinitions(DependencyObject dependencyObject)
        {
            return (IEnumerable<ItemPropertyDefinition>)dependencyObject.GetValue(ItemPropertyDefinitionsProperty);
        }

        private static void OnItemPropertyDefinitionsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            RadDataFilter dataFilter = dependencyObject as RadDataFilter;
            IEnumerable<ItemPropertyDefinition> definitions = e.NewValue as IEnumerable<ItemPropertyDefinition>;

            if (dataFilter != null && definitions != null)
            {
                dataFilter.ItemPropertyDefinitions.Clear();
                dataFilter.ItemPropertyDefinitions.AddRange(definitions);
            }
        }
    }
}
