using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

using MaxMaster.Models;
using MaxMaster.ViewModels;
using Microsoft.AspNet.Identity;

namespace MaxMaster.Controllers
{
    public class ClientServiceController : ApiController
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
        [HttpPost]
        public IHttpActionResult AddClientRequest()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {

                    var form = HttpContext.Current.Request.Form;
                    var clientId = form.Get("clientId");
                    var departmentId = form.Get("departmentId");
                    int orgId = Convert.ToInt32(form.Get("OrgId"));
                    var categoryId = form.Get("categoryId");
                    var u = User.Identity.GetUserId();

                    Activity activity = new Activity();

                    var user = db.Clients.Where(x => x.AspNetUserId == u && x.Active == true).FirstOrDefault();
                    var createdby = user.Id;

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

                    activity.TaskId = RandomString(4, orgId, createdby, true);
                    db.Activities.Add(activity);

                    ActivitiesLog activityLog = new ActivitiesLog();

                    activityLog.Status = activity.Status;
                    activityLog.Description = activity.Description;
                    activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.UtcNow);
                    activityLog.AssignedBy = activity.CreatedBy;
                    activityLog.AssignedTo = activity.TaskOwner;

                    activity.ActivitiesLogs.Add(activityLog);

                    var files = HttpContext.Current.Request.Files;

                    var fileAttachments = new List<HttpPostedFile>();

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


                    var from = "";
                    var to = "";
                    string name = "";
                    var taskId = activity.TaskId;

                    var client = db.Clients.Where(x => x.AspNetUserId == activity.CreatedBy && x.Active == true).FirstOrDefault();
                    to = client.Email;
                    name = client.Name;


                    var empl = db.Employees.Where(x => x.AspNetUserId == activity.AssignedTo && x.Active == true).FirstOrDefault();
                    to = empl.Email;
                    name = empl.FirstName + " " + empl.LastName;

                    string subject = "You have created a new ticket with ticket Id #" + taskId;

                    var body = @"<div style=""border: 1px solid gray;margin-top:2px; margin-right: 2px;padding: 9px;"" > 
                                     <p> Dear " + name + ", </p>" +
                                "<p> Greetings! Thank you for contacting our support team. We received your request in our system and we have started working on priority.</p>" +
                                "<p> We will approach you soon if we require any further details to probe and expedite on the issue. Please allow us some time to give you an update on the progress on resolution.Your patience till then is much appreciated.</p> " +
                                "<p> We encourage you to kindly use our support portal http://ourclientSupport.azurewebsites.net for reporting issues/Service Requests with support team or to seek any further updates on your ticket. </p>" +
                                "<p> If you are experiencing any delays or for any further assistance you are feel free to contact on below numbers:</p>" +
                                "<p> CUSTOMER SERVICE: 9 66 67 24 365 </p>" +
                                   " </div>";
                    bool sendmail = new EmailController().SendEmail(from, "Task Management System", to, subject, body);

