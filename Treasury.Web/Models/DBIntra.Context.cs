﻿//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Treasury.Web.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DB_INTRAEntities : DbContext
    {
        public DB_INTRAEntities()
            : base("name=DB_INTRAEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<V_EMPLY2> V_EMPLY2 { get; set; }
        public virtual DbSet<VW_OA_DEPT> VW_OA_DEPT { get; set; }
        public virtual DbSet<VW_OA_EMP> VW_OA_EMP { get; set; }
    }
}
