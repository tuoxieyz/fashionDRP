using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DomainLogicEncap;
using DistributionModel;
using SysProcessViewModel;
using Kernel;
using System.Transactions;
using IWCFServiceForIM;
using SysProcessModel;

namespace DistributionViewModel
{
    public class OrderManageVM : BillPagedReportVM<OrderSearchEntity>
    {
        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateDate", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "订货品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)}
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
                        new FilterDescriptor("CreateDate", FilterOperator.IsGreaterThanOrEqualTo, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("CreateDate", FilterOperator.IsLessThanOrEqualTo, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("Code", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        //public OrderManageVM()
        //{
        //    var childOrgs = VMGlobal.ChildOrganizations;
        //    if (childOrgs != null && childOrgs.Count > 0)
        //    {
        //        FilterDescriptors.Add(new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue));
        //    }
        //    Entities = this.SearchData();
        //}

        protected override IEnumerable<OrderSearchEntity> SearchData()
        {
            int totalCount = 0;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var data = ReportDataContext.SearchBillOrder(FilterDescriptors, DetailsDescriptors,oids, PageIndex, PageSize, ref totalCount);
            TotalCount = totalCount;
            return data;
        }

        public void SetQuantityForOrderEntity(OrderSearchEntity entity)
        {
            //var order = VMGlobal.DistributionQuery.LinqOP.Search<BillOrderDetails>(o => o.BillID == entity.BillID && o.IsDeleted == false).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, OrderQuantity = g.Sum(o => o.Quantity), CancelQuantity = g.Sum(o => o.QuaCancel), DeliveredQuantity = g.Sum(o => o.QuaDelivered) }).First();
            entity.订货数量 = entity.Details.Sum(o => o.Quantity);//order.OrderQuantity;
            entity.取消量 = entity.Details.Sum(o => o.QuaCancel);//order.CancelQuantity;
            entity.已发数量 = entity.Details.Sum(o => o.QuaDelivered);//order.DeliveredQuantity;
            var realOrderQuantity = entity.订货数量 - entity.取消量;
            entity.发货状态 = realOrderQuantity == entity.已发数量 ? "已完成" : (entity.已发数量 == 0 ? "未发货" : (realOrderQuantity > entity.已发数量 ? "部分已发货" : "数据有误"));
        }

        /// <summary>
        /// 取消量归零
        /// </summary>
        public OPResult ZeroCancelOrderQuantity(OrderSearchEntity order)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var orders = lp.Search<BillOrderDetails>(o => o.BillID == order.BillID).ToList();
            orders = orders.Where(o => o.QuaCancel != 0).ToList();
            if (orders.Count == 0)
            {
                return new OPResult { IsSucceed = false, Message = "取消量已经归零." };
            }
            int quaCancel = 0;//原取消量
            orders.ForEach(o =>
            {
                quaCancel += o.QuaCancel;
                o.QuaCancel = 0;
            });
            BillOrderChange change = new BillOrderChange
            {
                BillID = order.BillID,
                CreateTime = DateTime.Now,
                CreatorID = VMGlobal.CurrentUser.ID,
                Description = string.Format("取消量归零,还原量{0}件.", quaCancel)
            };
            OPResult result = null;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    lp.Update<BillOrderDetails>(orders);
                    lp.Add<BillOrderChange>(change);
                    result = new OPResult { IsSucceed = true, Message = "取消量归零成功!" };
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    result = new OPResult { IsSucceed = false, Message = "取消量归零失败,失败原因:\n" + ex.Message };
                }
            }
            if (result.IsSucceed)
            {
                foreach (var d in order.Details)
                    d.QuaCancel = 0;
                order.Changes.Insert(0, change);
                var users = GetIMUsers(order.OrganizationID);
                IMHelper.AsyncSendMessageTo(users, new IMessage
                {
                    Message = string.Format("订单{0}取消量归零,还原量{1}件.", order.BillCode, quaCancel)
                }, IMReceiveAccessEnum.订单变动);
            }
            return result;
        }

        /// <summary>
        /// 取消剩余订单
        /// </summary>
        public OPResult CancelLeftOrderQuantity(OrderSearchEntity order)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var orders = lp.Search<BillOrderDetails>(o => o.BillID == order.BillID && o.QuaCancel != o.Quantity - o.QuaDelivered).ToList();
            if (orders.Count == 0)
            {
                return new OPResult { IsSucceed = false, Message = "没有可取消的量." };
            }
            int quaCancel = 0;
            orders.ForEach(o =>
            {
                quaCancel += o.Quantity - o.QuaDelivered - o.QuaCancel;
                o.QuaCancel = o.Quantity - o.QuaDelivered;
            });
            BillOrderChange change = new BillOrderChange
            {
                BillID = order.BillID,
                CreateTime = DateTime.Now,
                CreatorID = VMGlobal.CurrentUser.ID,
                Description = string.Format("取消剩余订单量,取消量{0}件.", quaCancel)
            };
            OPResult result = null;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    lp.Update<BillOrderDetails>(orders);
                    lp.Add<BillOrderChange>(change);
                    result = new OPResult { IsSucceed = true, Message = "取消成功!" };
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    result = new OPResult { IsSucceed = false, Message = "取消失败,失败原因:\n" + ex.Message };
                }
            }
            if (result.IsSucceed)
            {
                foreach (var d in order.Details)
                    d.QuaCancel = d.Quantity - d.QuaDelivered;
                order.Changes.Insert(0, change);
                var users = GetIMUsers(order.OrganizationID);
                IMHelper.AsyncSendMessageTo(users, new IMessage
                {
                    Message = string.Format("订单{0}剩余量被取消,取消量{1}件.", order.BillCode, quaCancel)
                }, IMReceiveAccessEnum.订单变动);
            }
            return result;
        }

        /// <summary>
        /// 恢复单据
        /// </summary>
        public OPResult RevertOrder(OrderSearchEntity entity)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var order = lp.GetById<BillOrder>(entity.BillID);
            if (order == null)
                return new OPResult { IsSucceed = false, Message = "未找到相应单据." };
            if (!order.IsDeleted)
                return new OPResult { IsSucceed = false, Message = "订单未作废." };
            BillOrderChange change = new BillOrderChange
            {
                BillID = order.ID,
                CreateTime = DateTime.Now,
                CreatorID = VMGlobal.CurrentUser.ID,
                Description = "恢复作废订单"
            };
            order.IsDeleted = false;
            OPResult result = null;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    lp.Update<BillOrder>(order);
                    lp.Add<BillOrderChange>(change);
                    result = new OPResult { IsSucceed = true, Message = "恢复成功!" };
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    result = new OPResult { IsSucceed = false, Message = "恢复失败,失败原因:\n" + ex.Message };
                }
            }
            if (result.IsSucceed)
            {
                entity.Changes.Insert(0, change);
                var users = GetIMUsers(order.OrganizationID);
                IMHelper.AsyncSendMessageTo(users, new IMessage
                {
                    Message = string.Format("原作废订单{0}被恢复.", order.Code)
                }, IMReceiveAccessEnum.订单变动);
            }
            return result;
        }

        /// <summary>
        /// 逻辑删除
        /// </summary>
        public OPResult DeleteOrder(OrderSearchEntity entity)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var order = lp.GetById<BillOrder>(entity.BillID);
            if (order == null)
                return new OPResult { IsSucceed = false, Message = "未找到相应单据." };
            if (order.IsDeleted)
                return new OPResult { IsSucceed = false, Message = "订单已作废." };
            BillOrderChange change = new BillOrderChange
            {
                BillID = order.ID,
                CreateTime = DateTime.Now,
                CreatorID = VMGlobal.CurrentUser.ID,
                Description = "作废订单"
            };
            order.IsDeleted = true;
            OPResult result = null;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    lp.Update<BillOrder>(order);
                    lp.Add<BillOrderChange>(change);
                    result = new OPResult { IsSucceed = true, Message = "作废成功!" };
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    result = new OPResult { IsSucceed = false, Message = "作废失败,失败原因:\n" + ex.Message };
                }
            }
            if (result.IsSucceed)
            {
                entity.Changes.Insert(0, change);
                var users = GetIMUsers(order.OrganizationID);
                IMHelper.AsyncSendMessageTo(users, new IMessage
                {
                    Message = string.Format("订单{0}被作废.", order.Code)
                }, IMReceiveAccessEnum.订单变动);
            }
            return result;
        }

        public OPResult UpdateDetailsCancelQuantity(int did, int quaNew, string skuCode, OrderSearchEntity order)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var details = lp.GetById<BillOrderDetails>(did);
            if (quaNew == details.QuaCancel)
            {
                return new OPResult { IsSucceed = true };
            }
            string des = string.Format("SKU[{2}]取消量从{0}件改动为{1}件", details.QuaCancel, quaNew, skuCode);
            details.QuaCancel = quaNew;
            BillOrderChange change = new BillOrderChange
            {
                BillID = details.BillID,
                CreateTime = DateTime.Now,
                CreatorID = VMGlobal.CurrentUser.ID,
                Description = des
            };
            OPResult result = null;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    lp.Update<BillOrderDetails>(details);
                    lp.Add<BillOrderChange>(change);
                    result = new OPResult { IsSucceed = true, Message = "改动成功!" };
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    result = new OPResult { IsSucceed = false, Message = "改动失败,失败原因:\n" + ex.Message };
                }
            }
            if (result.IsSucceed)
            {
                order.Changes.Insert(0, change);
                var users = GetIMUsers(order.OrganizationID);
                IMHelper.AsyncSendMessageTo(users, new IMessage
                {
                    Message = string.Format("订单{0}" + des, order.BillCode)
                }, IMReceiveAccessEnum.订单变动);
            }
            return result;
        }

        private IEnumerable<ClientUserPoint> GetIMUsers(int oid)
        {
            List<int> oids = new List<int>();
            oids.Add(oid);
            if (oid == VMGlobal.CurrentUser.OrganizationID)
                oids.Add(OrganizationListVM.CurrentOrganization.ParentID);
            else
                oids.Add(VMGlobal.CurrentUser.OrganizationID);
            return IMHelper.OnlineUsers.Where(o => oids.Contains(o.OrganizationID)).ToArray();
        }
    }
}
