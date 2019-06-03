using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using Microsoft.AspNet.Identity;
using MaxMaster.Models;

namespace MaxMaster.Controllers
{
    public class ReportsController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetEmployeeDayWiseReport(string empId, string clientId, DateTime fromDate, DateTime toDate, string status, int? priority, string taskId, int? catId)
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var overallReport = db.LogReport(empId, clientId, fromDate, toDate, status, priority, taskId, catId).ToList();
                   // var overallReport = db.EmployeeDayReport(empId, clientId, fromDate, toDate, status, priority, taskId).OrderBy(x=>x.TaskLogDate).ToList();
                    var employeeReport = overallReport.Where(x=>x.TaskLogDate>=fromDate.Date && x.TaskLogDate<=toDate.Date.AddDays(1)&& x.TaskLogAssignedById == empId).Select(x => new
                    {
                        x.TaskId,
                        x.CreatedDate,
                        x.CreatedBy,
                        x.Subject,
                        x.Category,
                        x.Department,
                        x.CompletedDate,
                        x.EDOC,
                        x.Priority,
                        x.Project,
                        x.ProjectLocation,
                        x.Client,
                        x.SubCategory,
                        TaskStatus= x.Status,
                        x.Description,
                        x.TaskOwner,
                        x.TaskType,
                        x.TaskLogDate,
                        x.TaskLogDescription,
                        x.TaskLogStatus, 
                        HoursWorked= x.TaskLogHoursWorked,
                        QuantityWorked= x.TaskLogQuantityWorked,
                        TotalHoursWorked = overallReport.Where(z => z.TaskId == x.TaskId).Sum(z => (decimal?)z.TaskLogHoursWorked) ?? 0,
                        StartAndEndDate = overallReport.Where(z => z.TaskId ==x.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                        TaskLog = overallReport.Where(y => y.TaskId == x.TaskId).GroupBy(y => y.TaskLogId).Select(y => y.First()).Select(y => new
                        {
                            y.TaskLogId,
                            y.TaskLogDate,
                            y.TaskLogAssignedBy,
                            y.TaskLogAssignedTo,
                            y.TaskLogHoursWorked,
                            y.TaskLogQuantityWorked,
                            y.TaskLogDescription,
                            y.TaskLogStatus
                        }).OrderBy(y => y.TaskLogDate).ToList()
                    }).ToList();
                    return Content(HttpStatusCode.OK, new { employeeReport });
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        
        [HttpGet]
        public IHttpActionResult GetMyReport(DateTime? fromDate, DateTime toDate, string client, string status, int? priority, string taskId, int? catId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var employee = User.Identity.GetUserId();
                    var overallReport = db.LogReport(employee, client, fromDate, toDate, status, priority, taskId, catId).OrderBy(x => x.CreatedDate).ToList();
                    var myActivityReport = overallReport.GroupBy(x => x.TaskId).Select(x => x.First()).Select(x => new
                    {
                        x.TaskId,
                        x.CreatedDate,
                        Creator = x.CreatedBy,
                        x.Client,
                        x.Department,
                        x.Project,
                        x.ProjectLocation,
                        x.Quantity,
                        x.Category,
                        x.Priority,
                        x.SubCategory,
                        x.EDOC,
                        CreatedBy = x.CreatedById,
                        TaskOwner = x.TaskOwnerId,
                        x.Status,
                        x.CompletedDate,
                        x.Subject,
                        x.Description,
                        Notifications = 0, 
                        HoursWorked = overallReport.Where(y => y.TaskId == x.TaskId).Sum(y => (decimal?)y.TaskLogHoursWorked) ?? 0,
                        ExpectedStartDate = overallReport.Where(y => y.TaskId == x.TaskId).GroupBy(y => y.TaskLogId).Select(y => y.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                        TaskLog = overallReport.Where(y => y.TaskId == x.TaskId).GroupBy(y => y.TaskLogDate).Select(y => y.First()).Select(y => new {
                            y.TaskLogId,
                            y.TaskLogTaskId,
                            y.TaskLogDate,
                            y.TaskLogDescription,
                            y.TaskLogQuantityWorked,
                            y.TaskLogStartDate,
                            y.TaskLogEndDate,
                            y.TaskLogBudgetedHours,
                            y.TaskLogHoursWorked,
                            y.TaskLogAssignedTo,
                            y.TaskLogAssignedToId,
                            y.TaskLogAssignedBy,
                            y.TaskLogAssignedById,
                            y.TaskLogStatus
                        }).OrderBy(y => y.TaskLogDate).ToList()
                    }).ToList();

                    return Content(HttpStatusCode.OK, new { myActivityReport });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
       
        [HttpGet]
        public IHttpActionResult GetDashBoardData(string empId)
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var statusSummary = db.StatusSummary(empId).ToList();
                    var clientSummary = db.ClientTasksSummary(empId).ToList();
                    var categorySummary = db.CategoriesSummary(empId).ToList();
                    var employeesSummary = db.EmployeeTasksSummary().ToList();
                    var leadsSummary = db.LeadsSummary().ToList();

                    return Content(HttpStatusCode.OK, new { statusSummary, clientSummary, categorySummary, employeesSummary, leadsSummary });
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured,please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetTopPerformances()
        {
            try
            {
                using(MaxMasterDbEntities db= new MaxMasterDbEntities())
                { 
                    var topPerformances = db.TopPerformances().ToList();
                    var monthPerformance = topPerformances.OrderByDescending(x => x.MonthPoints).Take(3).ToList();
                    var pointsInWeek = topPerformances.OrderByDescending(x => x.WeekPoints).FirstOrDefault();
                    
                    var pointsinaDay = topPerformances.OrderByDescending(x => x.DayPoints).FirstOrDefault();
                    if (pointsinaDay != null && pointsInWeek!=null) {
                        var performerOftheDay = topPerformances.Where(x=>x.DayPoints == pointsinaDay.DayPoints).ToList();
                        var performerOftheWeek = topPerformances.Where(x => x.WeekPoints == pointsInWeek.WeekPoints).ToList();
                        return Content(HttpStatusCode.OK, new { topPerformances, monthPerformance, pointsinaDay, pointsInWeek, performerOftheDay, performerOftheWeek });
                    }
                    else if(pointsInWeek != null)  {
                        var performerOftheWeek = topPerformances.Where(x => x.WeekPoints == pointsInWeek.WeekPoints).ToList();
                        return Content(HttpStatusCode.OK, new { topPerformances, monthPerformance, pointsinaDay, pointsInWeek });
                    }
                    else if(pointsinaDay != null) {
                        var performerOftheDay = topPerformances.Where(x=>x.DayPoints == x.DayPoints).ToList();
                        return Content(HttpStatusCode.OK, new { topPerformances, monthPerformance, pointsinaDay, pointsInWeek, performerOftheDay });
                    }
                    else  {
                        return Content(HttpStatusCode.OK, new { topPerformances, monthPerformance, pointsinaDay, pointsInWeek });
                    } 
                } 
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An Error occured,please try again later");
            }
        }

        #region "Employee/(s) Activities Report "
        [HttpGet]
        public IHttpActionResult GetEmployeesReport(string empId, DateTime? fromDate, DateTime? toDate, string clientId, int? priority, string status, string taskId, int? catId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var allEmpTaskData = db.GetEmployeesActivitiesReport(empId, fromDate ,toDate, clientId, status, priority, taskId, catId).ToList();
                    var reportData = allEmpTaskData.GroupBy(x => x.TaskOwnerId).Select(x => x.First()).Select(x => new
                    {
                        x.TaskOwner,
                        x.TaskOwnerId,
                        Task = allEmpTaskData.Where(y => y.TaskOwnerId == x.TaskOwnerId).GroupBy(y => y.TaskId).Select(y => y.First()).Select(y => new
                        {
                            y.TaskId,
                            y.Client,
                            y.Department,
                            y.Category,
                            y.SubCategory,
                            y.Subject,
                            y.Description,
                            y.Priority,
                            y.CreatedDate,
                            y.EDOC,
                            y.CreatedBy,
                            y.CreatedById,
                            y.TaskOwner,
                            y.TaskOwnerId,
                            y.CompletedDate,
                            y.ParentTask_Id,
                            y.Status,
                            y.TaskType,
                            y.AssignedTo,
                            y.AssignedToId,
                            y.Project,
                            y.Quantity,
                            HoursWorked = allEmpTaskData.Where(z => z.TaskId == y.TaskId).Sum(z => (decimal?)z.TaskLogHoursWorked) ?? 0,
                            ActualStartDate = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                            ExpectedStartDate = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                            ExpectedEndDate = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogEndDate != null).FirstOrDefault(),
                            BudgetedHours = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                            TaskLog = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).Select(z => new
                            {
                                z.TaskLogId,
                                z.TaskLogTaskId,
                                z.TaskLogDate,
                                z.TaskLogAssignedTo,
                                z.TaskLogAssignedToId,
                                z.TaskLogAssignedBy,
                                z.TaskLogAssignedById,
                                z.TaskLogBudgetedHours,
                                z.TaskLogStartDate,
                                z.TaskLogEndDate,
                                z.TaskLogHoursWorked,
                                z.TaskLogStatus,
                                z.TaskLogDescription,
                                z.TaskLogQuantityWorked
                            }).OrderBy(z => z.TaskLogDate).ToList()
                        }).ToList()
                    }).ToList();
                    return Content(HttpStatusCode.OK, new { reportData });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occureed, please try again later");
            }
        }
        #endregion

