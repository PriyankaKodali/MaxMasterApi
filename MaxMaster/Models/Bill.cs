//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MaxMaster.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Bill
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Bill()
        {
            this.BillAttachments = new HashSet<BillAttachment>();
            this.ItemsBillMappings = new HashSet<ItemsBillMapping>();
        }
    
        public int Id { get; set; }
        public string BillNumber { get; set; }
        public string SupplierId { get; set; }
        public System.DateTime BillDate { get; set; }
        public Nullable<System.DateTime> DueDate { get; set; }
        public string PaymentTerms { get; set; }
        public string Notes { get; set; }
        public string PoNumber { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public string DiscountType { get; set; }
        public Nullable<decimal> Total { get; set; }
        public Nullable<decimal> Subtotal { get; set; }
        public Nullable<decimal> TDSAmount { get; set; }
        public Nullable<int> TDSId { get; set; }
        public Nullable<decimal> BalanceDue { get; set; }
        public string Status { get; set; }
        public Nullable<decimal> CourierCharges { get; set; }
        public Nullable<decimal> RoundOffNumber { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BillAttachment> BillAttachments { get; set; }
        public virtual TD TD { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ItemsBillMapping> ItemsBillMappings { get; set; }
    }
}
