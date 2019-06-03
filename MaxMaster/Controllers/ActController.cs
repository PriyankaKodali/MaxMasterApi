using MaxMaster.Models;
using MaxMaster.ViewModels;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;


namespace MaxMaster.Controllers
{
    public class ActController : ApiController
    {

        public static string GenerateTaskId()
        {
            using (MaxMasterDbEntities db = new MaxMasterDbEntities())
            {
                var activitiesCount = db.Activities.Count();
                if (activitiesCount > 0)
                {
                    var lastActivity = db.Activities.OrderByDescending(x => x.CreatedDate).FirstOrDefault().TaskId;
                    int count = Convert.ToInt32(lastActivity.Split('-')[1]);
                    GenerateNewNumber:
                    var taskId = "MAX-" + (count + 1).ToString("0000");
                    if (db.Activities.Any(x => x.TaskId == taskId))
                    {
                        count++;
                        goto GenerateNewNumber;
                    }
                    return taskId;
                }
                else
                {
                    var taskId = "MAX-0001";
                    return taskId;
                }
            }

        }

        #region "Edit Activity"
        [HttpPost]
        public IHttpActionResult EditActivity()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var taskId = form.Get("taskId");
                    var userId = User.Identity.GetUserId();
                    var hoursWorked = form.Get("hoursWorked");
                    var quantityWorked = form.Get("quantityWorked");
                    int orgId = Convert.ToInt32(form.Get("OrgId"));
                    var actionType = form.Get("actionType");

                    var activityNotification = db.AddActivityNotifications(taskId, User.Identity.GetUserId());

                    Activity mainTask = db.Activities.Find(taskId);

                    if (actionType == "Assign")
                    {
                        var assigneeList = JsonConvert.DeserializeObject<List<Assignee>>(form.Get("assigneeList"));

                        if (assigneeList.Count() > 1)
                        {
                            foreach (var assignee in assigneeList)
                            {
                                var parentTaskLog = JsonConvert.DeserializeObject<List<ActivityLog>>(form.Get("ParentTaskDetails"));
                                bool CreateNewActivity = AddChildTask(mainTask, assignee, parentTaskLog);
                                if (!CreateNewActivity)
                                {
                                    return Content(HttpStatusCode.InternalServerError, "Faied to assignee task");
                                }
                            }
                        }
                        else
                        {
                            if (mainTask.ParentTask_Id != null)
                            {
                                ActivitiesLog parentLog = new ActivitiesLog();

                                parentLog.TaskId = mainTask.ParentTask_Id;
                                parentLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                                parentLog.AssignedBy = User.Identity.GetUserId();
                                parentLog.AssignedTo = assigneeList[0].AssigneeId;
                                parentLog.Status = "Open";
                                if (hoursWorked != "" && hoursWorked != "NaN")
                                {
                                    parentLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                                }
                                parentLog.Description = form.Get("description");
                                if (quantityWorked != "" && quantityWorked != "NaN")
                                {
                                    parentLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked"));
                                }

                                db.ActivitiesLogs.Add(parentLog);
                            }

                            mainTask.Status = "Open";
                            mainTask.TaskOwner = assigneeList[0].AssigneeId;
                            mainTask.LastUpdatedBy = User.Identity.GetUserId();
                            mainTask.LastUpdated = MasterDataController.GetIndianTime(DateTime.Now);

                        }
                    }

