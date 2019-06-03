using MaxMaster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaxMaster.ViewModels
{


    public class ClientEmployeeModel
    {
        public int Client_Id { get; set; }
        public string client { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string SecondaryNumber { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Fax { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool Active { get; set; }
        public string ImageOne { get; set; }
        public string ImageTwo { get; set; }

    }

    public class ClientModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AspNetUserId { get; set; }
        public string ShortName { get; set; }
        public string Email { get; set; }
        public string PrimaryNum { get; set; }
        public string SecondaryNum { get; set; }
        public string PaymentType { get; set; }
        public string Currency { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string ClientType { get; set; }
        public string Fax { get; set; }
        public bool? IsActive { get; set; }
        public int OrgId { get; set; }
        public string OrgName { get; set; }
        public string ClientStatus { get; set; }
        public string GST { get; set; }
        public int? CreditPeriod { get; set; }
        public string PAN { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string IFSCCode { get; set; }
        public List<ClientVerticalsModel> Verticals { get; set; }
        public List<ClientLocationModel> ClientLocations { get; set; }

    }

    public class DoctorsModel
    {
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string SecondaryNumber { get; set; }
        public string Client { get; set; }
        public int Client_Id { get; set; }
        public string Addressline1 { get; set; }
        public string AddressLine2 { get; set; }
        public int Country_Id { get; set; }
        public string Country { get; set; }
        public int State_Id { get; set; }
        public string State { get; set; }
        public int City_Id { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string IdigitalId { get; set; }
        public string IdigitalAuthorId { get; set; }
        public string DictationMode { get; set; }
        public string JobLevel { get; set; }
        public string VoiceGrade { get; set; }
        public int MacroPercent { get; set; }
        public int? DoctorGroup_Id { get; set; }
        public string DoctorGroup { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<Specialty> Specialties { get; set; }

    }

    public class ClientLocationModel
    {
        public int? LocationId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Landmark { get; set; }
        public int Country { get; set; }
        public int State { get; set; }
        public int City { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public string zip { get; set; }
        public bool? IsInvoice { get; set; }
        public int? TimeZone { get; set; }
        public string TimeZoneLabel { get; set; }

    }

    public class ClientVerticalsModel
    {
        public int value { get; set; }
        public string label { get; set; }
    }

    public class EmployeeModel
    {
        public int EmpId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PrimaryPhoneNum { get; set; }
        public string SecondaryPhoneNum { get; set; }
        public string Aadhar { get; set; }
        public string Pan { get; set; }
        public string Gender { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public int Country { get; set; }
        public string CountryName { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int EmpType { get; set; }
        public string EmploymentType { get; set; }
        public int? ManagerId { get; set; }
        public string ManagerName { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public int DeptId { get; set; }
        public string DeptName { get; set; }
        public int DesgId { get; set; }
        public string DesgName { get; set; }
        public string DOB { get; set; }
        public string DOJ { get; set; }
        public string ZIP { get; set; }
        public string BloodGroup { get; set; }
        public int OrgId { get; set; }
        public string OrgName { get; set; }
        public int ProvisionalPeriod { get; set; }
        public int? ShiftId { get; set; }
        public string ShiftTimings { get; set; }
        public string PhotoUrl { get; set; }

    }

    public class EmployeePayScaleModel
    {
        public int CTC { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BranchName { get; set; }
    }

    public class DocumentModel
    {
        public string Category { get; set; }
        public string UploadDate { get; set; }
        public string DocumentDate { get; set; }
        public string Notes { get; set; }
        public string Keywords { get; set; }
    }

    public class DefaultAllocationsModel
    {
        public int Client_Id { get; set; }
        public string Client { get; set; }
        public int Doctor_Id { get; set; }
        public string Doctor { get; set; }
        public int Employee_Id { get; set; }
        public string Employee { get; set; }
        public bool? Inactive { get; set; }
        public int NoOfPages { get; set; }
        public decimal Accuracy { get; set; }
        public string JobLevel { get; set; }
    }

    public class OrganisationModel
    {
        public int Id { get; set; }
        public string OrgName { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string PrimaryPhone { get; set; }
        public string SecondaryPhone { get; set; }
        public string EmpPrefix { get; set; }
        public string GST { get; set; }
        public string TIN { get; set; }
        public string PAN { get; set; }
        public string ZIP { get; set; }
        public string OrgLogo { get; set; }
        public List<OrganisationLocation> OrgLocation { get; set; }
    }

    public class ActivititesModel
    {
        public int TaskId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? TaskDate { get; set; }
        public string TaskOwner { get; set; }
        public string CreatedBy { get; set; }
        public string Creator { get; set; }
        public DateTime? EDOC { get; set; }
        public DateTime? StartDate { get; set; }
        public string TaskType { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string  SubCategory { get; set; }
        public int? Quantity { get; set; }
        public string Client { get; set; }
        public string Project { get; set; }
    }

    public class ActivityLog
    {
        public int Id { get; set; }
        public string TaskId { get; set; }
        public string AssignedBy { get; set; }
        public string Description { get; set; }
        public string AssignedTo { get; set; }
        public int? HoursWorked { get; set; }
        public int? TotalHoursWorked { get; set; }
        public string Status { get; set; }
        public int? QuantityWorked { get; set; }
        public int? TotalQuantityWorked { get; set; }
        public DateTime? TaskDate { get; set; }
        public int? BudgetedHours { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string[] Attachments { get; set; }
        public DateTime? TaskCreatedDate { get; set; }
        public string AssignedById  { get; set; }
        public string AssignedToId { get; set; }

    }

    public class TaskAttachments
    {
        public string url { get; set; }
    }

    public class ActivitiesSummary
    {
        public int? TotalJobs { get; set; }
        public int? PriorityHighJobs { get; set; }
        public int? PriorityMediumJobs { get; set; }
        public int? PriorityLowJobs { get; set; }
      
    }

    public class ActivitiesSummaryReport
    {
        public int TotalToDo { get; set; }
        public int HighToDo { get; set; }
        public int MediumToDo { get; set; }
        public int LowToDo { get; set; }
        public int TotalByMe { get; set; }
        public int HighByMe { get; set; }
        public int MediumByMe { get; set; }
        public int LowByMe { get; set; }
        public int TotalThroughMe { get; set; }
        public int HighThroughMe { get; set; }
        public int MediumThroughMe { get; set; }
        public int LowThroughMe { get; set; }
        public int TotalToDoPending { get; set; }
        public int HighToDoPending { get; set; }
        public int MediumToDoPending { get; set; }
        public int LowToDoPending { get; set; }

    }

    public class keyValueModel
    {
        public int value { get; set; }
        public string label { get; set; }
    }

    public class OpportunitiesLogModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string AssignedTo { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public string[] Attachments { get; set; }

    }

    public class EmployeeTaskReport
    {
        public string TaskId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string TaskOwner { get; set; }
        public string AssignedTo { get; set; }
        public string AssignedToId { get; set; }
        public string TaskOwnerId { get; set; }
        public int? HoursWorked { get; set; }
        public string Subject { get; set; }
        public int Priority { get; set; }
        public string Status { get; set; }
        public DateTime? EDOC { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string TaskDescription { get; set; }
        public int? BudgetedHours { get; set; }
        public DateTime? StatDate { get; set; }
        public DateTime? ExpectedClosureDate { get; set; }
        public List<ActivityLog> ActivityLog { get; set; }
        public string Department { get; set; }
        public int? DepartmentId { get; set; }
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public DateTime? TaskResolvedDate { get; set; }
        public DateTime? TaskCreatedDate { get; set; }
        public int ActivityLogId { get; set; }
        public string TaskType { get; set; }
        public string Location { get; set; }
    }

    public class ClientContatsModel
    {
        public int ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class EmployeeDocumentModel
    {
        public string EmployeeName { get; set; }
        public string EmployeeNumber { get; set; }
        public int TotalCount { get; set; }
        public List<DocumentModel> Documents { get; set; }
        internal object FirstOrDefault()
        {
            throw new NotImplementedException();
        }
    }

    public class ClientServiceModel
    {
        public int Priority { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public string TaskOwner { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Subject { get; set; }
        public string TicketId { get; set; }
        public int? TotalTickets { get; set; }
    }

    public class ClientTicketLog
    {
        public string CreatedBy { get; set; }
        public DateTime? TaskDate { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
        public int ActivityLogId { get; set; }
        public List<Attachment> Attachments { get; set; }
        public string ActuallyAssignedTo { get; set; }
        public string AssignedToId { get; set; }

    }

    public class Attachment
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string AttachmentUrl { get; set; }
    }

    public class OrganisationLocation
    {
        public int? LocId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public int Country { get; set; }
        public int State { get; set; }
        public int City { get; set; }
        public string ZIP { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }

    }

    public class ItemMasterModel
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string ModelNum { get; set; }
        public string UPC { get; set; }
        public string EAN { get; set; }
        public string Country { get; set; }
        public int? CountryId { get; set; }
        public string Description { get; set; }
        public string Contents { get; set; }
        public int? PowerInput { get; set; }
        public string PowerInputUnits { get; set; }
        public string HSNCode { get; set; }
        public string Units { get; set; }
        public string Manufacturer { get; set; }
        public string Brand { get; set; }
        public int ThresholdValue { get; set; }
        public int GSTID { get; set; }
        public string GST { get; set; }
        public decimal? GSTRate { get; set; }
        public decimal? CostPrice { get; set; }
        public bool? SerialNoExists { get; set; }

    }

    public class ItemModel
    {
        public string Id { get; set; }
        public string SerialNo { get; set; }
        public string BatchNo { get; set; }
        public string HardwareVersion { get; set; }
        public string MacAddress { get; set; }
        public int? Warrenty { get; set; }
        public string WarrentyPeriod { get; set; }
        public decimal CP { get; set; }
        public decimal MinSP { get; set; }
        public decimal SP { get; set; }
        public decimal MRP { get; set; }
        public int Employee { get; set; }
        public int StockLocation { get; set; }
        public DateTime? ManufacturedDate { get; set; }
        public DateTime? StockInDate { get; set; }
        public string BillNumber { get; set; }
        public int MyProperty { get; set; }
        public List<ItemDetails> Items { get; set; }
        public string Location { get; set; }
        public string Units { get; set; }
        public bool SerialNumExists { get; set; }
    }

    public class ItemDetails
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string MacAddress { get; set; }
        public DateTime? ManufacturedDate { get; set; }
        public DateTime? StockOutDate { get; set; }
    }

    public class BilledItemsModel
    {
        public int? Id { get; set; }
        public int ModelId { get; set; }
        public string Tax { get; set; }
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }

    }

    public class BillModel
    {
        public int? Id { get; set; }
        public int BillId { get; set; }
        public string BillNumber { get; set; }
        public decimal? Total { get; set; }
        public int? TDSId { get; set; }
        public string TDSLabel { get; set; }
        public string PONumber { get; set; }
        public DateTime BillDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string PaymentTerms { get; set; }
        public decimal? CourierCharges { get; set; }
        public decimal? Discount { get; set; }
        public string DiscountType { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string Notes { get; set; }
        public decimal SubTotal { get; set; }
        public decimal? TDSAmount { get; set; }
        public string SupplierId { get; set; }
        public string Supplier { get; set; }
        public decimal? TDSRate { get; set; }
        public string UPC { get; set; }
        public decimal? BillAmount { get; set; }
        //   public int ModelId { get; set; }
        //   public string Model { get; set; }
        //    public bool SerialNumExists { get; set; }
        public decimal? DueAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? RoundOffNumber { get; set; }
        public int? TotalQuantity { get; set; }
        public int? QuantityUpdated { get; set; }
        public string Items { get; set; }
    }

    public class StockReport
    {
        public string ClientId { get; set; }
        public string Client { get; set; }
        public int ProjectId { get; set; }
        public string Project { get; set; }
        public string Status { get; set; }
        public int MyProperty { get; set; }
        public List<ProjectItemModel> Models { get; set; }
    }
    public class ProjectItemModel
    {
        public int? ModelId { get; set; }
        public string ModelName { get; set; }
        public int TotalItems { get; set; }
        public List<ItemDetails> Items { get; set; }
    }

    public class DispatchedStockModel
    {
        public string ClientId { get; set; }
        public string Client { get; set; }
        public int? ProjectId { get; set; }
        public string Project { get; set; }
        public int Quantity { get; set; }
        public List<ItemDetails> Items { get; set; }
    }

    public class Assignee
    {
        public string AssigneeId{ get; set; }
        public string AssigneeName { get; set; }
        public int? Quantity { get; set; }
    }

    public class TaskLog
    {
        public int TaskLogId { get; set; }
        public string TaskLogAssignedById { get; set; }
        public string TaskLogAssignedBy { get; set; }
        public DateTime TaskLogDate { get; set; }
        public string TaskLogAssignedToId { get; set; }
        public string TaskLogAssignedTo { get; set; }
        public string  TaskLogDescription { get; set; }
        public int? HoursWorked { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? BudgetedHours { get; set; }
        public int? QuantityWorked { get; set; }
        public string TaskLogStatus { get; set; }
        public List<LogAttachment> Attachments { get; set; }
    }

    public class LogAttachment
    {
        public int? AtchmentId { get; set; }
        public string Attachment { get; set; }
    }

    public class SubCat
    {
        public string Name { get; set; }
        public int Points { get; set; }
    }

    public class NewSubCategory
    {
        public int? SubCategoryId { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public int? PointsLogId { get; set; }
    }
}