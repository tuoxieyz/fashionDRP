using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Kernel;
using SysProcessViewModel;
using DomainLogicEncap;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class StorageInfoVM : EditSynchronousVM<Storage>
    {
        public StorageInfoVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<Storage> SearchData()
        {
            return LinqOP.Search<Storage>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).Select(o => new StorageBO(o)).ToList();
        }

        public override OPResult AddOrUpdate(Storage entity)
        {
            var result = base.AddOrUpdate(entity);
            if (result.IsSucceed)
            {
                var storage = StorageInfoVM.Storages.Find(o => o.ID == entity.ID);
                if (storage == null)
                    StorageInfoVM.Storages.Add(entity);
                else
                {
                    int index = StorageInfoVM.Storages.IndexOf(storage);
                    StorageInfoVM.Storages[index] = entity;
                }
                if (!entity.Flag)
                    StorageInfoVM.Storages.Remove(entity);
            }
            return result;
        }

        private static List<Storage> _storages;
        /// <summary>
        /// 状态为正常的仓库集合
        /// </summary>
        public static List<Storage> Storages
        {
            get
            {
                if (_storages == null)
                {
                    _storages = OrganizationLogic.GetStorages(VMGlobal.CurrentUser.OrganizationID);
                    VMGlobal.RefreshingEvent -= VMGlobal_RefreshingEvent;
                    VMGlobal.RefreshingEvent += VMGlobal_RefreshingEvent;
                }
                return _storages;
            }
        }

        static void VMGlobal_RefreshingEvent()
        {
            _storages = null;
        }
    }
}
