using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using IWCFService;
using DBAccess;
using System.Linq.Expressions;
using DistributionModel;
using SysProcessModel;
using Kernel;
using ManufacturingModel;
using System.Transactions;

namespace WCFServiceHost
{
    public class BillService : IBillService
    {
        private QueryGlobal _queryDistr = new QueryGlobal("DistributionConstr");
        private QueryGlobal _queryManu = new QueryGlobal("ManufacturingConnection");
        private QueryGlobal _querySys = new QueryGlobal("SysProcessConstr");

        public string GenerateBillCode(string billTypeName, int organizationID)
        {
            Type billType = Type.GetType(billTypeName);
            var method = Expression.Call(Expression.Constant(this), "GenerateBillCode", new Type[] { billType }, Expression.Constant(organizationID));
            Func<string> func = Expression.Lambda<Func<string>>(method).Compile();
            return func();
        }

        private string GetOrganizationCode(int organizationID)
        {
            var ocode = _queryDistr.LinqOP.Search<ViewOrganization>(b => b.ID == organizationID).Select(o => o.Code).First();
            return ocode;
        }

        //private string GenerateBillCode<TBill>(int organizationID) where TBill : DistributionBillBase
        //{
        //    var maxCode = _query.LinqOP.Search<TBill>(t => t.OrganizationID == organizationID && t.CreateTime >= DateTime.Now.Date && t.CreateTime <= DateTime.Now.AddDays(1).Date).Max(t => t.Code);
        //    if (string.IsNullOrEmpty(maxCode))
        //    {
        //        int tag = (int)Enum.Parse(typeof(BillTypeEnum), typeof(TBill).Name);
        //        string prefixion = Enum.GetName(typeof(BillCodePrefixion), tag);
        //        maxCode = prefixion + this.GetOrganizationCode(organizationID) + "-" + DateTime.Now.ToString("yyyyMMdd") + "000";
        //    }
        //    int preLength = maxCode.Length - 3;
        //    return maxCode.Substring(0, preLength) + (Convert.ToInt32(maxCode.Substring(preLength)) + 1).ToString("000");
        //}

        public DateTime GetDateTimeOfServer()
        {
            return DateTime.Now;
        }

        public OPResult SaveProductExchangeBill(BillProductExchange pe, IEnumerable<BillProductExchangeDetails> details, BillSnapshot snapshot, IEnumerable<BillSnapshotDetailsWithUniqueCode> snapshotDetails)
        {
            var manulp = _queryManu.LinqOP;
            var distrlp = _queryDistr.LinqOP;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    pe.ID = manulp.Add<BillProductExchange, int>(pe, b => b.ID);
                    foreach (var d in details)
                    {
                        d.BillID = pe.ID;
                    }
                    manulp.Add<BillProductExchangeDetails>(details);
                    snapshot.BillID = pe.ID;
                    snapshot.ID = distrlp.Add<BillSnapshot, int>(snapshot, o => o.ID);
                    foreach (var snapshotDetail in snapshotDetails)
                    {
                        snapshotDetail.SnapshotID = snapshot.ID;
                    }
                    distrlp.Add<BillSnapshotDetailsWithUniqueCode>(snapshotDetails);
                    foreach (var d in details)
                    {
                        this.UpdateSubcontractWhenExchange(pe.OuterFactoryID, d.ProductID, d.Quantity);
                    }
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = e.Message };
                }
            }
            return new OPResult { IsSucceed = true, Message = "保存成功." };
        }

        private void UpdateSubcontractWhenExchange(int factoryID, int productID, int quantity)
        {
            _queryManu.DB.ExecuteNonQuery("UpdateSubcontractWhenExchange", factoryID, productID, quantity);
        }

        public OPResult DeleteSKU(int pid, ProStyleChange change)
        {
            var distlp = _queryDistr.LinqOP;
            var manulp = _queryManu.LinqOP;
            var syslp = _querySys.LinqOP;

            OPResult result = null;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    distlp.Delete<BillOrderDetails>(o => o.ProductID == pid);                    
                    manulp.Delete<BillProductExchangeDetails>(o => o.ProductID == pid);
                    manulp.Delete<BillProductPlanDetails>(o => o.ProductID == pid);
                    manulp.Delete<BillSubcontractDetails>(o => o.ProductID == pid);
                    syslp.Delete<ProductUniqueCodeMapping>(o => o.ProductID == pid);
                    syslp.Delete<Product>(o => o.ID == pid);
                    syslp.Add<ProStyleChange>(change);
                    result = new OPResult { IsSucceed = true, Message = "删除成功!" };
                    scope.Complete();
                }
                catch (Exception e)
                {
                    result = new OPResult { IsSucceed = false, Message = "删除出错,出错原因:\n" + e.Message + "\n请告知系统管理员,防止出现冗余数据." };
                }
            }
            return result;
        }
    }
}
