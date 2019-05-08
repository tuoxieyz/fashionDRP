using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kernel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DBAccess;
using ViewModelBasic;
using SysProcessModel;
using DomainLogicEncap;
using Model.Extension;

namespace SysProcessViewModel
{
    public class CertificationMakeVM : PagedEditSynchronousVM<Certification>
    {
        private LinqOPEncap _linqOP = VMGlobal.SysProcessQuery.LinqOP;
        private Dictionary<int, IEnumerable<OrganizationPriceFloat>> _pfCache;

        #region 属性

        private BusiDataDictionary _width, _height;
        public int Width
        {
            get
            {
                if (_width == null)
                    return 0;
                else
                {
                    int width;
                    int.TryParse(_width.Value, out width);
                    return width;
                }
            }
            set
            {
                if (_width != null)
                    _width.Value = value.ToString();
            }
        }

        public int Height
        {
            get
            {
                if (_height == null)
                    return 0;
                else
                {
                    int height;
                    int.TryParse(_height.Value, out height);
                    return height;
                }
            }
            set
            {
                if (_height != null)
                    _height.Value = value.ToString();
            }
        }

        private IEnumerable<CertMaterielKind> _materielKinds;
        public IEnumerable<CertMaterielKind> MaterielKinds
        {
            get
            {
                if (_materielKinds == null)
                {
                    _materielKinds = _linqOP.Search<CertMaterielKind>().ToList();
                }
                return _materielKinds;
            }
        }

        private IEnumerable<CertMateriel> _availableMateriels;
        public IEnumerable<CertMateriel> AvailableMateriels
        {
            get
            {
                if (_availableMateriels == null)
                {
                    _availableMateriels = _linqOP.Search<CertMateriel>(o => o.Enabled).ToList();
                }
                return _availableMateriels;
            }
        }

        private IEnumerable<CertSafetyTech> _safetyTechs;
        public IEnumerable<CertSafetyTech> SafetyTechs
        {
            get
            {
                if (_safetyTechs == null)
                {
                    _safetyTechs = _linqOP.Search<CertSafetyTech>().ToList();
                }
                return _safetyTechs;
            }
        }

        public IEnumerable<CertSafetyTech> AvailableSafetyTechs
        {
            get { return SafetyTechs.Where(o => o.Enabled); }
        }

        private IEnumerable<CertCarriedStandard> _carriedStandards;
        public IEnumerable<CertCarriedStandard> CarriedStandards
        {
            get
            {
                if (_carriedStandards == null)
                {
                    _carriedStandards = _linqOP.Search<CertCarriedStandard>().ToList();
                }
                return _carriedStandards;
            }
        }

        public IEnumerable<CertCarriedStandard> AvailableCarriedStandards
        {
            get { return CarriedStandards.Where(o => o.Enabled); }
        }

        private IEnumerable<CertGrade> _grades;
        public IEnumerable<CertGrade> Grades
        {
            get
            {
                if (_grades == null)
                {
                    _grades = _linqOP.Search<CertGrade>().ToList();
                }
                return _grades;
            }
        }

