using System;
using System.Collections.Generic;
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
                    actLog.AssingedTo = form.Get("assignedTo");
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
                            actLog.AssingedTo = activity.CreatedBy;
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
                                actLog.AssingedTo = lastWorked.AssignedBy;
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