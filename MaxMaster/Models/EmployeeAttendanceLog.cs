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
    
    public partial class EmployeeAttendanceLog
    {
        public int Id { get; set; }
        public string Emp_Id { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Nullable<bool> IsClockIn { get; set; }
        public Nullable<bool> IsClockOut { get; set; }
        public Nullable<System.DateTime> Time { get; set; }
        public Nullable<System.DateTime> InsertedTime { get; set; }
        public string Address { get; set; }
    }
}
