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
    
    public partial class GetStockRequests_Result
    {
        public int Id { get; set; }
        public string Client { get; set; }
        public int Project { get; set; }
        public string Status { get; set; }
        public System.DateTime ExpectedStockDate { get; set; }
        public string ShortName { get; set; }
        public string Employee { get; set; }
        public Nullable<int> ProjectId { get; set; }
        public System.DateTime RequestDate { get; set; }
        public Nullable<int> Location_Id { get; set; }
        public string EmployeeName { get; set; }
        public string ProjectName { get; set; }
        public Nullable<int> TotalCount { get; set; }
    }
}
