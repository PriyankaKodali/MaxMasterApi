using MaxMaster.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace MaxMaster.Controllers
{
    public class EmployeeLocationController : ApiController
    {
      //  [HttpPost]
        //public IHttpActionResult LogLocationData()
        //{
        //    try
        //    {
        //        using (MaxMasterDbEntities db = new MaxMasterDbEntities())
        //        {
        //            var form = HttpContext.Current.Request.Form;
        //            List<LocationInfo> data = JsonConvert.DeserializeObject<List<LocationInfo>>(form.Get("locationData"));

        //            var LoginUser = User.Identity.GetUserId();
        //            int employeeId = db.Employees.Where(x => x.AspNetUserId == LoginUser && x.Active == true).Select(x => x.Id).FirstOrDefault();
        //            foreach (var location in data)
        //            {
        //                EmployeeAttendanceLog eal = new EmployeeAttendanceLog()
        //                {
        //                    Emp_Id= employeeId,
        //                    Latitude = location.Latitude,
        //                    Longitude = location.Longitude,
        //                    IsClockIn = location.IsClockIn,
        //                    IsClockOut = location.IsClockOut,
        //                    Time = MasterDataController.GetIndianTime(location.Time),
        //                    InsertedTime = MasterDataController.GetIndianTime(DateTime.UtcNow)
        //                };
        //                db.EmployeeAttendanceLogs.Add(eal);
        //            }
        //            db.SaveChanges();
        //            return Ok();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
        //        return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
        //    }
        //}
    }

    public class LocationInfo
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool IsClockIn { get; set; }
        public bool IsClockOut { get; set; }
        public DateTime Time { get; set; }
    }
}