        #region "Client wise Activities Report"
        [HttpGet]
        public IHttpActionResult GetClientWiseReport(string empId, DateTime? fromDate, DateTime? toDate, string clientId, int? priority, string status, string taskId, int? catId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var allEmpTaskData = db.GetEmployeesActivitiesReport(empId, fromDate, toDate, clientId, status, priority, taskId, catId).ToList();
                    var reportData = allEmpTaskData.Where(x => x.Department == null).GroupBy(x => x.Client).Select(x => x.First()).Select(x => new
                    {
                        x.Client,
                        x.Client_Id,
                        Task = allEmpTaskData.Where(y => y.Client_Id == x.Client_Id).GroupBy(y => y.TaskId).Select(y => y.First()).Select(y => new
                        {
                            y.TaskId,
                            y.Client,
                            y.Department,
                            y.Category,
                            y.SubCategory,
                            y.Subject,
                            y.Description,
                            y.Priority,
                            y.CreatedDate,
                            y.EDOC,
                            y.CreatedBy,
                            y.CreatedById,
                            y.TaskOwner,
                            y.TaskOwnerId,
                            y.CompletedDate,
                            y.ParentTask_Id,
                            y.Status,
                            y.TaskType,
                            y.AssignedTo,
                            y.AssignedToId,
                            y.Project,
                            y.Quantity,
                            HoursWorked = allEmpTaskData.Where(z => z.TaskId == y.TaskId).Sum(z => (decimal?)z.TaskLogHoursWorked) ?? 0,
                            ActualStartDate = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                            ExpectedStartDate = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                            ExpectedEndDate = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                            BudgetedHours = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                            TaskLog = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).Select(z => new
                            {
                                z.TaskLogId,
                                z.TaskLogTaskId,
                                z.TaskLogDate,
                                z.TaskLogAssignedTo,
                                z.TaskLogAssignedToId,
                                z.TaskLogAssignedBy,
                                z.TaskLogAssignedById,
                                z.TaskLogBudgetedHours,
                                z.TaskLogStartDate,
                                z.TaskLogEndDate,
                                z.TaskLogHoursWorked,
                                z.TaskLogStatus,
                                z.TaskLogDescription,
                                z.TaskLogQuantityWorked
                            }).OrderBy(z => z.TaskLogDate).ToList()
                        }).ToList()
                    }).ToList();
                    return Content(HttpStatusCode.OK, new { reportData });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region "Log report of employees"
        [HttpGet]
        public IHttpActionResult GetEmployeesLogReport(string empId, string clientId, DateTime fromDate, DateTime toDate, string status, int? priority, string taskId, int? catId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    try
                    {
                        var datestring = Convert.ToDateTime(fromDate.ToShortDateString());

                        var allEmpTaskData = db.LogReport(empId, clientId, fromDate, toDate, status, priority, taskId, catId).ToList();
                        var reportData = allEmpTaskData.GroupBy(x => x.TaskLogAssignedBy).Select(x => x.First()).Select(x => new {
                            x.TaskLogAssignedBy,
                            x.TaskLogAssignedById,
                            Task = allEmpTaskData.Where(y => y.TaskLogAssignedById == x.TaskLogAssignedById).GroupBy(y => y.TaskId).Select(y => y.First()).Select(y => new
                            {
                                y.TaskId,
                                y.Client,
                                y.Department,
                                y.Project,
                                y.ProjectLocation,
                                y.CreatedBy,
                                y.CreatedDate,
                                y.Category,
                                y.SubCategory,
                                y.CompletedDate,
                                y.Subject,
                                y.Status,
                                y.EDOC,
                                y.Priority,
                                y.Description,
                                HoursWorked = allEmpTaskData.Where(z => z.TaskId == y.TaskId).Sum(z => (decimal?)z.TaskLogHoursWorked) ?? 0,
                                ExpectedStartDate = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                                ExpectedEndDate = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                                BudgetedHours = allEmpTaskData.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                                TaskLog = allEmpTaskData.Where(z => z.TaskLogTaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).Select(z => new {
                                    z.TaskLogId,
                                    z.TaskLogTaskId,
                                    z.TaskLogDate,
                                    z.TaskLogAssignedTo,
                                    z.TaskLogAssignedToId,
                                    z.TaskLogAssignedBy,
                                    z.TaskLogAssignedById,
                                    z.TaskLogBudgetedHours,
                                    z.TaskLogStartDate,
                                    z.TaskLogEndDate,
                                    z.TaskLogHoursWorked,
                                    z.TaskLogStatus,
                                    z.TaskLogDescription,
                                    z.TaskLogQuantityWorked
                                }).OrderBy(z => z.TaskLogDate).ToList()
                            }).ToList()
                        }).ToList();

                        return Content(HttpStatusCode.OK, new { reportData });
                    }
                    catch (Exception ex)
                    {
                        new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                        return Content(HttpStatusCode.InternalServerError, "An error occured please try again later");
                    }
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region " Client wise log report"
        [HttpGet]
        public IHttpActionResult GetClientLogReport(string empId, string clientId, DateTime fromDate, DateTime toDate, string status, int? priority, string taskId, int? catId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var overallReport = db.LogReport(empId, clientId, fromDate, toDate, status, priority, taskId, catId).OrderBy(x => x.Client).ToList();
                    var reportData = overallReport.Where(x => x.Department == null).GroupBy(x => x.Client_Id).Select(x => x.First()).Select(x => new {
                        x.Client,
                        x.Client_Id,
                        Task = overallReport.Where(y => y.Client_Id == x.Client_Id).GroupBy(y => y.TaskId).Select(y => y.First()).Select(y => new {
                            y.TaskId,
                            y.CreatedBy,
                            y.Client,
                            y.CreatedDate,
                            y.EDOC,
                            y.TaskOwner,
                            y.TaskOwnerId,
                            y.Category,
                            y.SubCategory,
                            y.Priority,
                            y.Status,
                            y.Description,
                            y.Subject,
                            y.Quantity,
                            y.Project,
                            y.ProjectLocation,
                            y.AssignedTo,
                            y.AssignedToId,
                            HoursWorked = overallReport.Where(z => z.TaskId == y.TaskId).Sum(z => (decimal?)z.TaskLogHoursWorked) ?? 0,
                            ExpectedStartDate = overallReport.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                            ExpectedEndDate = overallReport.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                            BudgetedHours = overallReport.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).ToList().Where(tl => tl.TaskLogStartDate != null).FirstOrDefault(),
                            TaskLog = overallReport.Where(z => z.TaskId == y.TaskId).GroupBy(z => z.TaskLogId).Select(z => z.First()).Select(z => new
                            {
                                z.TaskLogId,
                                z.TaskLogTaskId,
                                z.TaskLogDate,
                                z.TaskLogAssignedTo,
                                z.TaskLogAssignedToId,
                                z.TaskLogAssignedBy,
                                z.TaskLogAssignedById,
                                z.TaskLogBudgetedHours,
                                z.TaskLogStartDate,
                                z.TaskLogEndDate,
                                z.TaskLogHoursWorked,
                                z.TaskLogStatus,
                                z.TaskLogDescription,
                                z.TaskLogQuantityWorked
                            }).OrderBy(z => z.TaskLogDate).ToList()
                        }).ToList()
                    }).ToList();
                    return Content(HttpStatusCode.OK, new { reportData });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion
    }
}