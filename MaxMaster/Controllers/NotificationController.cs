using MaxMaster.Models;
using MaxMaster.ViewModels;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MaxMaster.Controllers
{
    public class NotificationController : ApiController
    {
        public async Task<IHttpActionResult> SaveNotificationToken()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                if (userId != null)
                {
                    using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                    {
                        var employee = db.Employees.Where(x => x.AspNetUserId == userId).FirstOrDefault();
                        if (employee != null)
                        {
                            employee.NotificationToken = HttpContext.Current.Request.Form.Get("Token");
                            db.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occoured, please try again!");
            }
        }

        public IHttpActionResult SendNotificationOnTaskUpdateAsync(List<string> employees, string actionType, string taskId, string Status)
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                  
                    foreach (var employeeId in employees)
                    {
                        var employee = db.Employees.Where(x => x.AspNetUserId == employeeId).FirstOrDefault();
                        if (employee == null || employee.NotificationToken == null)
                        {
                            continue;
                        }
                        WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                        tRequest.Method = "post";
                        tRequest.Headers.Add(string.Format("Authorization: key={0}", "AIzaSyAqm4FRdcn7b-rn2jRfy-2EwTyFUFPzIKQ"));
                        tRequest.ContentType = "application/json";
                        var payload = new
                        {
                            to = employee.NotificationToken,
                            time_to_live = 200,
                            collapse_key = "test_type_b",
                            delay_while_idle = false,
                            notification = new { },
                            data = new
                            {
                                subText = actionType,
                                title = taskId + " - " + actionType,
                                message = taskId + " - " + actionType,
                                bigText = taskId + " - " + actionType,
                                color = "#3F81C5",
                                notificationType = "Navigation",
                                navigateTo = "TaskDetail",
                                navigationData = "{\"TaskId\" :\"" + taskId + "\"}"
                            }
                        };

                        string postbody = JsonConvert.SerializeObject(payload).ToString();
                        Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                        tRequest.ContentLength = byteArray.Length;
                        using (Stream dataStream = tRequest.GetRequestStream())
                        {
                            dataStream.Write(byteArray, 0, byteArray.Length);
                            tRequest.GetResponse();
                            //using (WebResponse tResponse = tRequest.GetResponse())
                            //{
                            //    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                            //    {
                            //        if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            //            {
                            //                string sResponseFromServer = tReader.ReadToEnd();
                            //                var k = sResponseFromServer;
                            //            }
                            //    }
                            //}
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return null;
            }
        }

        [HttpPost]
        public IHttpActionResult SendNotification()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var employees = JsonConvert.DeserializeObject<List<string>>(form.Get("employees"));
                    var title = form.Get("notificationTitle");
                    var message = form.Get("message");

                    foreach (var employeeId in employees)
                    {
                        var employee = db.Employees.Where(x => x.AspNetUserId == employeeId).FirstOrDefault();
                        if (employee == null || employee.NotificationToken == null)
                        {
                            continue;
                        }
                        WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                        tRequest.Method = "post";
                        tRequest.Headers.Add(string.Format("Authorization: key={0}", "AIzaSyAqm4FRdcn7b-rn2jRfy-2EwTyFUFPzIKQ"));
                        tRequest.ContentType = "application/json";
                        var payload = new
                        {
                            to = employee.NotificationToken,
                            time_to_live = 200,
                            collapse_key = "test_type_b",
                            delay_while_idle = false,
                            notification = new { },
                            data = new
                            {
                                subText = title,
                                title = title,
                                message = message,
                                bigText = message,
                                color = "#3F81C5",
                            }
                        };

                        string postbody = JsonConvert.SerializeObject(payload).ToString();
                        Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                        tRequest.ContentLength = byteArray.Length;
                        using (Stream dataStream = tRequest.GetRequestStream())
                        {
                            dataStream.Write(byteArray, 0, byteArray.Length);
                            tRequest.GetResponse();
                            //using (WebResponse tResponse = tRequest.GetResponse())
                            //{
                            //    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                            //    {
                            //        if (dataStreamResponse != null)
                            //           using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            //            {
                            //                string sResponseFromServer = tReader.ReadToEnd();
                            //                var k = sResponseFromServer;
                            //            }
                            //    }
                            //}
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpPost]
        public IHttpActionResult SendAttendanceNotification()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var title = form.Get("notificationTitle");
                    var message = form.Get("message");
                    //get all employees (working)

                    DateTime date = DateTime.Now;
                    var dayToday = " " + date.ToString("d");
                    DayOfWeek day = DateTime.Now.DayOfWeek;
                    dayToday = " " + day.ToString();
                     
                    if ((dayToday != "Sunday"))
                    {
                        var employees = db.GetEmployeesToBeLogin(date).ToList();
                         
                        foreach (var emp   in employees)
                        {
                            var employeeInLeave = db.LeaveRecords.Where(x => x.LeaveDate == DateTime.Now.Date && x.EmpId == emp.AspNetUserId).FirstOrDefault();
                            var employee = db.Employees.Where(x => x.AspNetUserId == emp.AspNetUserId).FirstOrDefault();
                            if (employee == null || employee.NotificationToken == null || employeeInLeave !=null)
                            {
                                continue;
                            }

                            WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                            tRequest.Method = "post";
                            tRequest.Headers.Add(string.Format("Authorization: key={0}", "AIzaSyAqm4FRdcn7b-rn2jRfy-2EwTyFUFPzIKQ"));
                            tRequest.ContentType = "application/json";
                            var payload = new
                            {
                                to = employee.NotificationToken,
                                time_to_live = 200,
                                collapse_key = "test_type_b",
                                delay_while_idle = false,
                                notification = new { },
                                data = new
                                {
                                    subText = "Attendance Reminder",
                                    title = "Attendance Reminder",
                                    message = "Please Clock In to TMS",
                                    bigText = "Please Clock In to TMS, if clocked in or holiday please ignore",
                                    color = "#3F81C5",
                                    notificationType = "Attendance"
                                }
                            };

                            string postbody = JsonConvert.SerializeObject(payload).ToString();
                            Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                            tRequest.ContentLength = byteArray.Length;
                            using (Stream dataStream = tRequest.GetRequestStream())
                            {
                                dataStream.Write(byteArray, 0, byteArray.Length);
                                tRequest.GetResponse();
                            }
                        }
 
                    }
                    
                }
                return Ok();
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
    }
}
