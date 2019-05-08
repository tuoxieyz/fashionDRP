using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Telerik.Windows.Controls;
using SysProcessModel;
using ERPViewModelBasic;
using DBAccess;
using System.Data;
using Kernel;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class AutoAllocateVM : ViewModelBase
    {
        LinqOPEncap _linqOP = VMGlobal.DistributionQuery.LinqOP;

        public int BrandID { get; set; }

        public int StorageID { get; set; }

        private IEnumerable<ProStyle> _styles;
        public IEnumerable<ProStyle> Styles
        {
            get { return _styles; }
            set
            {
                _styles = value;
                if (_styles != null && _styles.Count() > 0)
                {
                    var style = _styles.ElementAt(0);
                    BrandID = VMGlobal.SysProcessQuery.LinqOP.GetById<ProBYQ>(style.BYQID).BrandID;//由于一次分货只能对应一个品牌，因此第一款对应的品牌就是分货品牌
                }
                else
                {
                    BrandID = default(int);
                }
                //OnPropertyChanged("StylesInfo");
            }
        }

        //public string StylesInfo
        //{
        //    get
        //    {
        //        int num = 0;
        //        if (Styles != null)
        //            num = Styles.Count();
        //        return string.Format("共选择了{0}个分货款式", num);
        //    }
        //}

        //private IEnumerable<SysOrganization> _organizations;
        //public IEnumerable<SysOrganization> Organizations
        //{
        //    get { return _organizations; }
        //    set
        //    {
        //        _organizations = value;
        //        OnPropertyChanged("OrganizationsInfo");
        //    }
        //}

        //public string OrganizationsInfo
        //{
        //    get
        //    {
        //        int num = 0;
        //        if (Organizations != null)
        //            num = Organizations.Count();
        //        return string.Format("共选择了{0}个分货机构", num);
        //    }
        //}

        //private IEnumerable<SysOrganization> _organizationArray;
        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            //{
            //    if (_organizationArray == null || _organizationArray.Count() == 0)
            //        return null;
            //    return _organizationArray;
            //}
            set;
            //{
            //    _organizationArray = value;
            //}
        }

        private DataView _entities;
        public DataView Entities
        {
            get { return _entities; }
            set
            {
                _entities = value;
                OnPropertyChanged("Entities");
                OnPropertyChanged("CanAllocate");
            }
        }

        public bool CanAllocate
        {
            get
            {
                if (Entities != null && Entities.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        public AutoAllocateVM()
        {
            if (StorageInfoVM.Storages.Count == 1)
                StorageID = StorageInfoVM.Storages[0].ID;
        }

        private IEnumerable<OrganizationProductQuantity> GetSubordinateOrderAggregation(IEnumerable<int> pids)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            //var organizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var orderContext = lp.Search<BillOrder>(o => oids.Contains(o.OrganizationID));
            var orderDetailsContext = lp.GetDataContext<BillOrderDetails>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            orderContext = orderContext.Where(o => brandIDs.Contains(o.BrandID));
            //var orgContext = lp.GetDataContext<ViewOrganization>();
            var data = from order in orderContext
                       //from org in orgContext
                       //where order.OrganizationID == org.ID
                       from details in orderDetailsContext
                       where order.ID == details.BillID && pids.Contains(details.ProductID) && order.IsDeleted == false
                       select new OrganizationProductQuantity
                       {
                           OrganizationID = order.OrganizationID,
                           ProductID = details.ProductID,
                           Quantity = details.Quantity - details.QuaCancel - details.QuaDelivered
                       };
            var temp = data.GroupBy(o => new { o.ProductID, o.OrganizationID }).Select(g => new { g.Key, Quantity = g.Sum(o => o.Quantity) }).Where(g => g.Quantity != 0).ToList();
            var result = temp.Select(o => new OrganizationProductQuantity
            {
                OrganizationID = o.Key.OrganizationID,
                ProductID = o.Key.ProductID,
                Quantity = o.Quantity
            }).ToList();
            result.ForEach(o => o.OrganizationName = OrganizationArray.First(c => c.ID == o.OrganizationID).Name);
            return result;
        }

        private DataTable GenerateDataTable(IEnumerable<OrganizationProductQuantity> orders)
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("SKU码", typeof(string)));
            table.Columns.Add(new DataColumn("品牌", typeof(string)));
            table.Columns.Add(new DataColumn("款号", typeof(string)));
            table.Columns.Add(new DataColumn("色号", typeof(string)));
            table.Columns.Add(new DataColumn("尺码", typeof(string)));
            table.Columns.Add(new DataColumn("可用库存", typeof(int)));

            var oids = orders.Select(o => o.OrganizationID).Distinct();
            var grades = _linqOP.Search<OrganizationAllocationGrade>(o => oids.Contains(o.OrganizationID) && BrandID == o.BrandID).ToList();
            grades.ForEach(g =>
            {
                var temp = orders.Where(o => o.OrganizationID == g.OrganizationID);
                foreach (var t in temp)
                {
                    t.Grade = g.Grade;
                }
            });
            var onames = orders.OrderByDescending(o => o.Grade).Select(o => o.OrganizationName).Distinct().ToList();
            onames.Add("剩余可用库存");
            foreach (var on in onames)
            {
                table.Columns.Add(new DataColumn(on, typeof(int)));
            }

            return table;
        }

        public OPResult SearchData()
        {
            if (StorageID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择分货仓库." };
            }
            //if (OrganizationIDArray == null || OrganizationIDArray.Count() == 0)
            //{
            //    return new OPResult { IsSucceed = false, Message = "请选择分货机构." };
            //}
            if (Styles == null || Styles.Count() == 0)
            {
                return new OPResult { IsSucceed = false, Message = "请选择分货款式." };
            }
            var stocks = ReportDataContext.GetAvailableStock(StorageID,Styles);
            var pids = stocks.Select(o => o.ProductID);
            var orders = this.GetSubordinateOrderAggregation(pids);
            var table = this.GenerateDataTable(orders);
            pids = orders.Select(o => o.ProductID).Distinct();//可用库存和订单交集的productid
            var products = _linqOP.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList().OrderBy(o => o.ProductCode);
            foreach (var p in products)
            {
                DataRow row = table.NewRow();
                table.Rows.Add(row);
                row["SKU码"] = p.ProductCode;
                row["品牌"] = VMGlobal.PoweredBrands.Find(o => o.ID == p.BrandID).Code;
                row["款号"] = p.StyleCode;
                row["色号"] = VMGlobal.Colors.Find(o => o.ID == p.ColorID).Code;
                row["尺码"] = VMGlobal.Sizes.Find(o => o.ID == p.SizeID).Name;
                row["剩余可用库存"] = row["可用库存"] = stocks.First(s => s.ProductID == p.ProductID).Quantity;
                for (int i = 6; i < table.Columns.Count - 1; i++)
                {
                    var on = table.Columns[i].ColumnName;
                    var order = orders.FirstOrDefault(o => o.ProductID == p.ProductID && o.OrganizationName == on);
                    if (order != null)
                    {
                        row[on] = order.Quantity;
                    }
                }
            }
            Entities = table.DefaultView;
            return new OPResult { IsSucceed = true };
        }

        public OPResult Allocate()
        {
            if (this.Entities != null)
            {
                bool isFirst = true;
                var table = this.Entities.Table;
                var index = table.Columns.IndexOf("剩余可用库存");
                int count = table.Columns.Count - 1;
                if (index == count)
                {
                    for (int i = 6; i < count; i++)
                    {
                        var on = table.Columns[i].ColumnName;
                        var orderColName = on + "order";
                        var orderCol = new DataColumn(orderColName, typeof(int));
                        table.Columns.Add(orderCol);
                    }
                }
                else
                {
                    count = index;
                    isFirst = false;
                }
                try
                {
                    foreach (DataRowView row in this.Entities)
                    {
                        int stock = Convert.ToInt32(row["可用库存"]);
                        for (int i = 6; i < count; i++)
                        {
                            if (row[i] != DBNull.Value)
                            {
                                var orderColName = table.Columns[i].ColumnName + "order";
                                if (isFirst)
                                {
                                    row[orderColName] = row[i];
                                }
                                int order = Convert.ToInt32(row[orderColName]);
                                int allocate = Math.Min(stock, order);
                                stock -= allocate;
                                row[i] = allocate;
                                if (stock == 0)
                                    break;
                            }
                        }
                        row["剩余可用库存"] = stock;
                    }
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "分货失败,失败原因:\n" + e.Message };
                }
                return new OPResult { IsSucceed = true, Message = "分货成功." };
            }
            else
                return new OPResult { IsSucceed = false, Message = "请先查询库存与订单量." };
        }
    }

    internal class OrganizationProductQuantity : ProductQuantity
    {
        public int Grade { get; set; }
        public int OrganizationID { get; set; }
        public string OrganizationName { get; set; }
    }
}
