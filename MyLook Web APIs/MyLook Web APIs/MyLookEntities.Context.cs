﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyLook_Web_APIs
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class mylookEntities : DbContext
    {
        public mylookEntities()
            : base("name=mylookEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<avaliacao> avaliacao { get; set; }
        public virtual DbSet<convite> convite { get; set; }
        public virtual DbSet<foto> foto { get; set; }
        public virtual DbSet<tipoavaliacao> tipoavaliacao { get; set; }
        public virtual DbSet<usuario> usuario { get; set; }
        public virtual DbSet<interesses> interesses { get; set; }
        public virtual DbSet<seguidores> seguidores { get; set; }
    }
}
