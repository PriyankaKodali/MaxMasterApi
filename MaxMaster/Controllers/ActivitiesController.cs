using MaxMaster.Models;
using MaxMaster.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace MaxMaster.Controllers
{
    public class ActivitiesController : ApiController
    {
        public static string RandomString(int length, int OrgId, int createdBy, bool role)
        {
            Random generator = new Random();
            var r = generator.Next(1000, 10000).ToString();
            gotoRandom:
            using (MaxMasterDbEntities db = new MaxMasterDbEntities())
            {
                if (role)
                {
                    var prefix = createdBy + "-";
                    var taskId = prefix + r;
                    var exists = db.Activities.Where(x => x.TaskId == taskId).FirstOrDefault();
                    if (exists != null)
                    {
                        goto gotoRandom;
                    }
                    return taskId;
                }
                else
                {
                    var prefix = db.Organisations.Where(x => x.Id == OrgId).FirstOrDefault().EmpPrefix;
                    var taskId = prefix + r;
                    var exists = db.Activities.Where(x => x.TaskId == taskId).FirstOrDefault();
                    if (exists != null)
                    {
                        goto gotoRandom;
                    }
                    return taskId;
                }
             }
        }

        #region "Add Activity "
        [HttpPost]
        public IHttpActionResult AddActivity()
        {
            var form = HttpContext.Current.Request.Form;
            var clientId = form.Get("clientId");
            var departmentId = form.Get("departmentId");
            int orgId = Convert.ToInt32(form.Get("OrgId"));
            var categoryId = form.Get("categoryId");
            
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var u = User.Identity.GetUserId();
                //    var user = db.Employees.Where(x => x.AspNetUserId == u).FirstOrDefault();
                    Activity activity = new Activity();

                    var role = User.IsInRole("Client");
                    int createdby =0;
                  
                  if (role)
                    {
                        var user = db.Clients.Where(x => x.AspNetUserId == u && x.Active== true).FirstOrDefault();
                        createdby = user.Id;
                       
                        var clientOrg = user.OrgId;

                        var emp = db.Employees.Where(x => x.Role_Id == "8").FirstOrDefault();
                        if (emp != null)
                        {
                            activity.AssignedTo = emp.AspNetUserId;
                            activity.TaskOwner = emp.AspNetUserId;
                        }
                        activity.CreatedBy = User.Identity.GetUserId();
                        activity.TaskType = "Client";
                        activity.Client_Id = user.AspNetUserId;
                        activity.Project_Id = Convert.ToInt32(form.Get("projectId"));
                        activity.EDOC = DateTime.Now.AddDays(2);
                    }
                    else
                    {
                        var user = db.Employees.Where(x => x.AspNetUserId == u).FirstOrDefault();
                        activity.CreatedBy = user.AspNetUserId;
                        createdby = Convert.ToInt32( user.Id);

                        if (clientId != null)
                        {
                            activity.Client_Id = (clientId);
                            activity.Project_Id = Convert.ToInt32(form.Get("projectId"));
                        }

                        if (departmentId != null)
                        {
                            activity.Department_Id = Convert.ToInt32(departmentId);
                        }

                        activity.TaskOwner = (form.Get("assignedTo"));
                        var ed = (form.Get("edoc"));
                        activity.EDOC = DateTime.Parse(ed);
                        activity.TaskType = form.Get("taskType");
                        activity.AssignedTo =(form.Get("assignedTo"));
                    }

                    activity.Category_Id = Convert.ToInt32(form.Get("categoryId"));
                    activity.SubCategory_Id = Convert.ToInt32(form.Get("subCategoryId"));
                    activity.CreatedDate =MasterDataController.GetIndianTime(DateTime.UtcNow);
                    activity.Subject = form.Get("task");
                    activity.Priority = Convert.ToInt32(form.Get("priority"));
                    activity.Status = "Open";
                    activity.Description = form.Get("Description");
                    activity.TaskId = RandomString(4, orgId, createdby, role);

                    db.Activities.Add(activity);

                    ActivitiesLog activityLog = new ActivitiesLog();

                    activityLog.Status = activity.Status;
                    activityLog.Description = activity.Description;
                    activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.UtcNow);
                    activityLog.AssignedBy = activity.CreatedBy;
                    activityLog.AssingedTo = activity.TaskOwner;

                    activity.ActivitiesLogs.Add(activityLog);

                    var files = HttpContext.Current.Request.Files;

                    var fileAttachments = new List<HttpPostedFile>();

                   for(int i=0; i<files.Count; i++)
                    {
                        fileAttachments.Add(files[i]);
                    }

                   foreach(var file in fileAttachments)
                    {
                        var fileDirecory = HttpContext.Current.Server.MapPath("~/Task");

                        if (!Directory.Exists(fileDirecory))
                        {
                            Directory.CreateDirectory(fileDirecory);
                        }

                        var fileName =  file.FileName;
                        var filePath = Path.Combine(fileDirecory, fileName);
                        file.SaveAs(filePath);

                        ActivityAttachment actAttachments = new ActivityAttachment();

                        actAttachments.AttachmentURL = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "Task", fileName);
                        actAttachments.Name = Path.GetFileNameWithoutExtension(file.FileName);
                        activityLog.ActivityAttachments.Add(actAttachments);
                    }

                   var taskId = activity.TaskId;
                  
                    var from = "";
                    var to = "";
                    string name = "";

                    if (role)
                    {
                        var client = db.Clients.Where(x => x.AspNetUserId == u && x.Active == true).FirstOrDefault();
                        to = client.Email;
                        name = client.Name;
                    }
                    else
                    {
                        var empl = db.Employees.Where(x => x.AspNetUserId == activity.AssignedTo && x.Active == true).FirstOrDefault();
                        to = empl.Email;
                        name = empl.FirstName + " " + empl.LastName;
                    }
                    string subject = "You have created a new ticket with ticket Id #" + taskId ;
                    if(role)
                    {
                        var body = @"<div style=""border: 1px solid gray;margin-top:2px; margin-right: 2px;padding: 9px;"" > 
                                     <p> Dear " + name + ", </p>" +
                                    "<p> Greetings! Thank you for contacting our support team. We received your request in our system and we have started working on priority.</p>" +
                                    "<p> We will approach you soon if we require any further details to probe and expedite on the issue. Please allow us some time to give you an update on the progress on resolution.Your patience till then is much appreciated.</p> " +
                                    "<p> We encourage you to kindly use our support portal http://ourclientSupport.azurewebsites.net for reporting issues/Service Requests with support team or to seek any further updates on your ticket. </p>" +
                                    "<p> If you are experiencing any delays or for any further assistance you are feel free to contact on below numbers:</p>" +
                                    "<p> CUSTOMER SERVICE: 9 66 67 24 365 </p>" +
                                       " </div>";
                        bool sendmail = new EmailController().SendEmail(from, "Task Management System", to, subject, body);
                    }
                    db.SaveChanges();
                }
                return Ok();
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region "Edit Activity "
        [HttpPost]
        public IHttpActionResult EditActivity()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var taskId = (form.Get("taskId"));
                    var userId = User.Identity.GetUserId();
                    var hoursWorked = form.Get("hoursWorked");

                    bool IsClient = false ;
                    bool IsEmployee = false;

                //    var user = db.Employees.Where(x => x.AspNetUserId == userId).Select(x => x.AspNetUserId).FirstOrDefault();
           
                    Activity activity = db.Activities.Find(taskId);
                    ActivitiesLog activityLog = new ActivitiesLog();


                    string creator = activity.CreatedBy;
                    var client = db.Clients.Where(x => x.AspNetUserId == creator).FirstOrDefault();
                    var employee = db.Employees.Where(x => x.AspNetUserId == creator).FirstOrDefault();
                    if(client!= null)
                    {
                      IsClient = true;
                    }
                    if(employee!=  null)
                    {
                        IsEmployee = true;
                    }

                    var actionType = form.Get("actionType");
                    activityLog.TaskId = activity.TaskId;
                    activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                    activityLog.Description = form.Get("description");
                  
                    if (actionType == "Assign")
                    {
                        activity.TaskOwner =(form.Get("assignee"));
                        activity.Status = "Open";
                        
                        activityLog.AssignedBy = User.Identity.GetUserId();
                        activityLog.AssingedTo = (form.Get("assignee"));
                
                        if (hoursWorked != "" && hoursWorked!="NaN")
                        {
                            activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                        }
                      
                        activityLog.Status = "Open";

                        db.ActivitiesLogs.Add(activityLog);

                    }

                    if (actionType == "Pending")
                    {

                        var ac = (db.ActivitiesLogs.Where(x => x.TaskId == taskId).OrderByDescending(x => x.Id).FirstOrDefault());

                        activity.Status = "Pending";

                        activityLog.AssignedBy = User.Identity.GetUserId();
                        activityLog.AssingedTo = User.Identity.GetUserId();
                        activityLog.Status = "Pending";
                        activityLog.StartDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edos")));
                        activityLog.EndDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edoc")));
                    
                        activityLog.BudgetedHours = Convert.ToInt32(form.Get("budgetedHours"));
                        if(hoursWorked!="" && hoursWorked !="NaN")
                        //System.Single.IsNaN(Convert.ToInt32(form.Get("hoursWorked")))
                        {
                            activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                        }
                        
                            if(IsClient)
                            {
                                string subject = "An update on your ticket with Ticket Id #" + taskId + " ( " + activity.Subject + " )";
                                string from = "";
                                string to = client.Email;
                                var body = @"<div style=""border: 1px solid gray;margin-top:2px; margin-right: 2px;padding: 9px;"" > 
                                     <p> Dear " + client.Name + ", </p>" +
                                            "<p> An update related to TicketId #" + activity.TaskId + " ( " + activity.Subject + " ) is available.  " + "</p>" +
                                            "<p> We encourage you to kindly use our support portal http://ourclientSupport.azurewebsites.net for reporting issues/Service Requests with support team or to seek any further updates on your ticket. </p>" +
                                            "<p> CUSTOMER SERVICE: 9 66 67 24 635 </p>" +
                                               " </div>";
                                bool sendEmail = new EmailController().SendEmail(from, "Task Management System", to, subject, body);
                            }
                  
                        db.ActivitiesLogs.Add(activityLog);
                        db.Entry(activity).State = System.Data.Entity.EntityState.Modified;
                    }

                    if (actionType == "Resolved")
                    {
                        activity.TaskOwner = activity.CreatedBy;
                        activity.Status = "Resolved";

                        activityLog.AssignedBy = User.Identity.GetUserId();
                        activityLog.AssingedTo = activity.CreatedBy;
                        activityLog.Status = "Resolved";
                        activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));

                        activity.ActivitiesLogs.Add(activityLog);
                        db.Entry(activity).State = System.Data.Entity.EntityState.Modified;
                        
                        var user = activity.CreatedBy;
                      
                            if(IsClient)
                            {
                                string subject = "Your ticket with ticket Id #" + taskId + " ( " + activity.Subject +  " )  is resolved ";
                                string from = "";
                                string to = client.Email;
                                    var body = @"<div style=""border: 1px solid gray;margin-top:2px; margin-right: 2px;padding: 9px;"" > 
                                     <p> Dear " + client.Name + ", </p>" +
                                                "<p> Your issue related to TicketId " +activity.TaskId + " ( " + activity.Subject + " ) has been resolved. "  +"</p>" +
                                                "<p> We encourage you to kindly use our support portal http://ourclientSupport.azurewebsites.net for reporting issues/Service Requests with support team or to seek any further updates on your ticket. </p>" +
                                                "<p> CUSTOMER SERVICE: 9 66 67 24 635 </p>" +
                                                   " </div>";
                                bool sendEmail = new EmailController().SendEmail(from, "Task Management System", to, subject, body);
                             }
                    }

                    if (actionType == "AcceptToClose")
                    {
                        bool isClient = false;
                        activity.Status = "Closed";
                        activity.CompletedDate = MasterDataController.GetIndianTime(DateTime.Now);

                        activityLog.AssignedBy = activity.TaskOwner;
                        activityLog.AssingedTo = activity.CreatedBy;
                        activityLog.Status = "Closed by assignee";
                        activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                        
                            if (client != null)
                            {
                                string subject = "Your ticket with ticket Id #" + taskId + " ( " + activity.Subject + " )  is Closed ";
                                string from = "";
                                string to = client.Email;
                                var body = @"<div style=""border: 1px solid gray;margin-top:2px; margin-right: 2px;padding: 9px;"" > 
                                     <p> Dear " + client.Name + ", </p>" +
                                            "<p> Your issue related to TicketId #" + activity.TaskId + " ( " + activity.Subject + " )  is closed. If  " + "</p>" +
                                            "<p> We encourage you to kindly use our support portal http://ourclientSupport.azurewebsites.net for reporting issues/Service Requests with support team or to seek any further updates on your ticket. </p>" +
                                            "<p> CUSTOMER SERVICE: 9 66 67 24 635 </p>" +
                                               " </div>";
                                bool sendEmail = new EmailController().SendEmail(from, "Task Management System", to, subject, body);
                            }
                   

                        db.ActivitiesLogs.Add(activityLog);
                    }

                    if (actionType == "Reopen")
                    {
                        var lastWorked = db.ActivitiesLogs.Where(x => x.TaskId == taskId).OrderByDescending(x => x.TaskDate).FirstOrDefault();

                        activity.Status = "Reopened";
                        activity.TaskOwner = lastWorked.AssignedBy;
                        activity.CompletedDate = null;
                        activityLog.AssingedTo = lastWorked.AssignedBy;
                        activityLog.AssignedBy = User.Identity.GetUserId();
                        activityLog.Status = "Reopened";

                        //  activityLog.TaskId = activity.TaskId;
                        //  activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                        //  activityLog.Description = form.Get("description");

                        db.ActivitiesLogs.Add(activityLog);
                    }


                    var files = HttpContext.Current.Request.Files;
                    var fileAttachments = new List<HttpPostedFile>();
                    if (files.Count > 0)
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            fileAttachments.Add(files[i]);
                        }

                        foreach (var file in fileAttachments)
                        {
                            var fileDirecory = HttpContext.Current.Server.MapPath("~/Task");

                            if (!Directory.Exists(fileDirecory))
                            {
                                Directory.CreateDirectory(fileDirecory);
                            }

                            var fileName = file.FileName;
                            var filePath = Path.Combine(fileDirecory, fileName);
                            file.SaveAs(filePath);

                            ActivityAttachment actAttachments = new ActivityAttachment();

                            actAttachments.AttachmentURL = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "Task", fileName);
                            actAttachments.Name = Path.GetFileNameWithoutExtension(file.FileName);
                            activityLog.ActivityAttachments.Add(actAttachments);
                        }
                    }
                    db.Entry(activity).State = System.Data.Entity.EntityState.Modified;
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
        #endregion

        #region "Add Image "
        [HttpPost]
        public IHttpActionResult UploadImage()
        {
            try
            {
                var file = HttpContext.Current.Request.Files[0];
                if (file == null)
                {
                    return Content(HttpStatusCode.InternalServerError, "An Error occoured!");
                }

                string fileExtension = Path.GetExtension(file.FileName);
                string[] validExtensions = new string[] { ".jpg", ".jpeg", ".png", ".gif" };
                if (!validExtensions.Contains(fileExtension))
                {
                    return Content(HttpStatusCode.InternalServerError, "Invalid file type!!");
                }

                string emailImagesFolder = HttpContext.Current.Server.MapPath("~/Images/EditorImages");
                if (!Directory.Exists(emailImagesFolder))
                {
                    Directory.CreateDirectory(emailImagesFolder);
                }

                string fileName = DateTime.Now.Ticks.ToString() + fileExtension;
                string filePath = Path.Combine(emailImagesFolder, fileName);
                file.SaveAs(filePath);
                string fileUrl = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "Images", "EditorImages", fileName);

                return Content(HttpStatusCode.OK, new { link = fileUrl });
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occoured, please try again!");
            }
        }
        #endregion

        #region "Get Tasks on me(To do list)"
        [HttpGet]
        public IHttpActionResult GetMyTasks(string EmpId, string clientId, int? departmentId,string taskType, string priority, string status, int? page, int? count, string sortCol, string sortDir)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    int totalPages = 0;
                   
                    // stored procedure to get tasks on employee
                    var myTasks = db.GetMyTasks(EmpId, clientId, departmentId, taskType, priority, status, page, count, sortCol, sortDir).ToList();
                    var summaryReport = db.TaskStatsGet(User.Identity.GetUserId()).FirstOrDefault();

                    ActivitiesSummary actSummary = new ActivitiesSummary();
                    actSummary.PriorityHighJobs = summaryReport.HighMy;
                    actSummary.PriorityMediumJobs = summaryReport.MediumMy;
                    actSummary.PriorityLowJobs = summaryReport.LowMy;
                    actSummary.TotalJobs = actSummary.PriorityHighJobs + actSummary.PriorityMediumJobs + actSummary.PriorityLowJobs;

                    if(myTasks.Count()>0)
                    {
                        totalPages = (int)myTasks.FirstOrDefault().TotalCount;
                    }
                 
                    //if (myTasks.Count>0)
                    //{
                    //   

                    //    for (int i = 0; i < myTasks.Count; i++)
                    //    {
                    //        if (myTasks[i].Priority == 0)
                    //        {
                    //            PriorityHigh++;
                    //        }
                    //        else if (myTasks[i].Priority == 1)
                    //        {
                    //            PriorityMedium++;
                    //        }
                    //        else
                    //        {
                    //            PriorityLow++;
                    //        }
                    //    }
                    //   }

                    return Content(HttpStatusCode.OK, new { myTasks, totalPages, actSummary });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Invalid request");
            }
        }
        #endregion

        #region "Get Tasks Created By Me"
        [HttpGet]
        public IHttpActionResult GetTasksByMe (string EmpId, string clientId, int? departmentId, string taskType, string priority, string status, int? page, int? count, string sortCol, string sortDir)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    int? totalPages = 0;
                   
                    
                    // Stored procedure to get tasks assigned by the employee
                    var tasksByMe = db.GetTasksByMe(EmpId, clientId, departmentId, taskType, priority, status, page, count, sortCol, sortDir).ToList();
                    var summaryReport = db.TaskStatsGet(User.Identity.GetUserId()).FirstOrDefault();

                    if(tasksByMe.Count()>0)
                    {
                        totalPages = (int)(tasksByMe).FirstOrDefault().TotalCount;
                    }
                   
                    ActivitiesSummary actSummary = new ActivitiesSummary();

                    actSummary.PriorityHighJobs = summaryReport.HighByMe;
                    actSummary.PriorityMediumJobs = summaryReport.MediumByMe;
                    actSummary.PriorityLowJobs = summaryReport.LowByMe;
                    actSummary.TotalJobs = actSummary.PriorityHighJobs + actSummary.PriorityMediumJobs + actSummary.PriorityLowJobs;

                    return Content(HttpStatusCode.OK, new { tasksByMe, totalPages, actSummary });
                    
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Invalid Request");
            }
        }
        #endregion

        #region "Get Task Through Me"
        [HttpGet]
        public IHttpActionResult GetTasksThroughMe(string EmpId, string clientId, int? departmentId, string taskType, string priority, string status, int? page, int? count, string sortCol, string sortDir)
        {
            try
            {
                using (MaxMasterDbEntities db= new MaxMasterDbEntities())
                {
                    int? totalCount = 0;
                   
                    // stored procedure to get tasks through employee
                    var tasksThroughMe = db.GetTasksThroughMe(EmpId, clientId, departmentId, taskType, priority, status, page, count, sortCol, sortDir).ToList();
                    var summaryReport = db.TaskStatsGet(User.Identity.GetUserId()).FirstOrDefault();

                    if(tasksThroughMe.Count>0)
                    {
                        totalCount = (int)tasksThroughMe.FirstOrDefault().TotalCount;
                    }

                    ActivitiesSummary actSummary = new ActivitiesSummary();

                    actSummary.PriorityHighJobs = summaryReport.HighThroughMe;
                    actSummary.PriorityMediumJobs = summaryReport.MediumThroughMe;
                    actSummary.PriorityLowJobs = summaryReport.LowThroughMe;
                    actSummary.TotalJobs = tasksThroughMe.Count;

                    return Content(HttpStatusCode.OK, new { tasksThroughMe, totalCount, actSummary });
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Invalid Request");
            }
        }
        #endregion

        #region "Get Task basic information"
        [HttpGet]
        public IHttpActionResult GetTaskBasicInfo(string TaskId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var taskDetails = db.GetTaskBasicInfo(TaskId).FirstOrDefault();
                    ActivititesModel activity = new ActivititesModel();
                    
                    activity.Subject = taskDetails.Subject;
                    activity.Description = taskDetails.Description;
                    activity.TaskDate = taskDetails.CreatedDate;
                    activity.CreatedDate = taskDetails.CreatedDate;
                    activity.Status = taskDetails.Status;
                    activity.CreatedBy = taskDetails.CreatedByName;
                    activity.TaskOwner = taskDetails.TaskOwner;
                    activity.TaskType = taskDetails.TaskType;
                   // activity.TaskId = taskDetails.TaskId;
                    if (taskDetails.Priority == 0)
                    {
                        activity.Priority = "High";
                    }
                    else if(taskDetails.Priority == 1)
                    {
                        activity.Priority = "Medium";
                    }
                    else
                    {
                        activity.Priority = "Low";
                    }
                    activity.Creator = taskDetails.CreatedBy;

                    if(activity.TaskType == "Client")
                    {
                        var projectId = db.Activities.Where(x => x.TaskId == taskDetails.TaskId).Select(x => x.Project_Id).FirstOrDefault();
                        var locationId = db.Opportunities.Where(x => x.Id == projectId).Select(x => x.Location_Id).FirstOrDefault();
                        var location = db.ClientLocations.Where(x => x.Id == locationId).Select(x => x.AddressLine1 + " ," + x.AddressLine2 + x.City.Name + " ," + x.State.Name + " ," + x.Country.Name + " ," + x.ZIP).FirstOrDefault();
                        activity.Location = location;
                    }
                   
                    return Content(HttpStatusCode.OK, new { activity });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion
           
        #region "Get task log "
        public IHttpActionResult GetTaskLog(string taskId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var activityLog = db.GetTaskLog(taskId).ToList();

                    List<ActivityLog> taskLog = new List<ActivityLog>();

                    for(int i=0; i<activityLog.Count; i++)
                    {
                        ActivityLog al = new ActivityLog();

                     //   List<TaskAttachments> at = new List<TaskAttachments>();

                        //string noHTML = Regex.Replace(activityLog[i].Description, @"<[^>]+>|&nbsp;", "").Trim();
                        //string noHTMLNormalised = Regex.Replace(noHTML, @"\s{2,}", " ");
                        //al.Description = Regex.Replace(noHTMLNormalised, "<[^>]*>", string.Empty).Trim();

                        al.Description = activityLog[i].Description;
                        al.AssignedBy = activityLog[i].AssignedBy;
                        al.AssignedTo = activityLog[i].AssignedTo;
                        al.Id = activityLog[i].ActivityLogId;

                        var atchmts = db.ActivityAttachments.Where(x => x.ActivityId == al.Id).ToList();
                        al.Attachments = atchmts.Select(x => x.AttachmentURL).ToArray();
                       
                        al.Status = activityLog[i].Status;
                        al.TaskDate = activityLog[i].TaskDate;
                        al.HoursWorked = activityLog[i].HoursWorked;

                        taskLog.Add(al);
                    }

                    return Content(HttpStatusCode.OK, new { taskLog });
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region "Get Task total hours worked for the task "
        [HttpGet]
        public IHttpActionResult GetTaskHoursWorkedInfo(string taskId,string userId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var user = User.Identity.GetUserId();
                    ActivityLog activitylog = new ActivityLog();

                    var activitylogInfo = db.ActivitiesLogs.Where(x => x.TaskId == taskId &&  ((x.AssignedBy == userId && x.Status!="Pending") || (x.AssingedTo== userId && x.Status=="Pending"))).ToList();

                    if (activitylogInfo.Count > 0)
                    {
                        activitylog.TaskId = activitylogInfo[0].TaskId;
                        activitylog.TaskDate = activitylogInfo[0].TaskDate;
                        activitylog.StartDate = activitylogInfo[0].StartDate;
                        activitylog.EndDate = activitylogInfo[0].EndDate;
                        activitylog.TotalHoursWorked = activitylogInfo.Sum(x => x.HoursWorked);
                        activitylog.BudgetedHours = activitylogInfo[0].BudgetedHours;
                    }
                    
                    return Content(HttpStatusCode.OK, new { activitylog });

                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region  "Tasks summary report"
         [HttpGet]
         [Authorize]
        public IHttpActionResult GetActivitiesSummary()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var EmpUserId = User.Identity.GetUserId();
                    var EmpId = db.Employees.Where(x => x.AspNetUserId == EmpUserId).FirstOrDefault().Id;
                   var summaryReport = db.TaskStatsGet(User.Identity.GetUserId()).FirstOrDefault();

                    ActivitiesSummaryReport activitiesSummary = new ActivitiesSummaryReport();

                    activitiesSummary.HighToDo = (int)summaryReport.HighMy;
                    activitiesSummary.MediumToDo = (int)summaryReport.MediumMy;
                    activitiesSummary.LowToDo = (int)summaryReport.LowMy;
                    activitiesSummary.TotalToDo = (int)(summaryReport.HighMy + summaryReport.MediumMy + summaryReport.LowMy);

                    activitiesSummary.HighByMe = (int)summaryReport.HighByMe;
                    activitiesSummary.MediumByMe = (int)summaryReport.MediumByMe;
                    activitiesSummary.LowByMe = (int)summaryReport.LowByMe;
                    activitiesSummary.TotalByMe = (int)(summaryReport.HighByMe + summaryReport.MediumByMe + summaryReport.LowByMe);

                    activitiesSummary.HighThroughMe = (int)summaryReport.HighThroughMe;
                    activitiesSummary.MediumThroughMe = (int)summaryReport.MediumThroughMe;
                    activitiesSummary.LowThroughMe = (int)summaryReport.LowThroughMe;
                    activitiesSummary.TotalThroughMe = (int)(summaryReport.HighThroughMe + summaryReport.MediumThroughMe + summaryReport.LowThroughMe);

                    return Content(HttpStatusCode.OK, new { activitiesSummary });
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.OK, "An error occured, please try again later");
            }
        }

        #endregion

        #region "List of Employee/(s) Activities "
        public IHttpActionResult GetEmployeesReport(string empId, DateTime fromDate, string toDate, string clientId, int? priority, string status)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var employeesList = db.GetEmployeesTasksCount(empId, fromDate, toDate, clientId).OrderBy(x=>x.Employee).ToList();
                    var employeesTaskList = db.GetEmployeesActivitiesReport(empId, fromDate, toDate, clientId, priority, status).OrderByDescending(x=>x.CreatedDate).ToList();

                    int TotalCount = employeesList.Count();

                    List<EmployeeTaskReport> empReport = new List<EmployeeTaskReport>();
                   
                    for(int i=0; i<employeesTaskList.Count; i++)
                    {
                        EmployeeTaskReport eReport = new EmployeeTaskReport();

                        eReport.CreatedDate = employeesTaskList[i].CreatedDate;
                        eReport.CreatedBy = employeesTaskList[i].AssignedBy;
                        eReport.Subject = employeesTaskList[i].Subject;
                        eReport.Status = employeesTaskList[i].Status;
                        eReport.TaskOwner = employeesTaskList[i].TaskOwner;
                        eReport.EDOC = employeesTaskList[i].EDOC;
                        eReport.CompletedDate = employeesTaskList[i].CompletedDate;
                        eReport.TaskId = employeesTaskList[i].TaskId;
                        eReport.TaskOwnerId = employeesTaskList[i].TaskOwnerId;
                        eReport.AssignedToId = employeesTaskList[i].AssignedToId;
                        eReport.Department = employeesTaskList[i].Department;
                        eReport.ClientName = employeesTaskList[i].ClientName;
                 
                        var taskLog = db.ActivitiesLogs.Where(x => x.TaskId == eReport.TaskId && ((x.AssignedBy == eReport.AssignedToId && x.Status == "Pending") || (x.AssignedBy == eReport.AssignedToId))).ToList();
                        var hw = taskLog.Sum(x => x.HoursWorked);

                        eReport.HoursWorked = hw;
                        eReport.Priority = employeesTaskList[i].Priority;

                        var taskResolved=  db.ActivitiesLogs.Where(x => x.TaskId == eReport.TaskId && x.Status == "Resolved" && eReport.Status!="Reopened").OrderByDescending(x=>x.TaskDate).FirstOrDefault();
                        if(taskResolved!=null)
                        {
                            eReport.TaskResolvedDate = (taskResolved.TaskDate);
                        }
                 
                        empReport.Add(eReport);
                   
                    }
                    return Content(HttpStatusCode.OK, new { employeesList,  empReport , TotalCount });
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occureed, please try again later");
            }
        }
        #endregion

        #region "Get employee over all report "

        public IHttpActionResult GetIndividualEmpReport(string empId, DateTime? fromDate, string toDate, string clientId, string status, int? priority, string taskId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var EmployeeActivitiesReport = db.EmployeeActivitiesLogReport(empId, fromDate, toDate, clientId, status, priority, taskId).OrderBy(x=>x.TaskDate).OrderByDescending(x=>x.CreatedDate).ToList();

                    var taskList = EmployeeActivitiesReport.GroupBy(x => x.TaskId).Select(x => x.First()).ToList();

                    List<EmployeeTaskReport> empActivitiesReport = new List<EmployeeTaskReport>();

                    foreach (var task in taskList)
                    {
                        EmployeeTaskReport activityLog = new EmployeeTaskReport();

                        activityLog.CreatedBy = task.CreatedBy;
                        activityLog.CreatedDate = task.CreatedDate;
                        activityLog.Subject = task.Subject;
                        activityLog.TaskId = task.TaskId;
                        activityLog.Department = task.Department;
                        activityLog.ClientName = task.ClientName;
                        activityLog.TaskType = task.TaskType;

                        if(task.TaskType == "Client")
                        {
                            var projectId = db.Activities.Where(x => x.TaskId == task.TaskId).Select(x => x.Project_Id).FirstOrDefault();
                            var locationId = db.Opportunities.Where(x => x.Id == projectId).Select(x => x.Location_Id).FirstOrDefault();
                            var location = db.ClientLocations.Where(x => x.Id == locationId).Select(x => x.AddressLine1 + " ," + x.AddressLine2 + x.City.Name + " ," + x.State.Name + " ," + x.Country.Name + " ," + x.ZIP).FirstOrDefault();
                            activityLog.Location = location;
                        }

                        // activityLog.TaskOwner = task.TaskOwner;
                        //string noHTML = Regex.Replace(task.Description, @"<[^>]+>|&nbsp;", "").Trim();
                        //string noHTMLNormalised = Regex.Replace(noHTML, @"\s{2,}", " ");
                        //task.Description = Regex.Replace(noHTMLNormalised, "<[^>]*>", string.Empty).Trim();

                        activityLog.TaskDescription = task.Description;

                        var taskLog = EmployeeActivitiesReport.Where(x => x.TaskId == activityLog.TaskId).ToList();

                           for (int i=0; i< taskLog.Count(); i++)
                           {

                            if(taskLog[i].StartDate!= null)
                              {
                                activityLog.StatDate = Convert.ToDateTime(taskLog[i].StartDate);
                                activityLog.ExpectedClosureDate = Convert.ToDateTime(taskLog[i].EndDate);
                                activityLog.BudgetedHours = taskLog[i].BudgetedHours;
                               }
                            activityLog.ActualEndDate = taskLog[i].CompletedDate;
                        }

                        var TaskLog = db.ActivitiesLogs.Where(x => x.TaskId == activityLog.TaskId && x.HoursWorked != null).ToList();

                        if (TaskLog.Count > 0)
                        {
                            activityLog.ActualStartDate = TaskLog.FirstOrDefault().TaskDate;

                            int? k = 0;
                            for (int i = 0; i < TaskLog.Count; i++)
                            {
                                k += TaskLog[i].HoursWorked;
                                activityLog.HoursWorked = k;
                            }
                        }

                        activityLog.ActivityLog = EmployeeActivitiesReport.Where(x=>x.TaskId == activityLog.TaskId).Select(x => new ActivityLog
                        {
                            Id = x.activityLogId,
                            TaskId = x.TaskId,
                            TaskCreatedDate = x.TaskDate,
                            Status= x.LogStatus,
                            AssignedBy= x.AssignedBy,
                            AssignedTo= x.AssignedTo,
                            Description = x.Description,
                            HoursWorked = x.HoursWorked

                        }).OrderBy(x => x.TaskDate).ToList();
                        
                        empActivitiesReport.Add(activityLog);
                    }

                    return Content(HttpStatusCode.OK, new { empActivitiesReport });
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

    }
}



