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
    
    public partial class ItemsMaster
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ItemsMaster()
        {
            this.ItemPrices = new HashSet<ItemPrice>();
            this.Items = new HashSet<Item>();
            this.ItemsBillMappings = new HashSet<ItemsBillMapping>();
            this.POItemsMappings = new HashSet<POItemsMapping>();
            this.PuchaseOrders = new HashSet<PuchaseOrder>();
            this.SOItemsMappings = new HashSet<SOItemsMapping>();
            this.StockRequestMappings = new HashSet<StockRequestMapping>();
        }
    
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string ModelNumber { get; set; }
        public string Brand { get; set; }
        public string Manufacturer { get; set; }
        public Nullable<int> PowerInput { get; set; }
        public string PowerInputUnits { get; set; }
        public Nullable<int> MadeIn { get; set; }
        public string HSNCode { get; set; }
        public string UPC { get; set; }
        public string EAN { get; set; }
        public int ThresholdQuantity { get; set; }
        public string Description { get; set; }
        public string ItemContents { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public string Units { get; set; }
        public Nullable<decimal> GST { get; set; }
        public Nullable<decimal> CGST { get; set; }
        public Nullable<decimal> IGST { get; set; }
        public bool SrlNoExists { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Country Country { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ItemPrice> ItemPrices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Item> Items { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ItemsBillMapping> ItemsBillMappings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<POItemsMapping> POItemsMappings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PuchaseOrder> PuchaseOrders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SOItemsMapping> SOItemsMappings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StockRequestMapping> StockRequestMappings { get; set; }
    }
}
