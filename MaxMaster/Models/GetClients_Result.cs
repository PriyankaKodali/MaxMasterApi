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
    
    public partial class GetClients_Result
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Nullable<int> Vendor_Id { get; set; }
        public string ClientType { get; set; }
        public string Fax { get; set; }
        public bool Active { get; set; }
        public int OrgId { get; set; }
        public string OrgName { get; set; }
        public Nullable<int> TotalCount { get; set; }
        public string AspNetUserId { get; set; }
    }
}
