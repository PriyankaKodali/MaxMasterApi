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
    
    public partial class GetTasksThroughMe_Result
    {
        public Nullable<long> RowNum { get; set; }
        public int Category_Id { get; set; }
        public int SubCategory_Id { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string Subject { get; set; }
        public string TaskId { get; set; }
        public string Status { get; set; }
        public int Priority { get; set; }
        public string CategorySubCategory { get; set; }
        public string TaskOwner { get; set; }
        public Nullable<int> TAT { get; set; }
        public string Client_Id { get; set; }
        public string ClientName { get; set; }
        public Nullable<int> Department_Id { get; set; }
        public string Department { get; set; }
        public string TaskType { get; set; }
        public string EmpCreatedBy { get; set; }
        public string EmpTaskOwner { get; set; }
        public string ActuallyAssignedTo { get; set; }
        public Nullable<int> TotalCount { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> Notifications { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public string LastUpdatedById { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}