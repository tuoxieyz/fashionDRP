﻿//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DBEncapsulation
{
    using DbContextExtension;
    using ManufacturingModel;
    using SysProcessModel;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    public partial class ManufacturingEntities : AdapDbContext
    {
        public ManufacturingEntities()
            : base("ManufacturingConnection", "res://*/Manufacturing.csdl|res://*/Manufacturing.ssdl|res://*/Manufacturing.msl")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<BillProductExchange> BillProductExchange { get; set; }
        public DbSet<BillProductExchangeDetails> BillProductExchangeDetails { get; set; }
        public DbSet<BillProductPlan> BillProductPlan { get; set; }
        public DbSet<BillProductPlanDetails> BillProductPlanDetails { get; set; }
        public DbSet<BillSubcontract> BillSubcontract { get; set; }
        public DbSet<BillSubcontractDetails> BillSubcontractDetails { get; set; }
        //public DbSet<BusiDataDictionary> BusiDataDictionary { get; set; }
        public DbSet<Factory> Factory { get; set; }
        public DbSet<ViewProduct> ViewProduct { get; set; }
        public DbSet<ViewUser> ViewUser { get; set; }
    
    }
}