                    else
                    {
                        var childTasks = db.Activities.Where(x => x.ParentTask_Id == mainTask.TaskId).ToList();
                        Activity parentTask = db.Activities.Where(x => x.TaskId == mainTask.ParentTask_Id).FirstOrDefault();

                        ActivitiesLog activityLog = new ActivitiesLog();

                        if (childTasks.Count > 0)
                        {
                            foreach (var child in childTasks)
                            {
                                ActivitiesLog childLog = new ActivitiesLog();

                                childLog.TaskId = child.TaskId;
                                childLog.AssignedBy = User.Identity.GetUserId();
                                childLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                                childLog.Description = form.Get("description");
                                childLog.BudgetedHours = Convert.ToInt32(form.Get("budgetedHours"));

                                db.ActivitiesLogs.Add(childLog);
                            }
                        }

                        if (actionType == "InProcess")
                        {

                            activityLog.AssignedTo = User.Identity.GetUserId();
                            activityLog.StartDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edos")));
                            activityLog.EndDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edoc")));
                            activityLog.Status = "InProcess";

                            mainTask.Status = "InProcess";

                        }
                        else if (actionType == "Pending")
                        {
                            activityLog.AssignedTo = User.Identity.GetUserId();
                            activityLog.StartDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edos")));
                            activityLog.EndDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edoc")));
                            activityLog.Status = "Pending";

                            mainTask.Status = "Pending";
                        }
                        else if (actionType == "Resolved")
                        {
                            activityLog.AssignedTo = mainTask.CreatedBy;
                            activityLog.AssignedBy = User.Identity.GetUserId();
                            activityLog.Status = "Resolved";

                            mainTask.TaskOwner = mainTask.CreatedBy;
                            mainTask.Status = "Resolved";
                        }
                        else if (actionType == "AcceptToClose")
                        {
                            activityLog.Status = "Closed by assignee";
                            activityLog.AssignedBy = mainTask.TaskOwner;
                            activityLog.AssignedTo = mainTask.CreatedBy;

                            mainTask.TaskOwner = mainTask.CreatedBy;
                            mainTask.Status = "Closed";

                        }
                        else if (actionType == "Comments")
                        {
                            activityLog.AssignedTo = mainTask.TaskOwner;
                            activityLog.AssignedBy = User.Identity.GetUserId();

                            mainTask.Status = mainTask.Status;
                        }
                        else
                        {
                            if (actionType == "Reopen")
                            {
                                activityLog.AssignedTo = mainTask.LastUpdatedBy;
                                activityLog.AssignedBy = User.Identity.GetUserId();
                                activityLog.Status = "Reopened";

                                mainTask.Status = "Reopened";
                            }
                        }

                        if (parentTask != null)
                        {
                            ActivitiesLog parentLog = new ActivitiesLog();

                            parentLog.TaskId = parentTask.ParentTask_Id;
                            parentLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                            parentLog.AssignedBy = User.Identity.GetUserId();
                            parentLog.AssignedTo = parentTask.TaskOwner;

                            if (actionType == "Pending" || actionType == "Comments")
                            {
                                parentLog.Status = "Pending";
                            }
                            else if (actionType == "InProcess")
                            {
                                parentLog.Status = "InProcess";
                            }
                            else if (actionType == "Assign")
                            {
                                parentLog.Status = "Open";
                            }
                            else if (actionType == "Reopen")
                            {
                                parentLog.Status = "Reopened";
                            }
                            else if (actionType == "AcceptToClose")
                            {
                                var childTasksList = db.Activities.Where(x => x.ParentTask_Id == mainTask.TaskId).ToList();

                            }
                            else if (actionType == "Resolved")
                            {

                            }
                            else
                            {
                                parentLog.Status = "";
                            }

                            parentLog.BudgetedHours = Convert.ToInt32(form.Get("budgetedHours"));
                            if (hoursWorked != "" && hoursWorked != "NaN")
                            {
                                parentLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                            }
                            parentLog.Description = form.Get("description");
                            if (quantityWorked != "" && quantityWorked != "NaN")
                            {
                                parentLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked"));
                            }
                            db.ActivitiesLogs.Add(parentLog);
                        }

                        activityLog.AssignedBy = User.Identity.GetUserId();
                        activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                        activityLog.Description = form.Get("description");
                        activityLog.BudgetedHours = Convert.ToInt32(form.Get("budgetedHours"));
                        if (hoursWorked != "" && hoursWorked != "NaN")
                        {
                            activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                        }

                        if (quantityWorked != "" && quantityWorked != "NaN")
                        {
                            activityLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked"));
                        }
                        mainTask.ActivitiesLogs.Add(activityLog);

                        mainTask.LastUpdatedBy = User.Identity.GetUserId();
                        mainTask.LastUpdated = MasterDataController.GetIndianTime(DateTime.Now);

                        db.Entry(mainTask).State = System.Data.Entity.EntityState.Modified;

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
        #endregion



        public bool AddChildTask(Activity activity, Assignee assignee, List<ActivityLog> parentLog)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var u = User.Identity.GetUserId();
                    var user = db.Employees.Where(x => x.AspNetUserId == u).FirstOrDefault();

                    Activity task = new Activity();
                    ActivitiesLog activityLog = new ActivitiesLog();

                    task.AssignedTo = assignee.AssigneeId;
                    task.Category_Id = activity.Category_Id;
                    task.SubCategory_Id = activity.SubCategory_Id;
                    if (assignee.Quantity != null)
                    {
                        task.Subject = activity.Subject + " Quanity - " + assignee.Quantity;
                    }
                    else
                    {
                        task.Subject = activity.Subject;
                    }

                    task.TaskOwner = assignee.AssigneeId;
                    task.Description = activity.Description;
                    task.EDOC = Convert.ToDateTime(activity.EDOC);
                    task.Status = "Open";
                    task.TaskType = activity.TaskType;
                    task.Client_Id = activity.Client_Id;
                    task.Project_Id = activity.Project_Id;
                    task.Priority = activity.Priority;
                    task.CreatedBy = User.Identity.GetUserId();
                    task.CreatedDate = MasterDataController.GetIndianTime(DateTime.UtcNow);
                    task.ParentTask_Id = activity.TaskId;
                    if (assignee.Quantity != null)
                    {
                        task.Quantity = assignee.Quantity;
                    }
                    //else
                    //{
                    //    task.Quantity = null;
                    //}

                    task.TaskId = GenerateTaskId();

                    db.Activities.Add(task);
                    if (parentLog.Count() > 0)
                    {
                        for (int i = 0; i < parentLog.Count(); i++)
                        {
                            ActivitiesLog Log = new ActivitiesLog();

                            Log.Status = parentLog[i].Status;
                            Log.Description = parentLog[i].Description;
                            Log.TaskDate = Convert.ToDateTime(parentLog[i].TaskDate);
                            Log.AssignedBy = parentLog[i].AssignedById;
                            Log.AssignedTo = parentLog[i].AssignedToId;
                            Log.HoursWorked = parentLog[i].HoursWorked;
                            Log.QuantityWorked = parentLog[i].QuantityWorked;

                            task.ActivitiesLogs.Add(Log);
                        }
                    }

                    activityLog.Status = task.Status;
                    activityLog.Description = task.Description;
                    activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.UtcNow);
                    activityLog.AssignedBy = task.CreatedBy;
                    activityLog.AssignedTo = assignee.AssigneeId;
                    activityLog.TaskId = activity.TaskId;

                    task.ActivitiesLogs.Add(activityLog);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return false;
            }

        }
        
    }


}