using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SysProcessModel
{
    public class SysArea
    {
        public int ID { get; set; }

        public string Name { get; set; }

        //public ObservableCollection<SysProvince> Provinces { get; private set; }

        //public SysArea()
        //{
        //    Provinces = new ObservableCollection<SysProvince>();
        //}
    }

    public class SysProvience
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int AreaId { get; set; }

        //public ObservableCollection<SysCity> Cities { get; private set; }

        //public SysProvince()
        //{
        //    Cities = new ObservableCollection<SysCity>();
        //}
    }

    public class SysCity
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int ProvienceId { get; set; }
    }
}
