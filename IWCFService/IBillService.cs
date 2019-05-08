//*********************************************
// 公司名称：              
// 部    门：研发中心
// 创 建 者：徐时进
// 创建日期：2012-8-27 16:10:35
// 修 改 人：
// 修改日期：
// 修改说明：
// 文件说明：
//**********************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Kernel;
using ManufacturingModel;
using DistributionModel;
using SysProcessModel;

namespace IWCFService
{
    [ServiceContract(Namespace = "http://www.tuoxie.com/erp/")]
    public interface IBillService
    {
        /// <summary>
        /// 生成单据号
        /// </summary>
        /// <param name="billType">单据类型</param>
        /// <remarks>鉴于WCF参数不能传Type（Type为抽象类型，而运行时类型RuntimeType又不是公共类型,WCF无法解析），因此我将之改成Type String</remarks>
        [OperationContract]
        string GenerateBillCode(string billTypeName, int organizationID);

        /// <summary>
        /// 获取服务器当前时间(避免由于客户端时间不正确导致的一些问题)
        /// </summary>
        [OperationContract]
        DateTime GetDateTimeOfServer();

        [OperationContract]
        OPResult SaveProductExchangeBill(BillProductExchange pe, IEnumerable<BillProductExchangeDetails> details, BillSnapshot snapshot, IEnumerable<BillSnapshotDetailsWithUniqueCode> snapshotDetails);

        [OperationContract]
        OPResult DeleteSKU(int pid, ProStyleChange change);
    }
}
