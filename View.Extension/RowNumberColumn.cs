using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace View.Extension
{
    public class RowNumberColumn : Telerik.Windows.Controls.GridViewColumn
    {
        public override FrameworkElement CreateCellElement(Telerik.Windows.Controls.GridView.GridViewCell cell, object dataItem)
        {
            TextBlock textBlock = cell.Content as TextBlock;

            if (textBlock == null)
            {
                textBlock = new TextBlock();
            }

            textBlock.Text = string.Format("{0}", this.DataControl.Items.IndexOf(dataItem) + 1);

            return textBlock;
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args.PropertyName == "DataControl")
            {
                if (this.DataControl != null && this.DataControl.Items != null)
                {
                    this.DataControl.Items.CollectionChanged += (s, e) =>
                    {
                        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                        {
                            this.Refresh();
                        }
                    };
                }
            }
        }
    }
}
