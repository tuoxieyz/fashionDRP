using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Windows.Controls;
using Model.Extension;
using System.Collections.ObjectModel;
using Kernel;
using System.Transactions;
using DistributionModel;
using DBAccess;
using SysProcessModel;
using SysProcessViewModel;
using ERPModelBO;

namespace ERPViewModelBasic
{
    public abstract class BillVMBase<TBill, TDetails, TItemShow> : ViewModelBase
        where TBill : BillBase, new()
        where TDetails : BillDetailBase
        where TItemShow : ProductShow, new()
    {
        #region 属性

        private LinqOPEncap _linqOP;

        public string InputLabelString { get; private set; }

        private TBill _master;
        /// <summary>
        /// 单据信息
        /// </summary>
        public TBill Master
        {
            get
            {
                if (_master == null)
                    _master = new TBill();
                return _master;
            }
            set
            {
                if (_master != value)
                {
                    _master = value;
                    OnPropertyChanged("Master");
                }
            }
        }

        /// <summary>
        /// 单据明细
        /// </summary>
        public List<TDetails> Details { get; set; }

        private ObservableCollection<TItemShow> _gridDataItems = new ObservableCollection<TItemShow>();
        /// <summary>
        /// 扫单产生的列表数据
        /// </summary>
        public ObservableCollection<TItemShow> GridDataItems { get { return _gridDataItems; } }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        //public ICommand DeleteItemCommand
        //{
        //    get
        //    {
        //        return new DelegateCommand(param =>
        //        {
        //            TItemShow item = (TItemShow)param;
        //            GridDataItems.Remove(item);
        //        });
        //    }
        //}

        #endregion

        public BillVMBase(LinqOPEncap linqOP)
        {
            _linqOP = linqOP;
            InputLabelString = "SKU码|款号";
        }

        /// <summary>
        /// 生成单据编号
        /// </summary>
        protected abstract string GenerateBillCode();

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init()
        {
            this.Master = null;
            this.Details = null;
            GridDataItems.Clear();
        }

        /// <summary>
        /// 款号或SKU码输入
        /// </summary>
        public virtual void ProductCodeInput(string pcode, Action<IEnumerable<TItemShow>> actionWhenMore)
        {
            if (GridDataItems.Count > 0)
            {
                foreach (var product in GridDataItems)
                {
                    if (product.ProductCode == pcode)
                    {
                        product.Quantity += 1;
                        return;
                    }
                }
            }
            var datas = this.GetProductForShow(pcode);
            if (datas != null && datas.Count > 0)
            {
                if (datas.Count == 1 && pcode == datas[0].ProductCode)//是条形码
                {
                    var item = GridDataItems.FirstOrDefault(o => o.ProductID == datas[0].ProductID);
                    if (item != null)
                        item.Quantity += 1;
                    else
                    {
                        datas[0].Quantity = 1;
                        GridDataItems.Add(datas[0]);
                    }
                }
                else
                {
                    if (actionWhenMore != null)
                        actionWhenMore(datas);
                }
            }
            else
            {
                throw new Exception("没有相关成品信息.");
            }
        }

        public virtual void AddRangeToItems(IEnumerable<TItemShow> datas)
        {
            foreach (var data in datas)
            {
                if (data.Quantity != 0)
                {
                    var item = GridDataItems.FirstOrDefault(o => o.ProductID == data.ProductID);
                    if (item != null)
                        item.Quantity += data.Quantity;
                    else
                        GridDataItems.Add(data);
                }
            }
        }

        /// <summary>
        /// 遍历列表数据
        /// </summary>
        public virtual void TraverseGridDataItems(Action<TItemShow> action)
        {
            foreach (var item in GridDataItems)
            {
                if (item.Quantity != 0)
                {
                    action(item);
                }
            }
        }

        public void DeleteItem(TItemShow item)
        {
            GridDataItems.Remove(item);
        }

        protected virtual List<TItemShow> GetProductForShow(string code)
        {
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var products = VMGlobal.SysProcessQuery.LinqOP.GetDataContext<Product>();
            var prostyles = VMGlobal.SysProcessQuery.LinqOP.GetDataContext<ProStyle>();
            var byqs = VMGlobal.SysProcessQuery.LinqOP.GetDataContext<ProBYQ>();
            var query = from s in prostyles
                        from byq in byqs
                        where s.BYQID == byq.ID && brandIDs.Contains(byq.BrandID)
                        from p in products
                        where p.StyleID == s.ID && (p.Code == code || s.Code == code) && p.Flag
                        select new
                        {
                            ProductID = p.ID,
                            ProductCode = p.Code,
                            BrandID = byq.BrandID,
                            StyleCode = s.Code,
                            ColorID = p.ColorID,
                            SizeID = p.SizeID,
                            Year = byq.Year,
                            Quarter = byq.Quarter,
                            Price = s.Price,
                            BYQID = s.BYQID
                        };
            var ps = query.ToList();
            if (ps.Count > 0)
            {
                return ps.Select(o => new TItemShow
                {
                    ProductID = o.ProductID,
                    ProductCode = o.ProductCode,
                    BrandCode = VMGlobal.PoweredBrands.Find(b => b.ID == o.BrandID).Code,
                    StyleCode = o.StyleCode,
                    ColorCode = VMGlobal.Colors.Find(c => c.ID == o.ColorID).Code,
                    SizeCode = VMGlobal.Sizes.Find(s => s.ID == o.SizeID).Code,
                    SizeName = VMGlobal.Sizes.Find(s => s.ID == o.SizeID).Name,
                    BrandID = o.BrandID,
                    Year = o.Year,
                    Quarter = o.Quarter,
                    BYQID = o.BYQID,
                    Price = o.Price
                }).ToList();
            }
            return null;
        }

        public virtual TItemShow GetProductForShow(int productID)
        {
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var products = VMGlobal.SysProcessQuery.LinqOP.GetDataContext<Product>();
            var prostyles = VMGlobal.SysProcessQuery.LinqOP.GetDataContext<ProStyle>();
            var byqs = VMGlobal.SysProcessQuery.LinqOP.GetDataContext<ProBYQ>();
            var query = from p in products
                        from s in prostyles
                        where p.StyleID == s.ID && p.ID == productID && p.Flag
                        from byq in byqs
                        where s.BYQID == byq.ID && brandIDs.Contains(byq.BrandID)
                        select new TItemShow
                        {
                            ProductID = productID,
                            ProductCode = p.Code,
                            BrandCode = VMGlobal.PoweredBrands.Find(b => b.ID == byq.BrandID).Code,
                            StyleCode = s.Code,
                            ColorCode = VMGlobal.Colors.Find(o => o.ID == p.ColorID).Code,
                            SizeCode = VMGlobal.Sizes.Find(o => o.ID == p.SizeID).Code,
                            SizeName = VMGlobal.Sizes.Find(o => o.ID == p.SizeID).Name,
                            BrandID = byq.BrandID,
                            Year = byq.Year,
                            Quarter = byq.Quarter,
                            BYQID = s.BYQID,
                            Price = s.Price
                        };
            return query.FirstOrDefault();
        }

        #region 保存单据

        /// <summary>
        /// 保存单据
        /// </summary>
        public virtual OPResult Save()
        {
            var bill = Master;
            bill.CreatorID = VMGlobal.CurrentUser.ID;
            bill.Code = this.GenerateBillCode();
            var details = Details;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    bill.ID = _linqOP.Add<TBill, int>(bill, b => b.ID);
                    details.ForEach(d => d.BillID = bill.ID);
                    _linqOP.Add<TDetails>(details);
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = e.Message };
                }
            }
            return new OPResult { IsSucceed = true, Message = "保存成功." };
        }

        /// <summary>
        /// 保存单据(不经过事务和异常处理)
        /// </summary>
        public virtual void SaveWithNoTran()
        {
            var bill = Master;
            bill.CreateTime = DateTime.Now;
            bill.CreatorID = VMGlobal.CurrentUser.ID;
            bill.Code = this.GenerateBillCode();
            bill.ID = _linqOP.Add<TBill, int>(bill, b => b.ID);
            Details.ForEach(d => d.BillID = bill.ID);
            _linqOP.Add<TDetails>(Details);
        }

        #endregion
    }
}
