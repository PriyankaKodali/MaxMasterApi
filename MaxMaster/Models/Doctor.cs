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
    
    public partial class Doctor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Doctor()
        {
            this.DefaultAllocations = new HashSet<DefaultAllocation>();
            this.Specialties = new HashSet<Specialty>();
        }
    
        public int Id { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string PrimaryPhone { get; set; }
        public string SecondaryPhone { get; set; }
        public string Email { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string ZIP { get; set; }
        public string DictationMode { get; set; }
        public string JobLevel { get; set; }
        public string VoiceGrade { get; set; }
        public byte MacroPercent { get; set; }
        public int Client_Id { get; set; }
        public int City_Id { get; set; }
        public int Country_Id { get; set; }
        public int State_Id { get; set; }
        public Nullable<int> DoctorGroup_Id { get; set; }
        public string IdigitalId { get; set; }
        public string IdigitalAuthorId { get; set; }
        public bool Active { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public string AspNetUserId { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual City City { get; set; }
        public virtual Client Client { get; set; }
        public virtual Country Country { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DefaultAllocation> DefaultAllocations { get; set; }
        public virtual DoctorGroup DoctorGroup { get; set; }
        public virtual State State { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Specialty> Specialties { get; set; }
    }
}