//var taskId = activity.TaskId;
//string priority;
//if(activity.Priority == 0)
//{
//    priority = "High";
//}
//else if(activity.Priority == 1)
//{
//    priority = "Medium";
//}
//else
//{
//    priority = "Low";
//}

//var toEmp = db.Employees.Where(x => x.Id == activity.TaskOwner).FirstOrDefault();

//string subject = "A new Ticket " + taskId + " is created by " +  user.FirstName + " " + user.LastName + " to you";

//var html = @"<div> 
//            <div> 
//               Below are some your newly created task details of ticket Id :" + taskId +
//               @" <p> Subject : " + activity.Subject  +
//               @" </p> <p> Priority : " + priority +
//               @" </p> <p> Expected Date of closure : " + activity.EDOC.ToShortDateString() + @" </p> </div>
//               <div>
//        </div> </div>
//           Log on to <a href=""http://maxtms.azurewebsites.net"" > maxtms.azurewesites.net</a> for more details</div> </div>
//     </div>";

//bool mailToOwner= SendEmail("", "", toEmp.Email, subject, html);

//if (mailToOwner)
//{
//    var createdby = db.Employees.Where(x => x.Id == activity.CreatedBy).FirstOrDefault();
//    string TicketSubject = "Ticket Creation was successfull to " + toEmp.FirstName + " " + toEmp.LastName + " by you";

//    string TicketBody = html;
//    SendEmail("", "", createdby.Email, TicketSubject, TicketBody);
//}