                    db.SaveChanges();
                }
                return Ok();

            }
            catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError, "An error occured please try again later");
            }
        }

        //[HttpPost]
        //public IHttpActionResult EditClientRequest()
        //{
        //    try
        //    {
        //        using (MaxMasterDbEntities db = new MaxMasterDbEntities())
        //        {
        //            var form = HttpContext.Current.Request.Form;
        //            var taskId = (form.Get("taskId"));
        //            var userId = User.Identity.GetUserId();
        //            var hoursWorked = form.Get("hoursWorked");

        //          //  var quantityWorked = form.Get("quantityWorked");

        //            Activity activity = db.Activities.Find(taskId);
        //            ActivitiesLog activityLog = new ActivitiesLog();
                    
        //            string creator = activity.CreatedBy;
        //            var client = db.Clients.Where(x => x.AspNetUserId == creator).FirstOrDefault();
                  
        //            var actionType = form.Get("actionType");
        //            activityLog.TaskId = activity.TaskId;
        //            activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
        //            activityLog.Description = form.Get("description");

        //            if (actionType == "Assign")
        //            {
        //                activity.TaskOwner = (form.Get("assignee"));
        //                activity.Status = "Open";

        //                activityLog.AssignedBy = User.Identity.GetUserId();
        //                activityLog.AssingedTo = (form.Get("assignee"));

        //                if (hoursWorked != "" && hoursWorked != "NaN")
        //                {
        //                    activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
        //                }

        //                activityLog.Status = "Open";

        //                db.ActivitiesLogs.Add(activityLog);

        //            }

        //            if (actionType == "Pending")
        //            {

        //                var ac = (db.ActivitiesLogs.Where(x => x.TaskId == taskId).OrderByDescending(x => x.Id).FirstOrDefault());

        //                activity.Status = "Pending";

        //                activityLog.AssignedBy = User.Identity.GetUserId();
        //                activityLog.AssingedTo = User.Identity.GetUserId();
        //                activityLog.Status = "Pending";
        //                activityLog.StartDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edos")));
        //                activityLog.EndDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edoc")));

        //                activityLog.BudgetedHours = Convert.ToInt32(form.Get("budgetedHours"));
        //                if (hoursWorked != "" && hoursWorked != "NaN")
        //                //System.Single.IsNaN(Convert.ToInt32(form.Get("hoursWorked")))
        //                {
        //                    activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
        //                }

        //                if (quantityWorked != "" && quantityWorked != "NaN")
        //                {
        //                    activityLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked"));
        //                }

        //                    string subject = "An update on your ticket with Ticket Id #" + taskId + " ( " + activity.Subject + " )";
        //                    string from = "";
        //                    string to = client.Email;
        //                    var body = @"<div style=""border: 1px solid gray;margin-top:2px; margin-right: 2px;padding: 9px;"" > 
        //                             <p> Dear " + client.Name + ", </p>" +
        //                                "<p> An update related to TicketId #" + activity.TaskId + " ( " + activity.Subject + " ) is available.  " + "</p>" +
        //                                "<p> We encourage you to kindly use our support portal http://ourclientSupport.azurewebsites.net for reporting issues/Service Requests with support team or to seek any further updates on your ticket. </p>" +
        //                                "<p> CUSTOMER SERVICE: 9 66 67 24 635 </p>" +
        //                                   " </div>";
        //                    bool sendEmail = new EmailController().SendEmail(from, "Task Management System", to, subject, body);
                        
        //                db.ActivitiesLogs.Add(activityLog);
        //                db.Entry(activity).State = System.Data.Entity.EntityState.Modified;
        //            }

        //            if (actionType == "Resolved")
        //            {
        //                activity.TaskOwner = activity.CreatedBy;
        //                activity.Status = "Resolved";

        //                activityLog.AssignedBy = User.Identity.GetUserId();
        //                activityLog.AssingedTo = activity.CreatedBy;
        //                activityLog.Status = "Resolved";

        //                activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));

        //                if (quantityWorked != "" && quantityWorked != "NaN")
        //                {
        //                    activityLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked"));
        //                }


        //                activity.ActivitiesLogs.Add(activityLog);
        //                db.Entry(activity).State = System.Data.Entity.EntityState.Modified;

        //                var user = activity.CreatedBy;

        //                    string subject = "Your ticket with ticket Id #" + taskId + " ( " + activity.Subject + " )  is resolved ";
        //                    string from = "";
        //                    string to = client.Email;
        //                    var body = @"<div style=""border: 1px solid gray;margin-top:2px; margin-right: 2px;padding: 9px;"" > 
        //                             <p> Dear " + client.Name + ", </p>" +
        //                                "<p> Your issue related to TicketId " + activity.TaskId + " ( " + activity.Subject + " ) has been resolved. " + "</p>" +
        //                                "<p> We encourage you to kindly use our support portal http://ourclientSupport.azurewebsites.net for reporting issues/Service Requests with support team or to seek any further updates on your ticket. </p>" +
        //                                "<p> CUSTOMER SERVICE: 9 66 67 24 635 </p>" +
        //                                   " </div>";
        //                    bool sendEmail = new EmailController().SendEmail(from, "Task Management System", to, subject, body);
                       
        //            }

        //            if (actionType == "AcceptToClose")
        //            {
        //                activity.Status = "Closed";
        //                activity.CompletedDate = MasterDataController.GetIndianTime(DateTime.Now);

        //                activityLog.AssignedBy = activity.TaskOwner;
        //                activityLog.AssingedTo = activity.CreatedBy;
        //                activityLog.Status = "Closed by assignee";
        //                activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));

                       
        //                    string subject = "Your ticket with ticket Id #" + taskId + " ( " + activity.Subject + " )  is Closed ";
        //                    string from = "";
        //                    string to = client.Email;
        //                    var body = @"<div style=""border: 1px solid gray;margin-top:2px; margin-right: 2px;padding: 9px;"" > 
        //                             <p> Dear " + client.Name + ", </p>" +
        //                                "<p> Your issue related to TicketId #" + activity.TaskId + " ( " + activity.Subject + " )  is closed. If  " + "</p>" +
        //                                "<p> We encourage you to kindly use our support portal http://ourclientSupport.azurewebsites.net for reporting issues/Service Requests with support team or to seek any further updates on your ticket. </p>" +
        //                                "<p> CUSTOMER SERVICE: 9 66 67 24 635 </p>" +
        //                                   " </div>";
        //                    bool sendEmail = new EmailController().SendEmail(from, "Task Management System", to, subject, body);
                       
        //                db.ActivitiesLogs.Add(activityLog);
        //            }

        //            if (actionType == "Reopen")
        //            {
        //                var lastWorked = db.ActivitiesLogs.Where(x => x.TaskId == taskId).OrderByDescending(x => x.TaskDate).FirstOrDefault();

        //                activity.Status = "Reopened";
        //                activity.TaskOwner = lastWorked.AssignedBy;
        //                activity.CompletedDate = null;
        //                activityLog.AssingedTo = lastWorked.AssignedBy;
        //                activityLog.AssignedBy = User.Identity.GetUserId();
        //                activityLog.Status = "Reopened";

        //                //  activityLog.TaskId = activity.TaskId;
        //                //  activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
        //                //  activityLog.Description = form.Get("description");

        //                db.ActivitiesLogs.Add(activityLog);
        //            }


        //            var files = HttpContext.Current.Request.Files;
        //            var fileAttachments = new List<HttpPostedFile>();
        //            if (files.Count > 0)
        //            {
        //                for (int i = 0; i < files.Count; i++)
        //                {
        //                    fileAttachments.Add(files[i]);
        //                }

        //                foreach (var file in fileAttachments)
        //                {
        //                    var fileDirecory = HttpContext.Current.Server.MapPath("~/Task");

        //                    if (!Directory.Exists(fileDirecory))
        //                    {
        //                        Directory.CreateDirectory(fileDirecory);
        //                    }

        //                    var fileName = file.FileName;
        //                    var filePath = Path.Combine(fileDirecory, fileName);
        //                    file.SaveAs(filePath);

        //                    ActivityAttachment actAttachments = new ActivityAttachment();

        //                    actAttachments.AttachmentURL = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "Task", fileName);
        //                    actAttachments.Name = Path.GetFileNameWithoutExtension(file.FileName);
        //                    activityLog.ActivityAttachments.Add(actAttachments);
        //                }
        //            }
        //            db.Entry(activity).State = System.Data.Entity.EntityState.Modified;
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
        [HttpGet]
        public IHttpActionResult GetClientTickets(string clientId, int? page, int? count)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var clientTickets = db.GetClientTickets(clientId, page, count).OrderByDescending(x => x.CreatedDate).ToList();

                    int PriorityHigh = 0;
                    int PriorityMedium = 0;
                    int PriorityLow = 0;

                    List<ClientServiceModel> clientTicketsList = new List<ClientServiceModel>();

                    if (clientTickets.Count > 0)
                    {
                        for (int i = 0; i < clientTickets.Count; i++)
                        {
                            ClientServiceModel ticket = new ClientServiceModel();

                            ticket.TicketId = clientTickets[i].TaskId;
                            ticket.Priority = clientTickets[i].Priority;
                            ticket.Subject = clientTickets[i].subject;
                            ticket.Status = clientTickets[i].Status;
                            ticket.CreatedDate = clientTickets[i].CreatedDate;

                            var Ticket = ticket.TicketId;
                            var LastUpdateOfTicket = db.ActivitiesLogs.Where(x => x.TaskId == Ticket).OrderByDescending(x => x.TaskDate).FirstOrDefault();

                            ticket.LastUpdated = LastUpdateOfTicket.TaskDate;
                            ticket.TaskOwner = clientTickets[i].TaskOwner;

                            if (clientTickets[i].Priority == 0)
                            {
                                PriorityHigh++;
                            }
                            else if (clientTickets[i].Priority == 1)
                            {
                                PriorityMedium++;
                            }
                            else
                            {
                                PriorityLow++;
                            }

                            ticket.TotalTickets = clientTickets.FirstOrDefault().TotalCount;

                            clientTicketsList.Add(ticket);
                        }
                    }

                    int TotalCount = PriorityHigh + PriorityMedium + PriorityLow;

                    return Content(HttpStatusCode.OK, new { clientTicketsList, PriorityHigh, PriorityMedium, PriorityLow, TotalCount });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetTicketLog(string taskId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var clientTicketLog = db.GetTicketLog(taskId).ToList();

                    List<ClientTicketLog> ticketLogList = new List<ClientTicketLog>();

                    if (clientTicketLog.Count > 0)
                    {

                        for (int i = 0; i < clientTicketLog.Count; i++)
                        {
                            ClientTicketLog ticketLog = new ClientTicketLog();
                            ticketLog.CreatedBy = clientTicketLog[i].AssignedBy;
                            ticketLog.Description = clientTicketLog[i].Description;
                            ticketLog.Status = clientTicketLog[i].Status;
                            ticketLog.TaskDate = clientTicketLog[i].TaskDate;
                            ticketLog.ActivityLogId = clientTicketLog[i].ActivityLogId;
                            ticketLog.Department = clientTicketLog[i].Department;

                            var atchmts = db.ActivityAttachments.Where(x => x.ActivityId == ticketLog.ActivityLogId).ToList();
                            // ticketLog.Attachments = atchmts.Select(x => x.AttachmentURL).ToArray();

                            List<Attachment> attachments = new List<Attachment>();

                            if (atchmts.Count > 0)
                            {
                                foreach (var at in atchmts)
                                {
                                    Attachment att = new Attachment();
                                    att.AttachmentUrl = at.AttachmentURL;
                                    att.FileName = at.Name;
                                    att.Id = at.Id;
                                    attachments.Add(att);
                                }

                                //  ticketLog.Attachments= db.ActivityAttachments.Where(x => x.ActivityId == ticketLog.ActivityLogId).Select(x => new { value = x.AttachmentURL, label = x.Name }).ToList();
                            }

                            ticketLog.Attachments = attachments;
                            ticketLogList.Add(ticketLog);
                        }
                    }
                    var clientId = User.Identity.GetUserId();

                    var lastRespondedEmp = db.GetTicketLog(taskId).Where(x => x.AssignedById != clientId).OrderByDescending(x => x.TaskDate).FirstOrDefault();

                    return Content(HttpStatusCode.OK, new { ticketLogList, lastRespondedEmp });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpPost]
        public IHttpActionResult AddClientTicketResponse()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;

                    ActivitiesLog actLog = new ActivitiesLog();
                    actLog.AssignedBy = User.Identity.GetUserId();
                    actLog.AssignedTo = form.Get("assignedTo");
                    actLog.Status = "Pending";
                    actLog.Description = form.Get("description");
                    actLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                    actLog.TaskId = form.Get("taskId");

                    db.ActivitiesLogs.Add(actLog);
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

        [HttpPost]
        public IHttpActionResult FeedBackFromClient()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;

                    var ticketId = form.Get("TicketId");
                    string status = form.Get("Status");

                    Activity activity = db.Activities.Where(x => x.TaskId == ticketId).FirstOrDefault();
                    ActivitiesLog actLog = new ActivitiesLog();
                    if (activity != null)
                    {
                        if (status == "true")
                        {
                            activity.Status = "Closed";
                            activity.CompletedDate = MasterDataController.GetIndianTime(DateTime.Now);

                            actLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                            actLog.TaskId = ticketId;
                            actLog.AssignedBy = activity.TaskOwner;
                            actLog.AssignedTo = activity.CreatedBy;
                            actLog.Status = "Closed by assignee";
                            actLog.Description = "Satisfactory";

                            db.ActivitiesLogs.Add(actLog);
                            db.Entry(activity).State = System.Data.Entity.EntityState.Modified;

                            var taskCreator = db.Employees.Where(x => x.AspNetUserId == activity.CreatedBy && x.Active == true).FirstOrDefault();

                            db.SaveChanges();
                        }

                        else
                        {
                            if (activity.Status == "Resolved")
                            {
                                activity.Status = "Reopened";
                                activity.CompletedDate = null;

                                var lastWorked = db.ActivitiesLogs.Where(x => x.TaskId == ticketId).OrderByDescending(x => x.TaskDate).FirstOrDefault();

                                activity.Status = "Reopened";
                                activity.TaskOwner = lastWorked.AssignedBy;
                                activity.CompletedDate = null;

                                actLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                                actLog.TaskId = ticketId;
                                actLog.AssignedTo = lastWorked.AssignedBy;
                                actLog.AssignedBy = User.Identity.GetUserId();
                                actLog.Status = "Reopened";
                                actLog.Description = "Ticket Reopened ";

                                db.ActivitiesLogs.Add(actLog);
                                db.Entry(activity).State = System.Data.Entity.EntityState.Modified;

                                var taskCreator = db.Employees.Where(x => x.AspNetUserId == activity.CreatedBy && x.Active == true).FirstOrDefault();

                                if (taskCreator != null)
                                { }
                                else
                                {
                                    var Client = db.Clients.Where(x => x.AspNetUserId == activity.CreatedBy).FirstOrDefault();
                                    if (Client != null)
                                    {
                                        string subject = "Your Ticket Id #" + activity.TaskId + " ( " + activity.Subject + " ) is reopened. ";
                                        string from = "";
                                        string to = Client.Email;
                                        var body = @"<div style=""border: 1px solid gray;margin-top:2px; margin-right: 2px;padding: 9px;"" > 
                                     <p> Dear " + Client.Name + ", </p>" +
                                                    "<p> your ticket with Id " + activity.TaskId + " ( " + activity.Subject + " ) is reopened and the support team will contact you soon.  " + "</p>" +
                                                    "<p> We encourage you to kindly use our support portal http://ourclientSupport.azurewebsites.net for reporting issues/Service Requests with support team or to seek any further updates on your ticket. </p>" +
                                                    "<p> CUSTOMER SERVICE: 9 66 67 24 635 </p>" +
                                                       " </div>";
                                        bool sendEmail = new EmailController().SendEmail(from, "Task Management System", to, subject, body);
                                    }
                                }

                                db.SaveChanges();

                            }
                        }
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
    }
}