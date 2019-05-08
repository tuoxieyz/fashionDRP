using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace View.Extension
{
    public class EditorTemplateRule
    {
        private string _propertyName;
        private DataTemplate _dataTemplate;

        public string PropertyName
        {
            get
            {
                return this._propertyName;
            }
            set
            {
                this._propertyName = value;
            }
        }

        public DataTemplate DataTemplate
        {
            get
            {
                return this._dataTemplate;
            }
            set
            {
                this._dataTemplate = value;
            }
        }
    }

}
