using MaxMaster.Models;
using MaxMaster.ViewModels;
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
    public class StockController : ApiController
    {

        public static string RandomString(int length)
        {
            Random generator = new Random();
            var r = generator.Next(1000, 10000).ToString();
            gotoRandom:
            using (MaxMasterDbEntities db = new MaxMasterDbEntities())
            {
                var prefix = "MAX-";
                var taskId = prefix + r;
                var exists = db.Activities.Where(x => x.TaskId == taskId).FirstOrDefault();
                if (exists != null)
                {
                    goto gotoRandom;
                }
                return taskId;
            }
        }

        #region  "Add material request to database"
        [HttpPost]
        public IHttpActionResult AddMaterialRequest()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var items = JsonConvert.DeserializeObject<List<BilledItemsModel>>(form.Get("items"));
                    var emp = db.Employees.Where(x => x.Role_Id == "8").FirstOrDefault();
                    var category = db.Categories.Where(x => x.Name == "Stock").FirstOrDefault().Id;
                    var subCategory = db.SubCategories.Where(x => x.Name == "Stock").FirstOrDefault().Id;


                    StockRequest stockReq = new StockRequest();

                    stockReq.Employee = User.Identity.GetUserId();
                    stockReq.Client = form.Get("Client");
                    stockReq.ExpectedStockDate = Convert.ToDateTime(form.Get("expectedDate"));
                    stockReq.Project = Convert.ToInt32(form.Get("project"));
                    stockReq.Status = "Under Review";
                    stockReq.Notes = form.Get("notes");
                    stockReq.RequestDate = DateTime.Now;
                    stockReq.LastUpdated = DateTime.Now;
                    stockReq.UpdatedBy = User.Identity.GetUserId();

                    for (int i = 0; i < items.Count(); i++)
                    {
                        StockRequestMapping srm = new StockRequestMapping();
                        srm.ItemId = items[i].ModelId;
                        srm.Quanitity = items[i].Quantity;

                        stockReq.StockRequestMappings.Add(srm);
                    }

                    Activity act = new Activity();


                    if (emp != null)
                    {
                        act.AssignedTo = emp.AspNetUserId;
                    }

                    act.CreatedBy = User.Identity.GetUserId();
                    act.Status = "Open";
                    act.Subject = "Material request for project";
                    act.Project_Id = Convert.ToInt32(form.Get("project"));
                    act.EDOC = Convert.ToDateTime(form.Get("expectedDate"));
                    act.Client_Id = form.Get("Client");
                    act.Priority = 0;
                    act.CreatedDate = DateTime.Now;
                    act.TaskType = "Client";
                    act.TaskOwner = act.AssignedTo;
                    act.Description = form.Get("notes");
                    act.TaskId = RandomString(4);
                    stockReq.TaskId = act.TaskId;
                    act.Category_Id = Convert.ToInt32(category);
                    act.SubCategory_Id = Convert.ToInt32(subCategory);

                    ActivitiesLog activityLog = new ActivitiesLog();
                    activityLog.Status = act.Status;
                    activityLog.Description = act.Description;
                    activityLog.TaskDate = MasterDataController.GetIndianTime(DateTime.UtcNow);
                    activityLog.AssignedBy = act.CreatedBy;
                    activityLog.AssingedTo = act.TaskOwner;

                    act.ActivitiesLogs.Add(activityLog);

                    db.Activities.Add(act);

                    db.StockRequests.Add(stockReq);
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

        #region "Edit stock requested"
        [HttpPost]
        public IHttpActionResult EditStockRequest(int StockId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {

                    var form = HttpContext.Current.Request.Form;
                    var ItemsRequired = JsonConvert.DeserializeObject<List<BilledItemsModel>>(form.Get("items"));
                    var status = form.Get("status");
                    var task = form.Get("taskId");
                    var previouslyRequestedItems = db.StockRequestMappings.Where(x => x.StockReqId == StockId).ToList();
                    /* Get the stock through id */
                    StockRequest stockRequest = db.StockRequests.Find(StockId);

                    stockRequest.Client = form.Get("Client");
                    stockRequest.ExpectedStockDate = Convert.ToDateTime(form.Get("expectedDate"));
                    stockRequest.Project = Convert.ToInt32(form.Get("project"));
                    stockRequest.Status = form.Get("status");
                    stockRequest.Notes = form.Get("notes");
                    stockRequest.LastUpdated = DateTime.Now;
                    stockRequest.UpdatedBy = User.Identity.GetUserId();

                    if (status == "Approved")
                    {
                        foreach (var item in previouslyRequestedItems)
                        {
                            db.StockRequestMappings.Remove(item);
                            db.SaveChanges();
                        }

                        /* Add requested items to stock */
                        for (int i = 0; i < ItemsRequired.Count(); i++)
                        {
                            StockRequestMapping srm = new StockRequestMapping();
                            srm.ItemId = ItemsRequired[i].ModelId;
                            srm.Quanitity = ItemsRequired[i].Quantity;

                            stockRequest.StockRequestMappings.Add(srm);
                        }
                    }

                    /* Find task with the taskid  */
                    Activity activity = db.Activities.Find(task);
                    ActivitiesLog activityLog = new ActivitiesLog();

                    if (status == "Approved")
                    {
                        /* Get Stock Manager and assign activity to manager from super admin */
                        var stockManager = db.StockManagers.Where(x => x.IsActive == true).FirstOrDefault();

                        for (int j = 0; j < ItemsRequired.Count(); j++)
                        {
                            int quantity = ItemsRequired[j].Quantity;
                            int modelId = ItemsRequired[j].ModelId;

                            /* Get items with model id based on manufactured date */

                            var itemsAvailable = db.Items.Where(x => x.ItemModelId == modelId && x.IsAvailable == true).OrderBy(x => x.StockInDate).ToList();
                            if (itemsAvailable.Count() >= ItemsRequired[j].Quantity)
                            {
                                for (int k = 0; k < ItemsRequired[j].Quantity; k++)
                                {
                                    Item item = db.Items.Find(itemsAvailable[k].Id);

                                    item.ProjectId = Convert.ToInt32(form.Get("project"));
                                    item.IsAvailable = false;

                                    db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                }
                            }
                        }

                        if (stockManager != null)
                        {
                            activity.Status = "pending";
                            activity.TaskOwner = stockManager.AspNetUserId;

                            activityLog.TaskDate = DateTime.Now;
                            activityLog.AssignedBy = User.Identity.GetUserId();
                            activityLog.AssingedTo = stockManager.AspNetUserId;
                            activityLog.Description = "Items for the project is approved. Dispatch stock as early as possible";
                            activityLog.Status = "pending";

                            activity.ActivitiesLogs.Add(activityLog);
                            db.Entry(activity).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    else if (status == "Declined")
                    {
                        activity.Status = "Resolved";
                        activity.TaskOwner = stockRequest.Employee;

                        activityLog.TaskDate = DateTime.Now;
                        activityLog.AssignedBy = User.Identity.GetUserId();
                        activityLog.AssingedTo = activity.CreatedBy;
                        activityLog.Status = "Resolved";
                        activityLog.Description = "Your request for stock is declined";

                        activity.ActivitiesLogs.Add(activityLog);
                        db.Entry(activity).State = System.Data.Entity.EntityState.Modified;
                    }

                    else if (status == "Dispatched")
                    {
                        for (int j = 0; j < ItemsRequired.Count(); j++)
                        {
                            int quantity = ItemsRequired[j].Quantity;
                            int modelId = ItemsRequired[j].ModelId;
                            var stock = db.Items.Where(x => x.IsAvailable == false && x.StockOutDate == null).OrderBy(x => x.StockInDate).ToList();
                            if (stock.Count() >= ItemsRequired[j].Quantity)
                            {
                                for (int k = 0; k < ItemsRequired[j].Quantity; k++)
                                {
                                    Item item = db.Items.Find(stock[k].Id);

                                    item.ProjectId = Convert.ToInt32(form.Get("project"));
                                    item.IsAvailable = false;
                                    item.StockOutDate = DateTime.Now;

                                    db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                }

                                activity.TaskOwner = activity.CreatedBy;
                                activity.Status = "Resolved";

                                activityLog.TaskDate = DateTime.Now;
                                activityLog.AssignedBy = User.Identity.GetUserId();
                                activityLog.AssingedTo = activity.CreatedBy;
                                activityLog.Status = "Resolved";
                                activityLog.Description = "Your items were dispatched successfully ";
                                activityLog.HoursWorked = Convert.ToInt32(form.Get("hoursWorked"));

                                activity.ActivitiesLogs.Add(activityLog);
                                db.Entry(activity).State = System.Data.Entity.EntityState.Modified;

                            }
                            else
                            {
                                stockRequest.Status = form.Get("status");
                                return Content(HttpStatusCode.InternalServerError, "Requested stock is not available to dispatch");
                            }
                        }
                    }

                    db.Entry(stockRequest).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
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

        #region "Get all the stock requests based on role"
        [HttpGet]
        public IHttpActionResult GetStockRequests(string client, int? project, DateTime? fromDate, DateTime? toDate, string status, int? page, int? count)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var user = "";
                    int? TotalCount = 0;
                    var employeeId = User.Identity.GetUserId();
                    var emp = db.Employees.Where(x => x.AspNetUserId == employeeId).FirstOrDefault();
                    if (emp != null)
                    {
                        if (emp.Role_Id != "8")
                        {
                            user = User.Identity.GetUserId();
                        }
                        else
                        {
                            user = null;
                        }
                    }
                    else
                    {
                        user = null;
                    }
                    var stockRequests = db.GetStockRequests(user, client, project, fromDate, toDate, status, page, count).ToList();

                    var stockMangers = db.StockManagers.ToList();

                    var exists = stockMangers.Find(x => x.AspNetUserId == user);

                    if (exists != null)
                    {
                        var otherRequests = db.GetStockRequests(null, client, project, fromDate, toDate, "Approved", page, count).ToList();

                        for (int i = 0; i < otherRequests.Count(); i++)
                        {
                            stockRequests.Add(otherRequests[i]);
                        }
                    }

                    if (stockRequests.Count() > 0)
                    {
                        TotalCount = stockRequests.FirstOrDefault().TotalCount;
                    }
                    return Content(HttpStatusCode.OK, new { stockRequests, TotalCount });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region "Get stock request for edit"
        [HttpGet]
        public IHttpActionResult GetStockRequest(int stockReqId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var user = User.Identity.GetUserId();
                    var emp = db.Employees.Where(x => x.AspNetUserId == user).FirstOrDefault();

                    /* Stored procedure to get the items requested */

                    var stockRequest = db.GetStockRequest(stockReqId).FirstOrDefault();

                    var stockRequestItems = db.GetStockRequest(stockReqId).GroupBy(x => x.ModelId).Select(x => x.First()).Select(x => new
                    {
                        ModelId = x.ModelId,
                        ItemName = x.Model,
                        Quantity = x.Quanitity,
                        Description = x.Description,
                        StockReqMappingId = x.StockReqMappingId,
                        x.NoOfItemsAvailable
                    }).ToList();

                    if (emp != null)
                    {
                        if (emp.Role_Id == "8")
                        {
                            stockRequest.ShortName = stockRequest.ShortName + " ( " + emp.Organisation.OrgName + " )";
                        }
                        else
                        {
                            stockRequest.ShortName = stockRequest.ShortName;
                        }
                    }


                    var location = db.ClientLocations.Where(x => x.Id == stockRequest.Location_Id).FirstOrDefault();

                    if (location != null)
                    {
                        stockRequest.ProjectLocation = stockRequest.OpportunityName + " ( " + location.AddressLine1 + "," +
                         location.City.Name + "," + location.State.Name + "," + location.Country.Name + ","
                           + location.ZIP + " )";
                    }

                    return Content(HttpStatusCode.OK, new { stockRequest, stockRequestItems });
                }

            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region "Get list of stock managers"
        [HttpGet]
        public IHttpActionResult GetStockManagers()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var stockManagers = db.StockManagers.Where(x => x.IsActive == true).Select(x => new
                    {
                        x.AspNetUserId,
                        x.IsActive
                    }).ToList();
                    return Content(HttpStatusCode.OK, new { stockManagers });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        //#region "Get stock Report"
        //[HttpGet]
        //public IHttpActionResult GetStockReport(string clientId, int? projectId, int? page, int? count)
        //{
        //    try
        //    {
        //        using (MaxMasterDbEntities db = new MaxMasterDbEntities())
        //        {
        //            var report = db.GetStockReport(clientId, projectId, page, count).ToList();
        //            var projects = report.GroupBy(x => x.Project).ToList();

        //            List<StockReport> stockReport = new List<StockReport>();

        //            if (report.Count() > 0)
        //            {
        //                for (int i = 0; i < projects.Count(); i++)
        //                {
        //                    var stkproject = projects[i].FirstOrDefault();
        //                    StockReport StkReport = new StockReport

        //                    {
        //                        Client = stkproject.ShortName,
        //                        ClientId = stkproject.Client,
        //                        ProjectId = stkproject.Project,
        //                        Project = stkproject.OpportunityName,
        //                        Status = stkproject.Status,
        //                        Models = report.Where(x => x.Project == stkproject.Project).GroupBy(x => x.ItemModelId).Select(x => x.First()).Select(x => new ProjectItemModel
        //                        {
        //                            ModelId = x.ItemModelId,
        //                            ModelName = x.ModelName,
        //                            Items = report.Where(y => y.Project == stkproject.Project && y.ItemModelId == x.ItemModelId).Select(y => new ItemDetails
        //                            {
        //                                SerialNumber = y.SerialNumber,
        //                                MacAddress = y.MACAddress,
        //                                ManufacturedDate = y.ManufacturedDate
        //                            }).ToList(),
        //                            TotalItems = report.Where(y => y.Project == stkproject.Project && y.ItemModelId == x.ItemModelId).Count()

        //                        }).ToList(),

        //                    };

        //                    stockReport.Add(StkReport);
        //                }
        //            }

        //            return Content(HttpStatusCode.OK, new { stockReport });
        //        }

        //    }

        //    catch (Exception ex)
        //    {
        //        new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
        //        return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
        //    }
        //}
        //#endregion

        #region "Get Dispatched stock Report"
        public IHttpActionResult GetDispatchedStock(string clientId, int? project, DateTime? fromDate, DateTime? toDate, int? page, int? count)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var dispatchedStock = db.GetDispatchedStockReport(clientId, project, fromDate, toDate, page, count).ToList();

                    return Content(HttpStatusCode.OK, new { dispatchedStock });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured please try again later");
            }
        }
        #endregion

        #region "Get Dispatched Stock Details"
        public IHttpActionResult GetDispatchedStockDetails(int projectId, int itemId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var stock = db.Items.Where(x => x.ProjectId == projectId && x.ItemModelId == itemId).ToList();
                    List<ItemDetails> items = new List<ItemDetails>();

                    for (int i = 0; i < stock.Count(); i++)
                    {
                        ItemDetails item = new ItemDetails();
                        item.SerialNumber = stock[i].SerialNumber;
                        item.MacAddress = stock[i].MACAddress;
                        item.StockOutDate = stock[i].StockOutDate;
                        item.ManufacturedDate = stock[i].ManufacturedDate;

                        items.Add(item);
                    }
                    return Content(HttpStatusCode.OK, new { items });
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