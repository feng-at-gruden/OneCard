﻿//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace OneCard.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class onecardEntities : DbContext
    {
        public onecardEntities()
            : base("name=onecardEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<CardRecord> CardRecord { get; set; }
        public DbSet<CardRecord_His> CardRecord_His { get; set; }
        public DbSet<ExFloor_Record> ExFloor_Record { get; set; }
        public DbSet<ExFloor_Record24> ExFloor_Record24 { get; set; }
        public DbSet<Fitness> Fitness { get; set; }
        public DbSet<Fitness24> Fitness24 { get; set; }
        public DbSet<ZaoCanIn> ZaoCanIn { get; set; }
        public DbSet<ZaoCanIn24> ZaoCanIn24 { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Log> Log { get; set; }
    }
}
