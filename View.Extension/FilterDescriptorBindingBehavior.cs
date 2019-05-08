using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;
using System.Collections.Specialized;
using System.Windows;
using System.Collections;
using Telerik.Windows.Data;

namespace View.Extension
{
    public class FilterDescriptorBindingBehavior : ViewModelBase
    {
        private readonly RadDataFilter dataFilter = null;
        private readonly INotifyCollectionChanged descriptors = null;

        public static readonly DependencyProperty FilterDescriptorsProperty
            = DependencyProperty.RegisterAttached("FilterDescriptors", typeof(INotifyCollectionChanged), typeof(FilterDescriptorBindingBehavior),
                new PropertyMetadata(new PropertyChangedCallback(OnFilterDescriptorsPropertyChanged)));

        public static void SetFilterDescriptors(DependencyObject dependencyObject, INotifyCollectionChanged descriptors)
        {
            dependencyObject.SetValue(FilterDescriptorsProperty, descriptors);
        }

        public static INotifyCollectionChanged GetFilterDescriptors(DependencyObject dependencyObject)
        {
            return (INotifyCollectionChanged)dependencyObject.GetValue(FilterDescriptorsProperty);
        }

        private static void OnFilterDescriptorsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            RadDataFilter dataFilter = dependencyObject as RadDataFilter;
            INotifyCollectionChanged descriptors = e.NewValue as INotifyCollectionChanged;

            if (dataFilter != null && descriptors != null)
            {
                FilterDescriptorBindingBehavior behavior = new FilterDescriptorBindingBehavior(dataFilter, descriptors);
                behavior.Attach();
            }
        }

        private void Attach()
        {
            if (dataFilter != null && descriptors != null)
            {
                //dataFilter.Loaded -= dataFilter_Loaded;
                //dataFilter.Loaded += dataFilter_Loaded;
                CollectionChanged(null, null);//数据绑定和事件注册
                if(descriptors is CompositeFilterDescriptorCollection)
                {
                    dataFilter.FilterDescriptors.PropertyChanged -= FilterDescriptors_PropertyChanged;
                    dataFilter.FilterDescriptors.PropertyChanged += FilterDescriptors_PropertyChanged;
                }
            }
        }

        void FilterDescriptors_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LogicalOperator")
            {
                ((CompositeFilterDescriptorCollection)descriptors).LogicalOperator = ((CompositeFilterDescriptorCollection)sender).LogicalOperator;
            }
        }

        //void dataFilter_Loaded(object sender, RoutedEventArgs e)
        //{
        //    Transfer(GetFilterDescriptors(dataFilter) as IList, dataFilter.FilterDescriptors);

        //    descriptors.CollectionChanged -= CollectionChanged;
        //    descriptors.CollectionChanged += CollectionChanged;
        //}

        public FilterDescriptorBindingBehavior(RadDataFilter dataFilter, INotifyCollectionChanged descriptors)
        {
            this.dataFilter = dataFilter;
            this.descriptors = descriptors;
        }

        void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UnsubscribeFromEvents();

            Transfer(GetFilterDescriptors(dataFilter) as IList, dataFilter.FilterDescriptors);

            SubscribeToEvents();
        }

        void DataFilterFilterDescriptors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UnsubscribeFromEvents();

            Transfer(dataFilter.FilterDescriptors, GetFilterDescriptors(dataFilter) as IList);

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            dataFilter.FilterDescriptors.CollectionChanged += DataFilterFilterDescriptors_CollectionChanged;

            //if (GetFilterDescriptors(dataFilter) != null)
            //{
            //    GetFilterDescriptors(dataFilter).CollectionChanged += CollectionChanged;
            //}
        }

        private void UnsubscribeFromEvents()
        {
            dataFilter.FilterDescriptors.CollectionChanged -= DataFilterFilterDescriptors_CollectionChanged;

            //由于本系统中源集合一般不会主动增删项，因此基本不关CollectionChanged事件啥事
            //if (GetFilterDescriptors(dataFilter) != null)
            //{
            //    GetFilterDescriptors(dataFilter).CollectionChanged -= CollectionChanged;
            //}
        }

        public static void Transfer(IList source, IList target)
        {
            if (source == null || target == null)
                return;

            target.Clear();

            foreach (object o in source)
            {
                target.Add(o);
            }
        }
    }
}
