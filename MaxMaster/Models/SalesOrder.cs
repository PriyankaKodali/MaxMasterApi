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
    
    public partial class SalesOrder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SalesOrder()
        {
            this.SOAttachments = new HashSet<SOAttachment>();
            this.SOItemsMappings = new HashSet<SOItemsMapping>();
            this.StockOuts = new HashSet<StockOut>();
        }
    
        public int Id { get; set; }
        public string SONumber { get; set; }
        public string CustomerId { get; set; }
        public string ReferenceNumber { get; set; }
        public System.DateTime SODate { get; set; }
        public System.DateTime ExpectedShipmentDate { get; set; }
        public int DeliveryMethod { get; set; }
        public string CustomerNotes { get; set; }
        public string TermsAndConditions { get; set; }
        public string SalesPersonId { get; set; }
        public Nullable<decimal> GST { get; set; }
        public Nullable<decimal> CGST { get; set; }
        public Nullable<decimal> IGST { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SOAttachment> SOAttachments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SOItemsMapping> SOItemsMappings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StockOut> StockOuts { get; set; }
    }
}
