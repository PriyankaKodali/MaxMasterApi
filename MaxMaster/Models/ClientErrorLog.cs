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
    
    public partial class ClientErrorLog
    {
        public int Id { get; set; }
        public string ScreenName { get; set; }
        public string MethodName { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackTrace { get; set; }
        public int License_Id { get; set; }
        public Nullable<System.DateTime> Time { get; set; }
    }
}
