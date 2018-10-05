using MaxMaster.Models;
using MaxMaster.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace MaxMaster.Controllers
{
    public class AllocationsController : ApiController
    {
        [HttpPost]
        public IHttpActionResult AddDefaultAllocation()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;

                    DefaultAllocation alloc = new DefaultAllocation();
                    alloc.Client_Id = Convert.ToInt32(form.Get("Client"));
                    alloc.Doctor_Id = Convert.ToInt32(form.Get("Doctor"));
                    alloc.Employee_Id = Convert.ToInt32(form.Get("Employee"));
                    alloc.JobLevel = form.Get("JobLevel");
                    alloc.LastModified = DateTime.Now;
                    alloc.Accuracy = Convert.ToInt32(form.Get("Accuracy"));
                    alloc.Pages = Convert.ToInt32(form.Get("NoOfPages"));
                    alloc.Inactive = false;

                    db.DefaultAllocations.Add(alloc);
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

        [HttpGet]
        public IHttpActionResult GetDefaultAllocation(int? ClientId, int? DoctorId, int? EmployeeId, string JobLevel, int page, int count, string SortCol, string SortDir)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var defaultAllocations = db.GetDefaultAllocations(ClientId, DoctorId, EmployeeId, JobLevel, page, count, SortCol, SortDir).ToList();

                    int totalCount = 0;

                    if (defaultAllocations.Count > 0)
                    {
                        totalCount = (int)defaultAllocations.FirstOrDefault().TotalCount;
                    }
                    return Content(HttpStatusCode.OK, new { defaultAllocations, totalCount });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetDefaultEmp(int defaultAllocId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    DefaultAllocation da = await db.DefaultAllocations.FindAsync(defaultAllocId);

                    var defaultAlloc = new DefaultAllocationsModel();
                    defaultAlloc.Client_Id = da.Client_Id;
                    defaultAlloc.Client = da.Client.ShortName;
                    defaultAlloc.Doctor_Id = da.Doctor_Id;
                    defaultAlloc.Doctor = da.Doctor.FirstName + " " + da.Doctor.LastName;
                    defaultAlloc.Employee_Id = da.Employee_Id;
                    defaultAlloc.Employee = da.Employee.FirstName + " " + da.Employee.LastName;
                    defaultAlloc.NoOfPages = da.Pages;
                    defaultAlloc.Accuracy = da.Accuracy;
                    defaultAlloc.JobLevel = da.JobLevel;
                    defaultAlloc.Inactive = da.Inactive;

                    return Content(HttpStatusCode.OK, new { defaultAlloc });

                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An errror occured, please try again later");
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> UpdateDefaultAlloc(int defaultAllocId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;

                    DefaultAllocation defaultAlloc = db.DefaultAllocations.Find(defaultAllocId);

                    defaultAlloc.Client_Id = Convert.ToInt32(form.Get("Client"));
                    defaultAlloc.Doctor_Id = Convert.ToInt32(form.Get("Doctor"));
                    defaultAlloc.Employee_Id = Convert.ToInt32(form.Get("Employee"));
                    defaultAlloc.JobLevel = form.Get("JobLevel");
                    defaultAlloc.Pages = Convert.ToInt32(form.Get("NoOfPages"));
                    defaultAlloc.Accuracy = Convert.ToInt32(form.Get("Accuracy"));
                    defaultAlloc.LastModified = DateTime.Now;
                    defaultAlloc.Inactive = Convert.ToBoolean(form.Get("IsInActive"));

                    db.Entry(defaultAlloc).State = System.Data.Entity.EntityState.Modified;
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

    }
}