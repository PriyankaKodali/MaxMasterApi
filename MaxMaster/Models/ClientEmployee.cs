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
    
    public partial class ClientEmployee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ClientEmployee()
        {
            this.OpportunityContactMappings = new HashSet<OpportunityContactMapping>();
        }
    
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string PrimaryPhone { get; set; }
        public string SecondaryPhone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public int Client_Id { get; set; }
        public bool Active { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public string AspNetUserId { get; set; }
        public string ImageOne { get; set; }
        public string ImageTwo { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual ClientEmployee ClientEmployees1 { get; set; }
        public virtual ClientEmployee ClientEmployee1 { get; set; }
        public virtual Client Client { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OpportunityContactMapping> OpportunityContactMappings { get; set; }
    }
}
