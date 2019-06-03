using System;
using MaxMaster.Models;
using MaxMaster.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using Newtonsoft.Json;


namespace MaxMaster.Controllers
{
    public class CategoryController : ApiController
    {
       [HttpPost]
        public IHttpActionResult AddCategory()
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var subCategories = JsonConvert.DeserializeObject<List<SubCat>>(form.Get("subCategories")).ToList();

                    Category category = new Category();
                    category.Dept_Id = Convert.ToInt32(form.Get("deptId"));
                    category.Name = form.Get("category");

                    if (subCategories.Count() > 0)
                    {
                        foreach(var subCat in subCategories)
                        {
                            SubCategory subCategory = new SubCategory();
                            PointsLog pointslog = new PointsLog();

                            subCategory.Name = subCat.Name;
                            subCategory.Points = subCat.Points;

                            pointslog.Points = subCat.Points;
                            pointslog.FromDate = DateTime.Now;
                            
                            category.SubCategories.Add(subCategory);
                            subCategory.PointsLogs.Add(pointslog); 

                        }
                    }
                    db.Categories.Add(category);
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

       [HttpGet]
        public IHttpActionResult GetDeptWiseCategories()
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var overallCategories = db.GetCategories().ToList();
                    var departmentsInfo = overallCategories.GroupBy(x => x.DepartmentId).Select(x => x.First()).Select(x => new
                    {
                        x.DepartmentId,
                        x.Department,
                        Categories = overallCategories.Where(y => y.DepartmentId == x.DepartmentId).GroupBy(y => y.CategoryId).Select(y => y.First()).Select(y => new
                        {
                            y.CategoryId,
                            y.CategoryName,
                            SubCategories = overallCategories.Where(z => z.CategoryId == y.CategoryId).GroupBy(z => z.SubCategoryId).Select(z => z.First()).Select(z => new
                            {
                                z.SubCategoryId,
                                Name= z.SubCategory,
                                z.Points,
                                z.PointsLogId
                            }).OrderBy(z=>z.Name).ToList()
                        }).OrderBy(y=>y.CategoryName).ToList()
                    }).OrderBy(x=>x.Department).ToList();
                    return Content(HttpStatusCode.OK, new { departmentsInfo });
                }
            }
            catch(Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
       
       [HttpPost]
        public IHttpActionResult EditCategory()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    int catId = Convert.ToInt32(form.Get("catId"));
                    var subCategories = JsonConvert.DeserializeObject<List<NewSubCategory>>(form.Get("subCategories")).ToList();
                    var category = db.Categories.Find(catId);
 
                        foreach(var subCategory in subCategories) 
                        {
                            SubCategory subCat = new SubCategory();
                            PointsLog pointsLog = new PointsLog();

                            if (subCategory.SubCategoryId !=null)
                            {
                                var subcat = db.SubCategories.Find(subCategory.SubCategoryId);
                                if (subcat.Points != subCategory.Points)
                                {
                                    subcat.Points = subCategory.Points;
                                    var subcatPointsLog = db.PointsLogs.Find(subCategory.PointsLogId);

                                    if (subcatPointsLog != null){
                                        subcatPointsLog.ToDate = MasterDataController.GetIndianTime(DateTime.Now);
                                        db.Entry(subcatPointsLog).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    pointsLog.FromDate = MasterDataController.GetIndianTime(DateTime.Now);
                                    pointsLog.Points = subCategory.Points; 
                                    subcat.PointsLogs.Add(pointsLog);  
                                    db.Entry(subcat).State = System.Data.Entity.EntityState.Modified;
                                }
                                 else
                                 {
                                   subcat.Name = subCategory.Name;
                                   subcat.Points = subCategory.Points; 
                                   db.Entry(subcat).State = System.Data.Entity.EntityState.Modified;
                                   }
                            }
                            else
                            {
                                subCat.Name = subCategory.Name;
                                subCat.Points = subCategory.Points;

                                pointsLog.Points = subCategory.Points;
                                pointsLog.FromDate = MasterDataController.GetIndianTime(DateTime.Now); 

                                category.SubCategories.Add(subCat);
                                subCat.PointsLogs.Add(pointsLog);
                            }
                        }   
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
        public IHttpActionResult CheckIfCategoryExists(int? deptId, string name)
        {
            try
            {
                using(MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var category = db.Categories.Where(x => x.Name.ToUpper() == name.ToUpper() && x.Dept_Id==deptId ).FirstOrDefault();
                    return Content(HttpStatusCode.OK, new { Result = category != null });
                }
            }
            catch(Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

    }
}