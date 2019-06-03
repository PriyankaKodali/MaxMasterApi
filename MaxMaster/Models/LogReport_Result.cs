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
    
    public partial class LogReport_Result
    {
        public string TaskId { get; set; }
        public string Client { get; set; }
        public string Department { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime EDOC { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedById { get; set; }
        public string TaskOwner { get; set; }
        public string TaskOwnerId { get; set; }
        public Nullable<System.DateTime> CompletedDate { get; set; }
        public string ParentTask_Id { get; set; }
        public string Status { get; set; }
        public string TaskType { get; set; }
        public string AssignedTo { get; set; }
        public string AssignedToId { get; set; }
        public string Project { get; set; }
        public Nullable<int> Quantity { get; set; }
        public string Client_Id { get; set; }
        public Nullable<int> Project_Id { get; set; }
        public string ProjectLocation { get; set; }
        public int TaskLogId { get; set; }
        public string TaskLogTaskId { get; set; }
        public System.DateTime TaskLogDate { get; set; }
        public string TaskLogAssignedTo { get; set; }
        public string TaskLogAssignedToId { get; set; }
        public string TaskLogAssignedBy { get; set; }
        public string TaskLogAssignedById { get; set; }
        public Nullable<int> TaskLogBudgetedHours { get; set; }
        public Nullable<System.DateTime> TaskLogStartDate { get; set; }
        public Nullable<System.DateTime> TaskLogEndDate { get; set; }
        public Nullable<int> TaskLogHoursWorked { get; set; }
        public string TaskLogStatus { get; set; }
        public string TaskLogDescription { get; set; }
        public Nullable<int> TaskLogQuantityWorked { get; set; }
        public Nullable<int> Notifications { get; set; }
    }
}