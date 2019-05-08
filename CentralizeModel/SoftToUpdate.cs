using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;

namespace CentralizeModel
{
    public class SoftToUpdate
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID
        {
            get;
            set;
        }

        public string IdentificationKey { get; set; }

        private string _softName;
        public string SoftName
        {
            get
            {
                return this._softName;
            }
            set
            {
                this._softName = value;
            }
        }

        public string DownloadUrl { get; set; }

        public string UpdateUrl { get; set; }

        private string _description;
        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }
    }
}
