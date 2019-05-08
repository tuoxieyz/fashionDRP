using System;
using System.Collections.Generic;
using ERPViewModelBasic;
using ManufacturingModel;
using Kernel;
using SysProcessViewModel;
using DistributionModel;
using System.ServiceModel;
using IWCFService;
using ERPModelBO;
using System.Transactions;

namespace Manufacturing.ViewModel
{
    public class BillProductExchangeVM : ManufacturingBillVM<BillProductExchange, BillProductExchangeDetails, ProductShow>
    {
        public BillProductExchangeVM()
        {
            Master = new BillProductExchangeBO();
            Master.CreateTime = DateTime.Now;
        }

        public OPResult ValidateWhenSave()
        {
            if (Master.OuterFactoryID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择相关的工厂" };
            }
            if (Master.BrandID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "未指定生产品牌" };
            }
            return new OPResult { IsSucceed = true };
        }

        public override OPResult Save()
        {
            Master.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            Details = new List<BillProductExchangeDetails>();
            TraverseGridDataItems(p =>
            {
                Details.Add(new BillProductExchangeDetails { ProductID = p.ProductID, Quantity = p.Quantity });
            });
            if (Details.Count == 0)
            {
                return new OPResult { IsSucceed = false, Message = "没有需要保存的数据" };
            }
            OPResult opresult = new OPResult { IsSucceed = false, Message = "保存失败!" };
#if UniqueCode
            Master.CreateTime = DateTime.Now;
            Master.CreatorID = VMGlobal.CurrentUser.ID;
            Master.Code = this.GenerateBillCode();
            BillSnapshot snapshot = new BillSnapshot
            {
                CreateTime = Master.CreateTime,
                CreatorName = VMGlobal.CurrentUser.Name,
                OrganizationName = OrganizationListVM.CurrentOrganization.Name,
                BillCode = Master.Code,
                Remark = Master.Remark
            };
            var typeField = typeof(BillTypeEnum).GetField(typeof(BillProductExchange).Name);
            var displayNames = typeField.GetCustomAttributes(typeof(EnumDescriptionAttribute), false);
            snapshot.BillTypeName = ((EnumDescriptionAttribute)displayNames[0]).Description;
            var snapshotDetails = this.GetUniqueCodes();
           
            using (ChannelFactory<IBillService> channelFactory = new ChannelFactory<IBillService>("BillSVC"))
            {
                IBillService service = channelFactory.CreateChannel();
                opresult = service.SaveProductExchangeBill(((BillProductExchangeBO)Master).ConvertToBase(), Details, snapshot, snapshotDetails);
            }
#else
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    base.SaveWithNoTran();
                    foreach (var d in Details)
                    {
                        this.UpdateSubcontractWhenExchange(Master.OuterFactoryID, d.ProductID, d.Quantity);
                    }
                    scope.Complete();
                }
                catch (Exception e)
                {
                    opresult = new OPResult { IsSucceed = false, Message = e.Message };
                }
            }
            opresult = new OPResult { IsSucceed = true, Message = "保存成功." };
#endif
            return opresult;

            //using (TransactionScope scope = new TransactionScope())
            //{
            //    try
            //    {
            //        base.SaveWithNoTran();
            //        Details.ForEach(d => this.UpdateSubcontractWhenExchange(Master.OuterFactoryID, d.ProductID, d.Quantity));
            //        scope.Complete();
            //    }
            //    catch (Exception e)
            //    {
            //        return new OPResult { IsSucceed = false, Message = e.Message };
            //    }
            //}
            //return new OPResult { IsSucceed = true, Message = "保存成功." };
        }

        private void UpdateSubcontractWhenExchange(int factoryID, int productID, int quantity)
        {
            VMGlobal.ManufacturingQuery.DB.ExecuteNonQuery("UpdateSubcontractWhenExchange", factoryID, productID, quantity);
        }
    }
}
