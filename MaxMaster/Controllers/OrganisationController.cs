using MaxMaster.Models;
using MaxMaster.ViewModels;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;


namespace MaxMaster.Controllers
{
    public class OrganisationController : ApiController
    {
        [HttpPost]
        public IHttpActionResult AddOrganisation()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var OrganisationLocations = JsonConvert.DeserializeObject<List<ViewModels.OrganisationLocation>>(form.Get("OrgLocations"));

                    Organisation org = new Organisation();
                    org.OrgName = form.Get("orgName");
                    org.WebSite = form.Get("website");
                    org.Email = form.Get("Email");
                    org.PrimaryPhone = form.Get("primaryPhoneNum");
                    org.SecondaryPhone = form.Get("secondaryPhoneNum");
                    org.EmpPrefix = form.Get("empPrefix").ToUpper();
                    org.PAN = form.Get("pan");
                    org.GST = form.Get("gst");
                    org.TIN = form.Get("tin");
                    org.UpdatedBy = User.Identity.GetUserId();
                    org.LastUpdated = MasterDataController.GetIndianTime(DateTime.Now);

                    var EmpPrefixExists = db.Organisations.Where(x => x.EmpPrefix == org.EmpPrefix).FirstOrDefault();

                    if (EmpPrefixExists != null)
                    {
                        return Content(HttpStatusCode.InternalServerError, "Organisation with the same employee prefix already exists ");
                    }

                    var file = HttpContext.Current.Request.Files;

                    if (file.Count > 0)
                    {
                        var image = file["OrgLogo"];
                        var fileExtension = Path.GetExtension(image.FileName);

                        var fileDirecory = HttpContext.Current.Server.MapPath("~/OrgLogos");
                        if (!Directory.Exists(fileDirecory))
                        {
                            Directory.CreateDirectory(fileDirecory);
                        }
                        var fileName = DateTime.Now.Ticks + "_" + image.FileName;
                        var filepath = Path.Combine(fileDirecory, fileName);
                        image.SaveAs(filepath);
                        org.Logo = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "OrgLogos", fileName);
                    }

                    for (int i = 0; i < OrganisationLocations.Count(); i++)
                    {
                        OrgansationLocation orgLoc = new OrgansationLocation();

                        orgLoc.AddressLine1 = OrganisationLocations[i].AddressLine1;
                        orgLoc.AddressLine2 = OrganisationLocations[i].AddressLine2;
                        orgLoc.CountryId = Convert.ToInt32(OrganisationLocations[i].Country);
                        orgLoc.StateId = OrganisationLocations[i].State;
                        orgLoc.CityId = OrganisationLocations[i].City;
                        orgLoc.ZIP = OrganisationLocations[i].ZIP;

                        org.OrgansationLocations.Add(orgLoc);
                    }

