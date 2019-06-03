using MaxMaster.Models;
using MaxMaster.ViewModels;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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

        #region "Add Activity "
        [HttpPost]
        public IHttpActionResult AddActivity()
        {
            var form = HttpContext.Current.Request.Form;
            var clientId = form.Get("clientId");
            var departmentId = form.Get("departmentId");
            int orgId = Convert.ToInt32(form.Get("OrgId"));
            var categoryId = form.Get("categoryId");
            var projectId = form.Get("projectId");


            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var u = User.Identity.GetUserId();
                    var user = db.Employees.Where(x => x.AspNetUserId == u).FirstOrDefault();
                    var assigneeList = JsonConvert.DeserializeObject<List<Assignee>>(form.Get("assigneeList"));
                    
                    foreach (var employee in assigneeList)
                    {
                        Activity activity = new Activity();

                        if (clientId != null)
                        {
                            activity.Client_Id = (clientId);
                           if(projectId != null)
                            {
                                activity.Project_Id = Convert.ToInt32(form.Get("projectId"));
                            }
                        }

                        if (departmentId != null)
                        {
                            activity.Department_Id = Convert.ToInt32(departmentId);
                        }

                        var ed = (form.Get("edoc"));
                        activity.EDOC = DateTime.Parse(ed);
                        activity.TaskType = form.Get("taskType");
                    
                        if (employee.Quantity != null)
                        {
                            activity.Quantity = Convert.ToInt32(employee.Quantity);
                        } 

                     activity.CreatedBy = User.Identity.GetUserId(); 
                     activity.Category_Id = Convert.ToInt32(form.Get("categoryId"));
                     activity.SubCategory_Id = Convert.ToInt32(form.Get("subCategoryId"));
                     activity.CreatedDate =MasterDataController.GetIndianTime(DateTime.UtcNow);
                     activity.Subject = form.Get("task");
                     activity.Priority = Convert.ToInt32(form.Get("priority"));
                     activity.Status = "Open";
                     activity.Description = form.Get("Description");
                     activity.TaskOwner = employee.AssigneeId;
                     activity.AssignedTo= employee.AssigneeId;
                        //  activity.TaskId = RandomString(4, orgId, createdby, false);
                     activity.TaskId = GenerateTaskId();
                     activity.LastUpdatedBy = User.Identity.GetUserId();
                     activity.LastUpdated = MasterDataController.GetIndianTime(DateTime.Now);
                        db.Activities.Add(activity);
                
                     ActivitiesLog activityLog = new ActivitiesLog();

                     activityLog.Status = activity.Status;
                     activityLog.Description = activity.Description;
                     activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.UtcNow);
                     activityLog.AssignedBy = activity.CreatedBy;
                     activityLog.AssignedTo = employee.AssigneeId;

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
                       var activityNotification = db.AddActivityNotifications(activity.TaskId, User.Identity.GetUserId());
                       db.SaveChanges();
                    }
                 
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
                    var taskId = form.Get("taskId");
                    var userId = User.Identity.GetUserId();
                    var hoursWorked = form.Get("hoursWorked");
                    var quantityWorked = form.Get("quantityWorked"); 
                    var actionType = form.Get("actionType");
                    var points =  Convert.ToInt32(form.Get("points"));

                    Activity activity = db.Activities.Find(taskId);
                    ActivitiesLog activityLog = new ActivitiesLog();
                    string creator = activity.CreatedBy;

                    var client = db.Clients.Where(x => x.AspNetUserId == creator).FirstOrDefault();
                    var employee = db.Employees.Where(x => x.AspNetUserId == creator).FirstOrDefault();

                    activityLog.TaskId = activity.TaskId;
                    activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                    activityLog.Description = form.Get("description");

                    #region " Action type Assign" 
                    if (actionType == "Assign")
                    {
                        var assigneeList = JsonConvert.DeserializeObject<List<Assignee>>(form.Get("assigneeList"));
                        var parentTaskDetails = JsonConvert.DeserializeObject<List<TaskLog>>(form.Get("ParentTaskDetails"));
                        // var parentTaskDetails = JsonConvert.DeserializeObject<List<ActivityLog>>(form.Get("ParentTaskDetails"));
                        {/* check  */ }
                        if (assigneeList.Count() == 1)
                        {
                            var parentTask = db.Activities.Where(x => x.TaskId == activity.TaskId).FirstOrDefault();

                            if (parentTask.ParentTask_Id != null)
                            {
                                ActivitiesLog parentLog = new ActivitiesLog();

                                parentLog.TaskId = activity.ParentTask_Id;
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
                                    parentLog.Points = (points) * Convert.ToInt32(form.Get("quantityWorked")) ;
                                }

                                db.ActivitiesLogs.Add(parentLog);
                            }

                            activity.TaskOwner = assigneeList[0].AssigneeId;
                            activityLog.AssignedBy = User.Identity.GetUserId();
                            activityLog.AssignedTo = assigneeList[0].AssigneeId;

                            activityLog.TaskId = activity.TaskId;
                            activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                            activityLog.Description = form.Get("description");

                            if (hoursWorked != "" && hoursWorked != "NaN")
                            {
                                activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                            }
                            if (quantityWorked != "" && quantityWorked != "NaN")
                            {
                                activityLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked"));
                                activityLog.Points = (points) * Convert.ToInt32(form.Get("quantityWorked"));
                            }
                            activityLog.Status = "Open";
                            db.ActivitiesLogs.Add(activityLog);
                        }
                        else
                        {
                            foreach (var assignee in assigneeList)
                            {
                                ActivitiesLog actLog = new ActivitiesLog();
                                bool CreateNewActivity = AddChildTask(activity, assignee, parentTaskDetails);
                                if (!CreateNewActivity)
                                {
                                    return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
                                }
                                actLog.TaskId = activity.TaskId;
                                actLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                                actLog.Description = form.Get("description");
                                actLog.AssignedBy = User.Identity.GetUserId();
                                actLog.AssignedTo = assignee.AssigneeId;

                                if (hoursWorked != "" && hoursWorked != "NaN")
                                {
                                    actLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                                }
                                if (quantityWorked != "" && quantityWorked != "NaN")
                                {
                                    activityLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked"));
                                    activityLog.Points = (points) * Convert.ToInt32(form.Get("quantityWorked"));
                                }
                                actLog.Status = "Open";
                                db.ActivitiesLogs.Add(actLog);
                            }
                        }

                        activity.Status = "Open";
                    }
                    #endregion

                    #region "Action type pending"
                    if (actionType == "Pending")
                    {
                        activity.Status = "Pending";

                        if (activity.ParentTask_Id != null)
                        {
                            ActivitiesLog parentLog = new ActivitiesLog();

                            parentLog.TaskId = activity.ParentTask_Id;
                            parentLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                            parentLog.AssignedBy = User.Identity.GetUserId();
                            parentLog.AssignedTo = activity.TaskOwner;
                            parentLog.Status = "Pending";
                            parentLog.BudgetedHours = Convert.ToInt32(form.Get("budgetedHours"));
                            if (hoursWorked != "" && hoursWorked != "NaN")
                            {
                                parentLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                            }
                            parentLog.Description = form.Get("description");
                            if (quantityWorked != "" && quantityWorked != "NaN")
                            {
                                parentLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked")); 
                                parentLog.Points = (points) * Convert.ToInt32(form.Get("quantityWorked"));
                            }

                            db.ActivitiesLogs.Add(parentLog);
                        }

                        activityLog.TaskId = activity.TaskId;
                        activityLog.AssignedBy = User.Identity.GetUserId();
                        activityLog.AssignedTo = activity.TaskOwner;
                        activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                        activityLog.Status = "Pending";
                        activityLog.StartDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edos")));
                        activityLog.EndDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edoc")));
                        activityLog.Description = form.Get("description");
                        activityLog.BudgetedHours = Convert.ToInt32(form.Get("budgetedHours"));
                        if (hoursWorked != "" && hoursWorked != "NaN")
                        {
                            activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                        }

                        if (quantityWorked != "" && quantityWorked != "NaN")
                        {
                            activityLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked"));
                            activityLog.Points = (points) * Convert.ToInt32(form.Get("quantityWorked"));
                        }
                        db.ActivitiesLogs.Add(activityLog);
                        db.Entry(activity).State = System.Data.Entity.EntityState.Modified;
                    }
                    #endregion

                    #region "Action Type Resolved"
                    if (actionType == "Resolved")
                    {
                        activity.TaskOwner = activity.CreatedBy;
                        activity.Status = "Resolved";

                        if (activity.ParentTask_Id != null)
                        {
                            {/* List of child tasks for activities */ }
                            var childTaskList = db.Activities.Where(x => x.ParentTask_Id == activity.ParentTask_Id).ToList();
                            {/* Get parent task of the task if exists */}
                            var parentActivity = db.Activities.Where(x => x.TaskId == activity.ParentTask_Id).FirstOrDefault();
                            {/* Check if all child tasks are resolved */}
                            var ChildTaskexists = childTaskList.Any(x => x.Status == "Pending" || x.Status == "Reopened" || x.Status == "Open");

                            ActivitiesLog parentLog = new ActivitiesLog();
                            parentLog.TaskId = activity.ParentTask_Id;
                            parentLog.AssignedBy = User.Identity.GetUserId();
                            parentLog.AssignedTo = parentActivity.AssignedTo;
                            parentLog.Description = form.Get("description");
                            parentLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);

                            if (ChildTaskexists)
                            {
                                parentActivity.Status = "Pending";
                                parentActivity.TaskOwner = parentActivity.AssignedTo;

                                parentLog.Status = "Resolved";
                                db.Entry(parentActivity).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                            {
                                parentActivity.Status = "Resolved";
                                parentLog.Status = "Resolved";
                                db.Entry(parentActivity).State = System.Data.Entity.EntityState.Modified;
                            }
                            parentLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));

                            if (quantityWorked != "" && quantityWorked != "NaN")
                            {
                                parentLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked"));
                                parentLog.Points = (points) * Convert.ToInt32(form.Get("quantityWorked"));
                            }
                            db.ActivitiesLogs.Add(parentLog);
                        }

                        activityLog.AssignedBy = User.Identity.GetUserId();
                        activityLog.AssignedTo = activity.CreatedBy;
                        activityLog.Status = "Resolved";
                        if (form.Get("hoursWorked") != "" && form.Get("hoursWorked") != "NaN")
                        {
                            activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                        }
                        activityLog.Description = form.Get("description");
                        activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                        activityLog.TaskId = activity.TaskId;
                        if (quantityWorked != "" && quantityWorked != "NaN")
                        {
                            activityLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked"));
                            activityLog.Points = (points) * Convert.ToInt32(form.Get("quantityWorked"));
                        }
                        activity.ActivitiesLogs.Add(activityLog);
                        db.Entry(activity).State = System.Data.Entity.EntityState.Modified;
                    }
                    #endregion

                    #region "Action  Accept to close"
                    if (actionType == "AcceptToClose")
                    {
                        activity.Status = "Closed";
                        activity.CompletedDate = MasterDataController.GetIndianTime(DateTime.Now);
                        activity.TaskOwner = User.Identity.GetUserId();
                        activity.LastUpdatedBy = User.Identity.GetUserId();
                        activity.LastUpdated = MasterDataController.GetIndianTime(DateTime.Now);
                        bool childTasksCompleted = true;

                        {/* Get the parent task if exists */}
                        var ParentTask = db.Activities.Where(x => x.TaskId == activity.ParentTask_Id).FirstOrDefault();

                        {/* If all child tasks are closed then parent task status is resolved  */}
                        if (ParentTask != null)
                        {
                            var childTaskList = db.Activities.Where(x => x.ParentTask_Id == ParentTask.TaskId).ToList();
                            Activity mainTask = db.Activities.Where(x => x.TaskId == ParentTask.TaskId).FirstOrDefault();
                             
                            mainTask.LastUpdated = MasterDataController.GetIndianTime(DateTime.Now);
                            mainTask.LastUpdatedBy = User.Identity.GetUserId();

                            if (childTaskList.Count() > 0)
                            {
                                foreach (var child in childTaskList)
                                {
                                    Activity childActualTask = db.Activities.Where(x => x.TaskId == child.TaskId).FirstOrDefault();
                                    childActualTask.Status = "Closed";
                                    childActualTask.CompletedDate = MasterDataController.GetIndianTime(DateTime.Now);

                                    ActivitiesLog childTaskLog = new ActivitiesLog();
                                    childTaskLog.TaskId = child.TaskId;
                                    childTaskLog.AssignedBy = User.Identity.GetUserId();
                                    childTaskLog.AssignedTo = activity.CreatedBy;
                                    childTaskLog.Status = "Closed by assignee";
                                    childTaskLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                                    childTaskLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                                    childTaskLog.Description = form.Get("description");

                                    db.ActivitiesLogs.Add(childTaskLog);
                                    db.Entry(childActualTask).State = System.Data.Entity.EntityState.Modified;
                                }

                                mainTask.Status = "Closed";
                                mainTask.TaskOwner = mainTask.CreatedBy;
                                mainTask.CompletedDate = MasterDataController.GetIndianTime(DateTime.Now);

                                db.Entry(mainTask).State = System.Data.Entity.EntityState.Modified;
                            }

                            var ChildTaskexists = childTaskList.Any(x => x.Status != "Closed");
                            if (ChildTaskexists)
                            {
                                childTasksCompleted = false;
                            }

                            if (childTasksCompleted)
                            {
                                mainTask.Status = "Resolved";
                                mainTask.TaskOwner = ParentTask.CreatedBy;
                            }

                            ActivitiesLog parentLog = new ActivitiesLog();
                            parentLog.TaskId = activity.ParentTask_Id;
                            parentLog.AssignedBy = User.Identity.GetUserId();
                            parentLog.AssignedTo = activity.CreatedBy;
                            parentLog.Description = form.Get("description");
                            parentLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                            parentLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                            parentLog.Description = form.Get("description");
                            parentLog.Status = "Closed by assignee";

                            db.ActivitiesLogs.Add(parentLog);
                            
                            db.Entry(mainTask).State = System.Data.Entity.EntityState.Modified;
                        }
                        // activityLog.AssignedBy = activity.TaskOwner;
                        activityLog.TaskId = activity.TaskId;
                        activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                        activityLog.Description = form.Get("description");

                        activityLog.AssignedBy = User.Identity.GetUserId();
                        activityLog.AssignedTo = activity.CreatedBy;
                        activityLog.Status = "Closed by assignee";
                        activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                        activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now); ;
                        activityLog.Description = form.Get("description");
                        db.ActivitiesLogs.Add(activityLog);
                    }
                    #endregion

                    #region "Action Reopen"
                    if (actionType == "Reopen")
                    {
                        var lastWorked = db.ActivitiesLogs.Where(x => x.TaskId == taskId).OrderByDescending(x => x.TaskDate).FirstOrDefault();
                        var parentTask = db.Activities.Where(x => x.TaskId == activity.ParentTask_Id).FirstOrDefault();

                        if (parentTask != null)
                        {
                            ActivitiesLog parentLog = new ActivitiesLog();
                            parentLog.TaskId = activity.ParentTask_Id;
                            parentLog.Status = "Reopened";
                            parentLog.AssignedBy = User.Identity.GetUserId();
                            parentLog.AssignedTo = lastWorked.AssignedBy;
                            parentLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                            parentLog.Description = form.Get("Description");

                            parentTask.Status = "Pending";
                            parentTask.TaskOwner = parentTask.AssignedTo;
                            parentTask.CompletedDate = null;
                            parentTask.LastUpdated = MasterDataController.GetIndianTime(DateTime.Now);
                            parentTask.LastUpdatedBy = User.Identity.GetUserId();

                            db.ActivitiesLogs.Add(parentLog);
                            db.Entry(parentTask).State = System.Data.Entity.EntityState.Modified;

                        }

                        activityLog.TaskId = activity.TaskId;
                        activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                        activityLog.Description = form.Get("description");
                        activity.TaskOwner = lastWorked.AssignedBy;
                        activityLog.AssignedTo = lastWorked.AssignedBy;
                        activityLog.AssignedBy = User.Identity.GetUserId();
                        activityLog.Status = "Reopened";

                        activity.Status = "Reopened";
                        activity.CompletedDate = null;
                        db.ActivitiesLogs.Add(activityLog);
                    }
                    #endregion

                    if (actionType == "Comments")
                    {
                        var childTasks = db.Activities.Where(x => x.ParentTask_Id == activity.TaskId).ToList();
                        if (childTasks.Count() > 0)
                        {
                            foreach (var act in childTasks)
                            {
                                ActivitiesLog childLog = new ActivitiesLog();

                                childLog.AssignedBy = User.Identity.GetUserId();
                                childLog.AssignedTo = act.TaskOwner;
                                childLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                                childLog.TaskId = act.TaskId;
                                childLog.Status = activity.Status;
                                childLog.Description = form.Get("Description");
                                activity.ActivitiesLogs.Add(childLog);

                            }
                        }
                        activityLog.AssignedBy = User.Identity.GetUserId();
                        activityLog.AssignedTo = activity.TaskOwner;
                        activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                        activityLog.TaskId = activity.ParentTask_Id;
                        activityLog.Status = activity.Status;
                        activityLog.Description = form.Get("Description");

                        activity.ActivitiesLogs.Add(activityLog);
                    }

                    if (actionType == "InProcess")
                    {
                        var parentTask = db.Activities.Where(x => x.ParentTask_Id == activity.TaskId).FirstOrDefault();

                        if(parentTask!=null)
                        {
                            ActivitiesLog parentLog = new ActivitiesLog();

                            parentLog.TaskId = parentTask.TaskId;
                            parentLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                            parentLog.AssignedBy = User.Identity.GetUserId();
                            parentLog.AssignedTo = User.Identity.GetUserId();
                            parentLog.Status = "In Process";
                            activityLog.StartDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edos")));
                            activityLog.EndDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edoc")));
                            parentLog.Description = form.Get("Description");
                            parentLog.BudgetedHours = Convert.ToInt32(form.Get("budgetedHours"));
                            if (hoursWorked != "" && hoursWorked != "NaN")
                            {
                                parentLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                            }
                            parentLog.Description = form.Get("description");
                            if (quantityWorked != "" && quantityWorked != "NaN")
                            {
                                parentLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked"));
                                parentLog.Points =(points) * Convert.ToInt32(form.Get("quantityWorked"));
                            }
                            db.ActivitiesLogs.Add(parentLog);
                        }

                        activityLog.AssignedBy = User.Identity.GetUserId();
                        activityLog.AssignedTo = activity.TaskOwner;
                        activityLog.StartDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edos")));
                        activityLog.EndDate = MasterDataController.GetIndianTime(Convert.ToDateTime(form.Get("edoc")));
                        activityLog.Status = "InProcess";

                        activityLog.TaskId = activity.TaskId;
                        activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.Now);
                        activityLog.Description = form.Get("description");

                        activityLog.BudgetedHours = Convert.ToInt32(form.Get("budgetedHours"));
                        if (hoursWorked != "" && hoursWorked != "NaN")
                        {
                            activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));
                        }
                      //  activityLog.Description = form.Get("description");
                        if (quantityWorked != "" && quantityWorked != "NaN")
                        {
                            activityLog.QuantityWorked = Convert.ToInt32(form.Get("quantityWorked"));
                            activityLog.Points = (points) * Convert.ToInt32(form.Get("quantityWorked"));
                        }
                        activity.Status = "InProcess"; 
                        activity.ActivitiesLogs.Add(activityLog);

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

                    activity.LastUpdated = MasterDataController.GetIndianTime(DateTime.Now);
                    activity.LastUpdatedBy = User.Identity.GetUserId();

                    db.Entry(activity).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                     
                    var activityNotification = db.AddActivityNotifications(taskId, User.Identity.GetUserId());

                    var employeesList = db.ActivityNotifications.Where(x => x.ActivityId == taskId).ToList();

                    List<string> Employees = new List<string>();

                    foreach(var emp in employeesList)
                    {
                        Employees.Add(emp.EmployeeId);
                    }
                     NotificationController n = new NotificationController();
                    if(actionType != "AcceptToClose")
                    {
                        n.SendNotificationOnTaskUpdateAsync(Employees, actionType, activity.TaskId, activity.Status);
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
                    int PriorityHigh = 0;
                    int PriorityMedium = 0;
                    int PriorityLow = 0;
                    
                    // stored procedure to get tasks on employee
                    var myTasks = db.GetMyTasks(EmpId, clientId, departmentId, taskType, priority, status, page, count, sortCol, sortDir).OrderByDescending(x => x.CreatedDate).ToList();
                   // var summaryReport = db.TaskStatsGet(User.Identity.GetUserId()).FirstOrDefault();
                    var summaryReport = db.TaskStatsGet(EmpId).FirstOrDefault();

                    ActivitiesSummary actSummary = new ActivitiesSummary();
                    actSummary.PriorityHighJobs = summaryReport.HighMy;
                    actSummary.PriorityMediumJobs = summaryReport.MediumMy;
                    actSummary.PriorityLowJobs = summaryReport.LowMy;
                    actSummary.TotalJobs = actSummary.PriorityHighJobs + actSummary.PriorityMediumJobs + actSummary.PriorityLowJobs;

                    if(myTasks.Count()>0)
                    {
                        totalPages = (int)myTasks.FirstOrDefault().TotalCount;
                    }
                    ActivitiesSummary actSummaryTasksOnMe = new ActivitiesSummary();
                    if (myTasks.Count > 0)
                    {


                        for (int i = 0; i < myTasks.Count; i++)
                        {
                            if (myTasks[i].Priority == 0) {
                                PriorityHigh++;
                            }
                            else if (myTasks[i].Priority == 1) {
                                PriorityMedium++;
                            }
                            else  {
                                PriorityLow++;
                            }
                        }
                    }
                    actSummaryTasksOnMe.PriorityHighJobs = PriorityHigh;
                    actSummaryTasksOnMe.PriorityMediumJobs = PriorityMedium;
                    actSummaryTasksOnMe.PriorityLowJobs = PriorityLow;
                    actSummaryTasksOnMe.TotalJobs = actSummaryTasksOnMe.PriorityHighJobs + actSummaryTasksOnMe.PriorityMediumJobs + actSummaryTasksOnMe.PriorityLowJobs;

                    return Content(HttpStatusCode.OK, new { myTasks, totalPages, actSummary, actSummaryTasksOnMe });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Invalid request");
            }
        }
        #endregion

        #region "Get Tasks"
        public IHttpActionResult GetMyTasksWeb(string EmpId, string clientId, int? departmentId, string taskType, string priority, string status, int? page, int? count, string sortCol, string sortDir)
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var myTasks = db.GetMyTasksWeb(EmpId, clientId, departmentId, taskType, priority, status, page, count, sortCol, sortDir).OrderByDescending(x => x.CreatedDate).ToList();
                    //  var summaryReport = db.TaskStatsGet(User.Identity.GetUserId()).FirstOrDefault();
                    var summaryReport = db.TaskStatsGet(EmpId).FirstOrDefault();

                    int totalCount = 0;
                  
                    if (myTasks.Count() > 0)
                    {
                        totalCount = (int)myTasks.FirstOrDefault().TotalCount;
                    }
                    ActivitiesSummary actSummaryTasksOnMe = new ActivitiesSummary();
                  
                    actSummaryTasksOnMe.PriorityHighJobs = summaryReport.HighMy;
                    actSummaryTasksOnMe.PriorityMediumJobs = summaryReport.MediumMy;
                    actSummaryTasksOnMe.PriorityLowJobs = summaryReport.LowMy;
                    actSummaryTasksOnMe.TotalJobs = actSummaryTasksOnMe.PriorityHighJobs + actSummaryTasksOnMe.PriorityMediumJobs + actSummaryTasksOnMe.PriorityLowJobs;

                    return Content(HttpStatusCode.OK, new { myTasks , actSummaryTasksOnMe, totalCount });
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Invalid Request");
            }
        }
        #endregion

        #region "Get to do pending task list"
        public IHttpActionResult GetToDoPendingTasks(string EmpId, string clientId, int? departmentId, string taskType, string priority, string status, int? page, int? count, string sortCol, string sortDir)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    int totalCount = 0;
                    var myPendingTasks = db.GetToDoPendingTasks(EmpId, clientId, departmentId, taskType, priority, status, page, count, sortCol, sortDir).OrderByDescending(x => x.CreatedDate).ToList();
                 //   var summaryReport = db.TaskStatsGet(User.Identity.GetUserId()).FirstOrDefault();
                    var summaryReport = db.TaskStatsGet(EmpId).FirstOrDefault();

                    ActivitiesSummary actSummary = new ActivitiesSummary();
                  
                    actSummary.PriorityHighJobs = summaryReport.HighMyPending;
                    actSummary.PriorityMediumJobs = summaryReport.MediumMyPending;
                    actSummary.PriorityLowJobs =summaryReport.LowMyPending;

                    actSummary.TotalJobs = actSummary.PriorityHighJobs + actSummary.PriorityMediumJobs + actSummary.PriorityLowJobs;

                    if (myPendingTasks.Count() > 0)
                    {
                        totalCount = (int)myPendingTasks.FirstOrDefault().TotalCount;
                    }
                    
                    return Content(HttpStatusCode.OK, new { myPendingTasks, totalCount, actSummary });
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
                    var tasksByMe = db.GetTasksByMe(EmpId, clientId, departmentId, taskType, priority, status, page, count, sortCol, sortDir).OrderByDescending(x => x.CreatedDate).ToList();
                    //var summaryReport = db.TaskStatsGet(User.Identity.GetUserId()).FirstOrDefault();
                    var summaryReport = db.TaskStatsGet(EmpId).FirstOrDefault();

                    if (tasksByMe.Count()>0)
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
                    var tasksThroughMe = db.GetTasksThroughMe(EmpId, clientId, departmentId, taskType, priority, status, page, count, sortCol, sortDir).OrderByDescending(x=>x.CreatedDate).ToList();
                    // var summaryReport = db.TaskStatsGet(User.Identity.GetUserId()).FirstOrDefault();
                    var summaryReport = db.TaskStatsGet(EmpId).FirstOrDefault();

                    if (tasksThroughMe.Count>0)
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
                    activity.CreatedBy = taskDetails.CreatedBy;
                   // activity.CreatedBy = taskDetails.CreatedByName;
                    activity.TaskOwner = taskDetails.TaskOwner;
                    activity.TaskType = taskDetails.TaskType;
                    activity.Category = taskDetails.Category;
                    activity.SubCategory = taskDetails.SubCategory;
                    activity.Quantity = taskDetails.Quantity;
                    activity.EDOC = taskDetails.EDOC;
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
                        activity.Client = taskDetails.Client;
                        activity.Location = taskDetails.ProjectLocation;
                        activity.Project = taskDetails.Project;
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
                    var user = User.Identity.GetUserId();
                    var Notifications = db.ActivityNotifications.Where(x => x.ActivityId == taskId && x.EmployeeId == user).FirstOrDefault();
                    
                    List<ActivityLog> taskLog = new List<ActivityLog>();

                    for(int i=0; i<activityLog.Count; i++)
                    {
                        ActivityLog al = new ActivityLog();
                        
                        al.Description = activityLog[i].Description;
                        al.AssignedBy = activityLog[i].AssignedBy;
                        al.AssignedTo = activityLog[i].AssignedTo;
                        al.Id = activityLog[i].ActivityLogId;
                        al.AssignedById = activityLog[i].AssignedById;
                        al.AssignedToId = activityLog[i].AssignedToId;

                        var atchmts = db.ActivityAttachments.Where(x => x.ActivityId == al.Id).ToList();

                        al.Attachments = atchmts.Select(x => x.AttachmentURL).ToArray();
                        al.Status = activityLog[i].Status;
                        al.TaskDate = activityLog[i].TaskDate;
                        al.HoursWorked = activityLog[i].HoursWorked;
                        al.QuantityWorked = activityLog[i].QuantityWorked;

                        taskLog.Add(al);
                    }

                    if (Notifications != null)
                    {
                        db.ActivityNotifications.Remove(Notifications);
                        db.Entry(Notifications).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
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
        
        public IHttpActionResult GetTaskDetail(string taskId, string employeeId)
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var taskCompleteInfo = db.GetTaskDetail(taskId, employeeId).ToList();
                    var taskDetail = taskCompleteInfo.GroupBy(x => x.TaskId).Select(x => x.First()).Select(x => new
                     {
                        x.TaskId,
                        x.Category,
                        x.SubCategory,
                        x.Client,
                        x.CreatedById,
                        x.CreatedBy,
                        x.CreatedDate,
                        x.Department,
                        x.TaskOwner,
                        x.TaskOwnerId,
                        x.EDOC,
                        x.Priority,
                        x.Subject,
                        x.Description,
                        x.Notifications,
                        x.CompletedDate,
                        x.Quantity,
                        x.Status,
                        x.TaskType,
                        Project = x.OpportunityName,
                        x.ProjectLocation,
                        x.Points,
                        TaskLog= taskCompleteInfo.Where(y=>y.TaskId == x.TaskId).GroupBy(y=>y.TaskLogId).Select(y=>y.First()).Select(y=>new {
                                 y.TaskLogId,
                                 y.TaskLogDate,
                                 y.TaskLogAssignedToId,
                                 y.TaskLogAssignedTo,
                                 y.TaskLogAssignedById,
                                 y.TaskLogAssignedBy,
                                 y.HoursWorked,
                                 y.QuantityWorked,
                                 y.TaskLogDescription,
                                 y.TaskLogStatus,
                                 Attachments= taskCompleteInfo.Where(z => z.TaskLogId == y.TaskLogId).GroupBy(z=>z.AtchmentId).Select(z => z.First()).Select(z=>new
                                 {
                                     z.AtchmentId,
                                     z.AttachmentURL
                                 }).ToList()
                        }).ToList()
                    }).FirstOrDefault();

                    if (taskDetail != null)
                    {
                        if(taskDetail.Notifications > 0)
                        {
                            var Notification = db.ActivityNotifications.Where(x => x.ActivityId == taskDetail.TaskId && x.EmployeeId == employeeId).FirstOrDefault();
                            if (Notification != null)
                            {
                                db.ActivityNotifications.Remove(Notification);
                                db.Entry(Notification).State = System.Data.Entity.EntityState.Deleted;
                                db.SaveChanges();
                            } 
                        }
                    }
                    return Content(HttpStatusCode.OK, new { taskDetail });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occureed, please try again later");
            }
        }

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

                    var activitylogInfo = db.ActivitiesLogs.Where(x => x.TaskId == taskId &&  ((x.AssignedBy == userId && x.Status!="Pending") || (x.AssignedTo== userId && x.Status=="Pending"))).OrderByDescending(x=>x.TaskDate).ToList();

                    if (activitylogInfo.Count > 0)
                    {
                        activitylog.TaskId = activitylogInfo[0].TaskId;
                        activitylog.TaskDate = activitylogInfo[0].TaskDate;
                        activitylog.StartDate = activitylogInfo[0].StartDate;
                        activitylog.EndDate = activitylogInfo[0].EndDate;
                        activitylog.TotalHoursWorked = activitylogInfo.Sum(x => x.HoursWorked);
                        activitylog.BudgetedHours = activitylogInfo[0].BudgetedHours;
                        activitylog.TotalQuantityWorked = activitylogInfo.Sum(x => x.QuantityWorked); 
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

        #region "Get Task details of a row"
        [HttpGet]
        public IHttpActionResult GetTask(string userId, string clientId, int? departmentId, string taskType, string priority, string status, int? page,int? count, string sortCol, string sortDir, string taskCategory)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    if(taskCategory == "ByMe")
                    {
                        var reqTask = db.GetTasksByMe(userId, clientId, departmentId, taskType, priority, status, page, count, sortCol, sortDir).FirstOrDefault();
                        return Content(HttpStatusCode.OK, new { reqTask });
                    }
                    else if(taskCategory== "ThroughMe")
                    {
                        var reqTask= db.GetTasksThroughMe(userId, clientId, departmentId, taskType, priority, status, page, count, sortCol, sortDir).FirstOrDefault();
                        return Content(HttpStatusCode.OK, new { reqTask });
                    }
                    else if(taskCategory == "ToDo")
                    {
                        var reqTask= db.GetMyTasksWeb(userId, clientId, departmentId, taskType, priority, status, page, count, sortCol, sortDir).FirstOrDefault();
                        return Content(HttpStatusCode.OK, new { reqTask });
                    }
                    else if(taskCategory == "ToDoPending")
                    {
                        var reqTask = db.GetToDoPendingTasks(userId, clientId, departmentId, taskType, priority, status, page, count, sortCol, sortDir).FirstOrDefault();
                        return Content(HttpStatusCode.OK, new { reqTask });
                    }
                    else
                    {
                        return Content(HttpStatusCode.OK, new {  });
                    }
                   
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.OK, "An error occured, please try again later");
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

                    activitiesSummary.HighToDoPending = (int)summaryReport.HighMyPending;
                    activitiesSummary.MediumToDoPending = (int)summaryReport.MediumMyPending;
                    activitiesSummary.LowToDoPending = (int)summaryReport.LowMyPending;
                    activitiesSummary.TotalToDoPending = (int)(summaryReport.HighMyPending + summaryReport.MediumMyPending+ summaryReport.LowMyPending); 

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
         
        #region Mark as unread"
        [HttpPost]
        public IHttpActionResult AddNotification(string taskId)
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    ActivityNotification notification = new ActivityNotification();
                    notification.ActivityId = taskId;
                    notification.EmployeeId = User.Identity.GetUserId();
                    db.ActivityNotifications.Add(notification);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch(Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

          
        public bool AddChildTask(Activity activity, Assignee assignee, List<TaskLog> parentLog)
        {
            try
            {
                using(MaxMasterDbEntities db= new MaxMasterDbEntities())
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
                    if(parentLog.Count()>0)
                    {
                        for (int i = 0; i < parentLog.Count(); i++)
                        {
                            ActivitiesLog Log = new ActivitiesLog();

                            Log.Status = parentLog[i].TaskLogStatus;
                            Log.Description = parentLog[i].TaskLogDescription;
                            Log.TaskDate = Convert.ToDateTime(parentLog[i].TaskLogDate);
                            Log.AssignedBy = parentLog[i].TaskLogAssignedById;
                            Log.AssignedTo = parentLog[i].TaskLogAssignedToId;
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
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return false;
            }
        
        }

    }
}




