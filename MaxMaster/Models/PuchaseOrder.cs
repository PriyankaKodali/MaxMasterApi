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
    
    public partial class PuchaseOrder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PuchaseOrder()
        {
            this.POAttachments = new HashSet<POAttachment>();
            this.POItemsMappings = new HashSet<POItemsMapping>();
        }
    
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public int ItemId { get; set; }
        public string PONumber { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime ExpectedDeliveryDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string DeliverTo { get; set; }
        public int DeliveryLocationId { get; set; }
        public string ClientId { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public string Notes { get; set; }
        public string TermsAndConditions { get; set; }
    
        public virtual ItemsMaster ItemsMaster { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<POAttachment> POAttachments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<POItemsMapping> POItemsMappings { get; set; }
    }
}
