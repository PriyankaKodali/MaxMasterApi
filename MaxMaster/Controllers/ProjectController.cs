using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using MaxMaster.Models;
using System.IO;
using System.Configuration;

namespace MaxMaster.Controllers
{
    public class ProjectController : ApiController
    {
    //    [HttpPost]
    //public IHttpActionResult AddProject()
    //    {
    //        try
    //        {

    //            using (MaxMasterDbEntities db = new MaxMasterDbEntities())
    //            {
    //                var form = HttpContext.Current.Request.Form;

    //                Project project = new Project();
    //                project.Client_Id = Convert.ToInt32(form.Get("clientId"));
    //                project.ProjectName = form.Get("projectName");
    //                project.Description = form.Get("description");
    //                project.EDOS = DateTime.Parse(form.Get("edos"));
    //                project.EDOC = DateTime.Parse(form.Get("edoc"));
    //                project.FirstName = form.Get("firstName");
    //                project.LastName = form.Get("lastName");
    //                project.PrimaryPhone = form.Get("primaryPhone");
    //                project.SecondaryPhone = form.Get("secondaryPhone");
    //                project.EmailId = form.Get("email");
    //                project.Status = form.Get("status");
    //                project.AddressLine1 = form.Get("addressLine1");
    //                project.AddressLine2 = form.Get("addressLine2");
    //                project.Country_Id = Convert.ToInt32(form.Get("countryId"));
    //                project.City_Id = Convert.ToInt32(form.Get("cityId"));
    //                project.State_Id = Convert.ToInt32(form.Get("stateId"));
    //                project.Landmark = form.Get("landMark");
    //                project.ZIP = form.Get("zip");

    //               // var files = HttpContext.Current.Request.Files;
                
    //           //     var purchaseorderFiles = new List<HttpPostedFile>();
    //           //     var quotationFiles = new List<HttpPostedFile>();
                 
    //                //for (int i = 0; i < files.Count; i++)
    //                //{
    //                //    if (files.GetKey(i).Contains("po-"))
    //                //    {
    //                //        purchaseorderFiles.Add(files[i]);
    //                //    }
    //                //    if(files.GetKey(i).Contains("quotations-"))
    //                //    {
    //                //        quotationFiles.Add(files[i]);
    //                //    }
    //                //}
                    
    //                // if(quotationFiles.Count >0)
    //                //{
    //                //    foreach(var file in quotationFiles)
    //                //    {
    //                //        var fileDirecory = HttpContext.Current.Server.MapPath("~/Projects/Quotations");

    //                //        if(!Directory.Exists(fileDirecory))
    //                //          {
    //                //            Directory.CreateDirectory(fileDirecory);
    //                //          }

    //                //        var fileName = file.FileName;
    //                //        var filePath = Path.Combine(fileDirecory, fileName);
    //                //        file.SaveAs(filePath);

    //                //        ProjectQuotation pq = new ProjectQuotation();
    //                //        pq.FileName = fileName;
    //                //        pq.AttachmentURL = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "Projects", "Quotations", fileName);

    //                //        project.ProjectQuotations.Add(pq);
    //                //    }
                      
    //                //}

    //                // if(purchaseorderFiles.Count> 0)
    //                //{
    //                //    foreach(var po in purchaseorderFiles)
    //                //    {
    //                //        var fileDirectory = HttpContext.Current.Server.MapPath("~/Projects/POs");
    //                //        if(!Directory.Exists(fileDirectory))
    //                //        {
    //                //            Directory.CreateDirectory(fileDirectory);
    //                //        }

    //                //        var fileName = po.FileName;
    //                //        var filePath = Path.Combine(fileDirectory, fileName);
    //                //        po.SaveAs(filePath);

    //                //        ProjectPOs purchaseorder = new ProjectPOs();
    //                //        purchaseorder.FileName = fileName;
    //                //        purchaseorder.AttachmentURL = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "Projects", "POs", fileName);
    //                //        project.ProjectPOs.Add(purchaseorder);

    //                //    }
    //                //}

    //                db.Projects.Add(project);
    //                db.SaveChanges();

    //                }
    //             return Ok();
    //        }
    //        catch(Exception ex)
    //        {
    //            new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
    //            return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
    //        }
    //    }

        //[HttpGet]
        //public IHttpActionResult GetProjects(string projectName, int? client, string email, string projectHolder, string phoneNum, int? page, int? count, string sortCol, string sortDir)
        //{
        //    try
        //    {
        //        using (MaxMasterDbEntities db = new MaxMasterDbEntities())
        //        {
        //            int? totalCount = 0;
        //            var projectsList = db.GetProjects(projectName, client, email, projectHolder, phoneNum, page, count, sortCol, sortDir).ToList();

        //            if(projectsList.Count>0)
        //            {
        //                totalCount = projectsList.FirstOrDefault().TotalCount;
        //            }

        //            return Content(HttpStatusCode.OK, new { projectsList, totalCount });
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
        //        return Content(HttpStatusCode.InternalServerError, "An Error occured, please try again later");
        //    }
        //}
    }
}