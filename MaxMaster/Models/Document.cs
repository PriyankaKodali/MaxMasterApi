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
    
    public partial class Document
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public System.DateTime UploadDate { get; set; }
        public Nullable<System.DateTime> DocumentDate { get; set; }
        public string DocumentURL { get; set; }
        public string Notes { get; set; }
        public int Employee_Id { get; set; }
        public string Keywords { get; set; }
    }
}