//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class tipoavaliacao
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tipoavaliacao()
        {
            this.avaliacao = new HashSet<avaliacao>();
            this.interesses = new HashSet<interesses>();
        }
    
        public int id { get; set; }
        public string Descricao { get; set; }
        public System.DateTime C_dataCriacao { get; set; }
        public System.DateTime C_dataAlteracao { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<avaliacao> avaliacao { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<interesses> interesses { get; set; }
    }
}
