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
    
    public partial class GetToDoLeads_Result
    {
        public Nullable<long> RowNum { get; set; }
        public int Id { get; set; }
        public string OpportunityName { get; set; }
        public string TaskOwner { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public Nullable<int> Location_Id { get; set; }
        public string LastUpdatedBy { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public string Description { get; set; }
        public string Client { get; set; }
        public Nullable<int> TAT { get; set; }
        public string OppOwner { get; set; }
        public string OppCreator { get; set; }
        public string OppLastUpdate { get; set; }
        public string OppLocation { get; set; }
        public Nullable<int> TotalCount { get; set; }
    }
}
