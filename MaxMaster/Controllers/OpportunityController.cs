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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;


namespace MaxMaster.Controllers
{
    public class OpportunityController : ApiController
    {
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

                string contentImagesFolder = HttpContext.Current.Server.MapPath("~/Images/ContentImages");
                if (!Directory.Exists(contentImagesFolder))
                {
                    Directory.CreateDirectory(contentImagesFolder);
                }

                string fileName = DateTime.Now.Ticks.ToString() + fileExtension;
                string filePath = Path.Combine(contentImagesFolder, fileName);
                file.SaveAs(filePath);
                string fileUrl = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "Images", "ContentImages", fileName);

                return Content(HttpStatusCode.OK, new { link = fileUrl });
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occoured, please try again!");
            }
        }


        #region "Add opportunity"
        [HttpPost]
        public IHttpActionResult AddOpportunity()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;

                    var checkedContacts = JsonConvert.DeserializeObject<List<string>>(form.Get("contactPersons"));
                    var opportunityCategories = JsonConvert.DeserializeObject<List<keyValueModel>>(form.Get("Categories"));

                    Opportunity opp = new Opportunity();

                    opp.AssignedTo = (form.Get("assignTo"));
                    opp.Client_Id = form.Get("client");
                    opp.CreatedDate = MasterDataController.GetIndianTime(DateTime.UtcNow);
                    opp.CreatedBy = User.Identity.GetUserId();
                    opp.TaskOwner = (form.Get("assignTo"));
                    opp.OpportunityName = form.Get("opportunity");
                    opp.RefferedBy = form.Get("refferedBy");
                    opp.Org_Id = Convert.ToInt32(form.Get("orgId"));
                    opp.Status = form.Get("Status");

                    opp.Description = form.Get("comments");
                    opp.Location_Id = Convert.ToInt32(form.Get("Location"));

                    if (opp.Status == "Accepted")
                    {
                        opp.EDOC = DateTime.Parse(form.Get("edoc"));
                        opp.EDOS = DateTime.Parse(form.Get("edos"));

                    }
                    else if (opp.Status == "Completed")
                    {
                        opp.ActualCompletedDate = Convert.ToDateTime(form.Get("CompletedDate"));
                    }

                    db.Opportunities.Add(opp);

                    for (int i = 0; i < opportunityCategories.Count; i++)
                    {
                        OpportunityCategoryMapping oppCat = new OpportunityCategoryMapping();
                        oppCat.Category_Id = Convert.ToInt32(opportunityCategories[i].value);

                        opp.OpportunityCategoryMappings.Add(oppCat);
                    }

                    for (int j = 0; j < checkedContacts.Count; j++)
                    {
                        OpportunityContactMapping oppContact = new OpportunityContactMapping();
                        oppContact.Contact_Id = Convert.ToInt32(checkedContacts[j]);

                        opp.OpportunityContactMappings.Add(oppContact);
                    }

                    OpportunitiesLog oppLog = new OpportunitiesLog();

                    oppLog.CreatedBy = User.Identity.GetUserId();
                    oppLog.CreatedDate = MasterDataController.GetIndianTime(DateTime.UtcNow);
                    oppLog.AssignedTo = opp.TaskOwner;
                    oppLog.Status = form.Get("Status");
                    oppLog.Comments = form.Get("comments");

                    opp.OpportunitiesLogs.Add(oppLog);

                    var files = HttpContext.Current.Request.Files;
                    var fileAttachments = new List<HttpPostedFile>();

                    for (int i = 0; i < files.Count; i++)
                    {
                        fileAttachments.Add(files[i]);
                    }
                    foreach (var file in fileAttachments)
                    {
                        var fileDirecory = HttpContext.Current.Server.MapPath("~/CommentAttachments");

                        if (!Directory.Exists(fileDirecory))
                        {
                            Directory.CreateDirectory(fileDirecory);
                        }

                        var fileName = file.FileName;
                        var filePath = Path.Combine(fileDirecory, fileName);
                        file.SaveAs(filePath);

                        OpportunityAttachment attachment = new OpportunityAttachment();

                        attachment.FileName = Path.GetFileNameWithoutExtension(file.FileName);
                        attachment.FileAttachment = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "CommentAttachments", fileName);
                        oppLog.OpportunityAttachments.Add(attachment);
                    }

                    db.SaveChanges();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured , please try again later");
            }
        }
        #endregion

        #region "Get opportunity complete information from database "
        [HttpGet]
        public IHttpActionResult GetOpportunity(int oppId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var oppInfo = db.GetOpportunityBasicInfo(oppId).FirstOrDefault();
                    var oppContacts = db.GetOpportunityContacts(oppId).ToList();
                    var oppLogInfo = db.GetOpportunityLog(oppId).ToList();

                    List<OpportunitiesLogModel> oppLogs = new List<OpportunitiesLogModel>();

                    for (int i = 0; i < oppLogInfo.Count; i++)
                    {
                        OpportunitiesLogModel oppLog = new OpportunitiesLogModel();

                        oppLog.CreatedDate = oppLogInfo[i].CreatedDate.ToString("dd-MMM-yyyy hh:mm tt");
                        oppLog.CreatedBy = oppLogInfo[i].CreatedBy;
                        if (oppLogInfo[i].AssignedTo != null)
                        {
                            oppLog.AssignedTo = oppLogInfo[i].AssignedTo.ToString();
                        }
                        oppLog.Status = oppLogInfo[i].Status;
                        oppLog.Id = oppLogInfo[i].Id;
                        oppLog.Comments = oppLogInfo[i].Comments;

                        //string noHTML = Regex.Replace(oppLogInfo[i].Comments, @"<[^>]+>|&nbsp;", "").Trim();
                        //string noHTMLNormalised = Regex.Replace(noHTML, @"\s{2,}", " ");
                        //oppLog.Comments = Regex.Replace(noHTMLNormalised, "<[^>]*>", string.Empty).Trim();

                        var attachments = db.OpportunityAttachments.Where(x => x.OppLog_Id == oppLog.Id).ToList();

                        if (attachments.Count > 0)
                        {
                            oppLog.Attachments = attachments.Select(x => x.FileAttachment).ToArray();
                        }

                        oppLogs.Add(oppLog);
                    }

                    return Content(HttpStatusCode.OK, new { oppInfo, oppLogs, oppContacts });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }

        }
        #endregion

        #region "Update Opportunity"
        [HttpPost]
        public IHttpActionResult EditOpportunity(int oppId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var assingedTo = form.Get("assignee");
                    var status = form.Get("status");

                    Opportunity opportunity = db.Opportunities.Where(x => x.Id == oppId).FirstOrDefault();

                    var userId = User.Identity.GetUserId();
                    int UserId = db.Employees.Where(x => x.AspNetUserId == userId).FirstOrDefault().Id;
                    opportunity.Status = status;

                    OpportunitiesLog oppLog = new OpportunitiesLog();

                    if (assingedTo != null)
                    {
                        oppLog.AssignedTo = (assingedTo);
                        opportunity.TaskOwner = (assingedTo);
                    }
                    else
                    {
                        oppLog.AssignedTo = opportunity.TaskOwner;
                    }

                    //  oppLog.CreatedBy = opportunity.CreatedBy;
                    oppLog.CreatedDate = MasterDataController.GetIndianTime(DateTime.UtcNow);
                    oppLog.OpportunityId = oppId;
                    oppLog.Status = form.Get("status");
                    oppLog.CreatedBy = User.Identity.GetUserId();
                    oppLog.Comments = form.Get("Comments");

                    if (oppLog.Status == "Accepted")
                    {
                        opportunity.EDOC = DateTime.Parse(form.Get("edoc"));
                        opportunity.EDOS = DateTime.Parse(form.Get("edos"));
                    }
                    else if (oppLog.Status == "Completed")
                    {
                        opportunity.ActualCompletedDate = Convert.ToDateTime(form.Get("completedDate"));
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
                            var fileDirecory = HttpContext.Current.Server.MapPath("~/CommentAttachments");

                            if (!Directory.Exists(fileDirecory))
                            {
                                Directory.CreateDirectory(fileDirecory);
                            }

                            var fileName = file.FileName;
                            var filePath = Path.Combine(fileDirecory, fileName);
                            file.SaveAs(filePath);

                            OpportunityAttachment attachment = new OpportunityAttachment();

                            attachment.FileName = Path.GetFileNameWithoutExtension(file.FileName);
                            attachment.FileAttachment = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "CommentAttachments", fileName);
                            oppLog.OpportunityAttachments.Add(attachment);
                        }

                    }

                    db.OpportunitiesLogs.Add(oppLog);
                    db.Entry(opportunity).State = System.Data.Entity.EntityState.Modified;

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

        #region "Get Opportunities List"
        [HttpGet]
        public IHttpActionResult GetOpportunitiesList(string opportunity, string client, string assignTo, string status, int? orgId, int? page, int? count)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    int? totalCount = 0;
                    var opportunities = db.GetOpportunitiesList(opportunity, client, assignTo, status, orgId, page, count).OrderBy(x => x.CreatedDate).ToList();
                    if (opportunities.Count() > 0)
                    {
                        totalCount = (int)opportunities.FirstOrDefault().TotalCount;
                    }

                    return Content(HttpStatusCode.OK, new { opportunities, totalCount });

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