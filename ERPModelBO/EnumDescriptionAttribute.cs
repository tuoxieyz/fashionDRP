using System;

namespace ERPModelBO
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class EnumDescriptionAttribute : Attribute
    {
        private string _description;

        public string Description
        {
            get { return this._description; }
        }

        public EnumDescriptionAttribute(string description)
            : base()
        {
            this._description = description;
        }
    }
}
