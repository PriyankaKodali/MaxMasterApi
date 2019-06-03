using MaxMaster.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml;

namespace MaxMaster.Controllers
{
    public class EmployeeLocationController : ApiController
    {
       
        [HttpPost]
        public IHttpActionResult LogLocationData(object myObject)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var root = JObject.Parse(myObject.ToString()).First.First;
                    var location = JsonConvert.DeserializeObject<LocationInfo>(root.ToString());
                    if(db.EmployeeAttendanceLogs.Any(x=>x.Emp_Id==location.EmployeeId && location.Time == x.Time))
                    {
                        return Ok();
                    }
                    string Address = null;
                    try
                    {
                        string latlon = location.Latitude + "," + location.Longitude;
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.Load("https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + latlon + "&key=AIzaSyCPbyemMRyuWmR7yiD-nIwXaaeZaw-jK9o");
                        XmlNodeList xNodelst = xDoc.GetElementsByTagName("result");
                        XmlNode xNode = xNodelst.Item(0);
                        Address = xNode.SelectSingleNode("formatted_address").InnerText;
                    }
                    catch
                    {

                    }
                    
                    EmployeeAttendanceLog eal = new EmployeeAttendanceLog()
                    {
                        Emp_Id = location.EmployeeId,
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        IsClockIn = location.IsClockIn,
                        IsClockOut = location.IsClockOut,
                        Time = MasterDataController.GetIndianTime(location.Time),
                        Address = Address,
                        InsertedTime = MasterDataController.GetIndianTime(DateTime.UtcNow)
                    };
                    db.EmployeeAttendanceLogs.Add(eal);
                    db.SaveChanges();

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetEmployeeLocationData(string EmployeeId,DateTime Date )
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var employeeLocations = db.GetEmployeeLocations(EmployeeId,Date).ToList();
                    return Content(HttpStatusCode.OK, new { employeeLocations });
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Anerror occuured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetAllEmployeesLatestLocation(DateTime currentDate)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                   var EmployeesLatestLocation = db.GetEmployeesLatestLocation(currentDate).ToList();
                    return Content(HttpStatusCode.OK, new { EmployeesLatestLocation });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        public IHttpActionResult GetSchedule()
        {
            string schedule = "2,3,4,5,6,7 9:00-21:00";
            return Content(HttpStatusCode.OK, new { schedule });
        }

        [HttpGet]
        public async Task<IHttpActionResult> PopulateEmptyAddresses()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var emptyAddressLocations = db.EmployeeAttendanceLogs.Where(x => x.Address == null).ToList();
                    foreach (var location in emptyAddressLocations)
                    {
                        try
                        {
                            string latlon = location.Latitude + "," + location.Longitude;
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.Load("https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + latlon + "&key=AIzaSyCPbyemMRyuWmR7yiD-nIwXaaeZaw-jK9o");
                            XmlNodeList xNodelst = xDoc.GetElementsByTagName("result");
                            XmlNode xNode = xNodelst.Item(0);
                            location.Address = xNode.SelectSingleNode("formatted_address").InnerText;
                            db.Entry(location).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        catch
                        {

                        }
                    }
                }
                return Content(HttpStatusCode.OK, "Success");
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetAttendanceReport(string empId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    int totalCount = 0;
                    var attendanceReport = db.EmployeeAttendanceReport(empId, fromDate, toDate).OrderBy(x => x.EmployeeName).ToList();
                    if (attendanceReport.Count > 0)
                    {
                        totalCount = attendanceReport.Count();
                    }
                    return Content(HttpStatusCode.OK, new { attendanceReport, totalCount });
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]  
        public IHttpActionResult GetEmployeesToBeLoggedIN(DateTime date)
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var employees = db.GetEmployeesToBeLogin(date).ToList();
                    return Content(HttpStatusCode.OK, new { employees });
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpPost]
        public IHttpActionResult UpdateEmployeeLeave()
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;

                    LeaveRecord leave = new LeaveRecord();
                    leave.EmpId = form.Get("empId");
                    leave.LeaveDate = Convert.ToDateTime(form.Get("leaveDate"));
                    leave.UpdatedBy = User.Identity.GetUserId();
                    leave.LeaveType = form.Get("leaveType");

                    db.LeaveRecords.Add(leave);
                    db.SaveChanges();
                    return Ok();
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An errorr occured, please try again later");
            }

        }

    }

    public class LocationInfo
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string EmployeeId { get; set; }
        public bool IsClockIn { get; set; }
        public bool IsClockOut { get; set; }
        public DateTime Time { get; set; }
    }
}

//   public IHttpActionResult LogLocationData(List<LocationInfo> data)
// try
//            {
//                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
//                {
//                    var form = HttpContext.Current.Request.Form;
//var LoginUser = User.Identity.GetUserId();

//                    foreach (var location in data)
//                    {

//                        EmployeeAttendanceLog eal = new EmployeeAttendanceLog()
//                        {
//                            Emp_Id = LoginUser,
//                            Latitude = location.Latitude,
//                            Longitude = location.Longitude,
//                            IsClockIn = location.IsClockIn,
//                            IsClockOut = location.IsClockOut,
//                            Time = MasterDataController.GetIndianTime((new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(location.Time)),
//                            InsertedTime = MasterDataController.GetIndianTime(DateTime.UtcNow)
//                        };
//db.EmployeeAttendanceLogs.Add(eal);
//                    }
//                    db.SaveChanges();
//                    return Ok();
//                }
//            }
//            catch (Exception ex)
//            {
//                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
//                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
//            }






