using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Telerik.Windows.Controls;
using DistributionModel;
using System.Data;
using ERPViewModelBasic;
using ManufacturingModel;
using SysProcessViewModel;
using SysProcessModel;

namespace DistributionViewModel
{
    public class OrderBalanceSheetVM : ViewModelBase
    {
        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "订货品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string) }
                    };
                }
                return _itemPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _filterDescriptors;
        public CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {  
                        new FilterDescriptor("BrandID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue),
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        IEnumerable<ItemPropertyDefinition> _storageDefinitions;
        public IEnumerable<ItemPropertyDefinition> StorageDefinitions
        {
            get
            {
                if (_storageDefinitions == null)
                {
                    _storageDefinitions = new List<ItemPropertyDefinition>() {
                        new ItemPropertyDefinition { DisplayName = "仓库", PropertyName = "StorageID", PropertyType = typeof(int) }
                     };
                }
                return _storageDefinitions;
            }
        }

        ObservableCollection<FilterDescriptor> _storageDescriptors;
        public ObservableCollection<FilterDescriptor> StorageDescriptors
        {
            get
            {
                if (_storageDescriptors == null)
                {
                    _storageDescriptors = new ObservableCollection<FilterDescriptor>() 
                    {
                        new FilterDescriptor("StorageID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue)
                    };
                }
                return _storageDescriptors;
            }
        }

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
            }
        }

        private IEnumerable<OrganizationProductQuantity> GetSubordinateOrderAggregation()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            //var organizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var orderContext = lp.Search<BillOrder>(o => oids.Contains(o.OrganizationID) && brandIDs.Contains(o.BrandID));
            var orderDetailsContext = lp.GetDataContext<BillOrderDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var data = from order in orderContext
                       from details in orderDetailsContext
                       where order.ID == details.BillID && order.IsDeleted == false
                       from product in productContext
                       where product.ProductID == details.ProductID
                       select new BillEntityForAggregation
                       {
                           OrganizationID = order.OrganizationID,
                           BrandID = product.BrandID,
                           ProductID = product.ProductID,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity - details.QuaCancel - details.QuaDelivered
                       };
            data = (IQueryable<BillEntityForAggregation>)data.Where(FilterDescriptors);
            data = data.Where(o => o.Quantity != 0);
            var temp = data.GroupBy(o => new { o.ProductID, o.OrganizationID }).Select(g => new { g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
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
            var onames = orders.Select(o => o.OrganizationName).Distinct().ToList();
            foreach (var on in onames)
            {
                table.Columns.Add(new DataColumn(on, typeof(int)));
            }
            table.Columns.Add(new DataColumn("订单合计", typeof(int)));
            table.Columns.Add(new DataColumn("库存", typeof(int)));
            table.Columns.Add(new DataColumn("可用库存", typeof(int)));            
            table.Columns.Add(new DataColumn("半成品量", typeof(int)));
            table.Columns.Add(new DataColumn("需投产量", typeof(int)));
            return table;
        }

        private IEnumerable<ProductQuantity> GetStock(IEnumerable<int> pids)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var sids = StorageInfoVM.Storages.Select(o => o.ID);
            var stockContext = (IQueryable<Stock>)lp.Search<Stock>(o => sids.Contains(o.StorageID) && pids.Contains(o.ProductID)).Where(StorageDescriptors);
            var temp = stockContext.GroupBy(o => o.ProductID).Select(g => new ProductQuantity
            {
                ProductID = g.Key,
                Quantity = g.Sum(o => o.Quantity)
            }).ToList();
            return temp;
        }

        private IEnumerable<ProductQuantity> GetOccupantStock(IEnumerable<int> pids)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var sids = StorageInfoVM.Storages.Select(o => o.ID);
            int status = (int)BillDeliveryStatusEnum.已装箱未配送;
            var deliveries = (IQueryable<BillDelivery>)lp.Search<BillDelivery>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.Status == status && sids.Contains(o.StorageID)).Where(StorageDescriptors);
            var deliveryDetails = lp.GetDataContext<BillDeliveryDetails>();
            var deliveryData = from delivery in deliveries
                               from details in deliveryDetails
                               where delivery.ID == details.BillID && pids.Contains(details.ProductID)
                               select new ProductQuantity
                               {
                                   Quantity = details.Quantity,
                                   ProductID = details.ProductID
                               };
            //占用库存
            var deliveryResult = deliveryData.GroupBy(o => o.ProductID).Select(g => new ProductQuantity { ProductID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            return deliveryResult;
        }

        private IEnumerable<ProductQuantity> GetSemiProduct(IEnumerable<int> pids)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var subcontracts = lp.Search<BillSubcontract>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && !o.IsDeleted);
            var details = lp.GetDataContext<BillSubcontractDetails>();
            //下句不能直接用details = ，而要重新声明一个新的变量（此处为data），否则会报错，应该为IQToolkit的bug
            var data = from s in subcontracts
                      from d in details
                      where s.ID == d.BillID && pids.Contains(d.ProductID)
                      select d;
            var temp = data.GroupBy(o => o.ProductID).Select(g => new ProductQuantity
            {
                ProductID = g.Key,
                Quantity = g.Sum(o => o.Quantity - o.QuaCancel - o.QuaCompleted)
            }).ToList();
            return temp;
        }

        public void SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var orders = this.GetSubordinateOrderAggregation();
            var pids = orders.Select(o => o.ProductID).Distinct();
            var stocks = this.GetStock(pids);
            var occupants = this.GetOccupantStock(pids);
            var semis = this.GetSemiProduct(pids);//半成品
            var table = this.GenerateDataTable(orders);
            var products = lp.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList().OrderBy(o => o.ProductCode);
            foreach (var p in products)
            {
                DataRow row = table.NewRow();
                table.Rows.Add(row);
                row["SKU码"] = p.ProductCode;
                row["品牌"] = VMGlobal.PoweredBrands.Find(o => o.ID == p.BrandID).Code;
                row["款号"] = p.StyleCode;
                row["色号"] = VMGlobal.Colors.Find(o => o.ID == p.ColorID).Code;
                row["尺码"] = VMGlobal.Sizes.Find(o => o.ID == p.SizeID).Name;
                var stock = stocks.FirstOrDefault(s => s.ProductID == p.ProductID);
                if (stock != null)
                    row["库存"] = stock.Quantity;
                var occupant = occupants.FirstOrDefault(s => s.ProductID == p.ProductID);
                row["可用库存"] = ((stock == null ? 0 : stock.Quantity) - (occupant == null ? 0 : occupant.Quantity));
                row["订单合计"] = orders.Where(o => o.ProductID == p.ProductID).Sum(o => o.Quantity);
                var semi = semis.FirstOrDefault(s => s.ProductID == p.ProductID);
                if (semi != null)
                    row["半成品量"] = semi.Quantity;
                row["需投产量"] = Convert.ToInt32(row["订单合计"]) - Convert.ToInt32(row["可用库存"]) - (semi == null ? 0 : semi.Quantity);
                for (int i = 5; i < table.Columns.Count - 5; i++)
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
        }
    }
}