                    db.Organisations.Add(org);
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
        public IHttpActionResult GetOrganisations(string orgName, string email, string website, string phoneNum, int? page, int? count, string sortCol, string sortDir)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var organisations = db.GetOrganisations(orgName, email, website, phoneNum, page, count, sortCol, sortDir).ToList();
                    int TotalCount = 0;
                    if (organisations.Count > 0)
                    {
                        TotalCount = (int)organisations.FirstOrDefault().TotalCount;
                    }
                    return Content(HttpStatusCode.OK, new { organisations, TotalCount });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetOrganisation(int orgId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var org = db.Organisations.Find(orgId);

                    OrganisationModel organisation = new OrganisationModel();

                    organisation.Id = org.Id;
                    organisation.OrgName = org.OrgName;
                    organisation.Email = org.Email;
                    organisation.PrimaryPhone = org.PrimaryPhone;
                    organisation.Website = org.WebSite;
                    organisation.SecondaryPhone = org.SecondaryPhone;
                    organisation.PAN = org.PAN;
                    organisation.TIN = org.TIN;
                    organisation.GST = org.GST;
                    organisation.EmpPrefix = org.EmpPrefix;
                    organisation.OrgLogo = org.Logo;

                    organisation.OrgLocation = new List<OrganisationLocation>();

                    var OrgLocations = db.OrgansationLocations.Where(x => x.OrgId == orgId).ToList();

                    for (int i = 0; i < OrgLocations.Count(); i++)
                    {
                        OrganisationLocation orgLoc = new OrganisationLocation();

                        orgLoc.LocId = OrgLocations[i].Id;
                        orgLoc.AddressLine1 = OrgLocations[i].AddressLine1;
                        orgLoc.AddressLine2 = OrgLocations[i].AddressLine2;
                        orgLoc.City = OrgLocations[i].CityId;
                        orgLoc.CityName = OrgLocations[i].City.Name;
                        orgLoc.State = OrgLocations[i].StateId;
                        orgLoc.StateName = OrgLocations[i].City.State.Name;
                        orgLoc.Country = OrgLocations[i].CountryId;
                        orgLoc.CountryName = OrgLocations[i].City.State.Country.Name;
                        orgLoc.ZIP = OrgLocations[i].ZIP;

                        organisation.OrgLocation.Add(orgLoc);
                    }

                    return Content(HttpStatusCode.OK, new { organisation });

                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpPost]
        public IHttpActionResult UpdateOrg(int OrgId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var orgLocations = JsonConvert.DeserializeObject<List<ViewModels.OrganisationLocation>>(form.Get("OrgLocations"));

                    Organisation organisation = db.Organisations.Find(OrgId);

                    organisation.OrgName = form.Get("orgName");
                    organisation.Email = form.Get("Email");
                    organisation.WebSite = form.Get("website");
                    organisation.PrimaryPhone = form.Get("primaryPhoneNum");
                    organisation.SecondaryPhone = form.Get("secondaryPhoneNum");
                    organisation.PAN = form.Get("pan");
                    organisation.GST = form.Get("gst");
                    organisation.TIN = form.Get("tin");
                    organisation.EmpPrefix = form.Get("empPrefix");
                    organisation.UpdatedBy = User.Identity.GetUserId();
                    organisation.LastUpdated = MasterDataController.GetIndianTime(DateTime.Now);

                    var file = HttpContext.Current.Request.Files;

                    if (file.Count > 0)
                    {
                        var image = file["OrgLogo"];
                        var fileExtension = Path.GetExtension(image.FileName);

                        var fileDirecory = HttpContext.Current.Server.MapPath("~/OrgLogos");
                        if (!Directory.Exists(fileDirecory))
                        {
                            Directory.CreateDirectory(fileDirecory);
                        }
                        var fileName = DateTime.Now.Ticks + "_" + image.FileName;
                        var filepath = Path.Combine(fileDirecory, fileName);
                        image.SaveAs(filepath);
                        organisation.Logo = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "OrgLogos", fileName);
                    }

                    for (int i = 0; i < orgLocations.Count(); i++)
                    {
                        var locationId = orgLocations[i].LocId;

                        if (locationId != null)
                        {
                            var loc = db.OrgansationLocations.Find(locationId);

                            loc.AddressLine1 = orgLocations[i].AddressLine1;
                            loc.AddressLine2 = orgLocations[i].AddressLine2;
                            loc.CityId = orgLocations[i].City;
                            loc.StateId = orgLocations[i].State;
                            loc.CountryId = orgLocations[i].Country;
                            loc.ZIP = orgLocations[i].ZIP;

                            db.Entry(loc).State = EntityState.Modified;
                        }
                        else
                        {
                            OrgansationLocation orgLoc = new OrgansationLocation();

                            orgLoc.AddressLine1 = orgLocations[i].AddressLine1;
                            orgLoc.AddressLine2 = orgLocations[i].AddressLine2;
                            orgLoc.CountryId = orgLocations[i].Country;
                            orgLoc.StateId = orgLocations[i].State;
                            orgLoc.CityId = orgLocations[i].City;
                            orgLoc.ZIP = orgLocations[i].ZIP;

                            organisation.OrgansationLocations.Add(orgLoc);
                        }
                    }

                    db.Entry(organisation).State = EntityState.Modified;
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