        public IEnumerable<CertGrade> AvailableGrades
        {
            get { return Grades.Where(o => o.Enabled); }
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
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string)}, 
                        new ItemPropertyDefinition { DisplayName = "质量等级", PropertyName = "Grade", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "执行标准", PropertyName = "CarriedStandard", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "安全技术类别", PropertyName = "SafetyTechnique", PropertyType = typeof(int)}
                    };
                }
                return _itemPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _filterDescriptors;
        public override CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {  
                        new FilterDescriptor("StyleCode", FilterOperator.Contains,  FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        #endregion

        public CertificationMakeVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = this.SearchData();
            var sizes = _linqOP.Search<BusiDataDictionary>(o => o.Name == "合格证尺寸");
            var children = _linqOP.GetDataContext<BusiDataDictionary>();
            var query = from child in children
                        from size in sizes
                        where child.ParentCode == size.Code
                        select child;
            var data = query.ToList();
            _width = data.FirstOrDefault(o => o.Name == "宽度");
            _height = data.FirstOrDefault(o => o.Name == "高度");
        }

        protected override IEnumerable<Certification> SearchData()
        {
            var result = base.SearchData().Select(o => new CertificationBO(o)).ToList();
            var sids = result.Select(o => o.StyleID);
            var styles = VMGlobal.SysProcessQuery.LinqOP.Search<ProStyle>(o => sids.Contains(o.ID)).ToList();
            foreach (var r in result)
            {
                var style = styles.Find(o => o.ID == r.StyleID);
                r.Style = style == null ? new ProStyleBO() : new ProStyleBO(style);
            }
            return result;
        }

        public IEnumerable<OrganizationPriceFloat> GetFloatPriceTactics(string styleCode)
        {
            var lp = VMGlobal.SysProcessQuery.LinqOP;
            var prostyle = lp.Search<ProStyle>(o => o.Code == styleCode).FirstOrDefault();
            if (prostyle != null)
            {
                if (_pfCache == null)
                    _pfCache = new Dictionary<int, IEnumerable<OrganizationPriceFloat>>();
                if (!_pfCache.ContainsKey(prostyle.BYQID))
                {
                    var oids = VMGlobal.ChildOrganizations.Select(o => o.ID);
                    var list = lp.Search<OrganizationPriceFloat>(o => o.BYQID == prostyle.BYQID && oids.Contains(o.OrganizationID)).ToList();
                    _pfCache.Add(prostyle.BYQID, list.Select(o =>
                    {
                        var organization = VMGlobal.ChildOrganizations.Find(c => c.ID == o.OrganizationID);
                        return new OrganizationPriceFloatForCertification
                        {
                            OrganizationID = organization.ID,
                            OrganizationCode = organization.Code,
                            OrganizationName = organization.Name,
                            FloatRate = o.FloatRate,
                            LastNumber = o.LastNumber
                        };
                    }));
                }
                return _pfCache[prostyle.BYQID];
            }
            return new List<OrganizationPriceFloatForCertification>();
        }

        public OPResult SaveSize()
        {
            try
            {
                if (_width != null)
                    _linqOP.Update<BusiDataDictionary>(_width);
                if (_height != null)
                    _linqOP.Update<BusiDataDictionary>(_height);
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:" + e.Message };
            }
            return new OPResult { IsSucceed = true, Message = "保存成功." };
        }
    }

    public class OrganizationPriceFloatForCertification : OrganizationPriceFloat
    {
        public string OrganizationCode { get; set; }
        public string OrganizationName { get; set; }
    }

    public class MaterielKindForCertificationSetVM : EditSynchronousVM<CertMaterielKind>
    {
        public MaterielKindForCertificationSetVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<CertMaterielKind> SearchData()
        {
            return base.SearchData().Select(o => new MaterielKindForCertificationBO(o)).ToList();
        }
    }

    public class MaterielForCertificationSetVM : EditSynchronousVM<CertMateriel>
    {
        public MaterielForCertificationSetVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<CertMateriel> SearchData()
        {
            return base.SearchData().Select(o => new MaterielForCertificationBO(o)).ToList();
        }
    }

    public class SafetyTechForCertificationSetVM : EditSynchronousVM<CertSafetyTech>
    {
        public SafetyTechForCertificationSetVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<CertSafetyTech> SearchData()
        {
            return base.SearchData().Select(o => new SafetyTechForCertificationBO(o)).ToList();
        }

        public override OPResult Delete(CertSafetyTech entity)
        {
            if (entity.ID != default(int))
            {
                if (LinqOP.Any<Certification>(o => o.SafetyTechnique == entity.ID))
                    return new OPResult { IsSucceed = false, Message = "该安全技术类别已被使用，若不再使用请将状态置为禁用。" };
            }
            return base.Delete(entity);
        }
    }

    public class CarriedStandardForCertificationSetVM : EditSynchronousVM<CertCarriedStandard>
    {
        public CarriedStandardForCertificationSetVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<CertCarriedStandard> SearchData()
        {
            return base.SearchData().Select(o => new CarriedStandardForCertificationBO(o)).ToList();
        }

        public override OPResult Delete(CertCarriedStandard entity)
        {
            if (entity.ID != default(int))
            {
                if (LinqOP.Any<Certification>(o => o.CarriedStandard == entity.ID))
                    return new OPResult { IsSucceed = false, Message = "该执行标准已被使用，若不再使用请将状态置为禁用。" };
            }
            return base.Delete(entity);
        }
    }

    public class GradeForCertificationSetVM : EditSynchronousVM<CertGrade>
    {
        public GradeForCertificationSetVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<CertGrade> SearchData()
        {
            return base.SearchData().Select(o => new GradeForCertificationBO(o)).ToList();
        }

        public override OPResult Delete(CertGrade entity)
        {
            if (entity.ID != default(int))
            {
                if (LinqOP.Any<Certification>(o => o.Grade == entity.ID))
                    return new OPResult { IsSucceed = false, Message = "该质量等级已被使用，若不再使用请将状态置为禁用。" };
            }
            return base.Delete(entity);
        }
    }
}
