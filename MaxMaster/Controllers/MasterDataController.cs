using MaxMaster.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace MaxMaster.Controllers
{
    public class MasterDataController : ApiController
    {

        [HttpGet]
        public IHttpActionResult GetCities(int stateId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var cities = db.Cities.Where(x => x.State_Id == stateId).Select(x => new { value = x.Id, label = x.Name }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { cities });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured! please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetStates(int countryId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var states = db.States.Where(x => x.Country_Id == countryId).Select(x => new { value = x.Id, label = x.Name }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { states });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured! please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetCountries()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var countries = db.Countries.Select(x => new { value = x.Id, label = x.Name }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { countries });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured! please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetDesignations()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var designations = db.Designations.Select(x => new { value = x.Id, label = x.Name }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { designations });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured! please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetDepartments()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var departments = db.Departments.Select(x => new { value = x.Id, label = x.Name }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { departments });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured! please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetSpecialties()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var specialties = db.Specialties.Select(x => new { value = x.Id, label = x.Name }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { specialties });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured! please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetManagers(int? departmentId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var managers = (from employee in db.Employees where (employee.Department_Id == departmentId && employee.Designation.Name == "Manager") || (employee.Designation.Name == "Sr.Manager") select new { value = employee.Id, label = employee.FirstName + " " + employee.LastName }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { managers });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured! please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetClientVerticals()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var clientVerticals = db.ClientVerticals.Select(x => new { value = x.Id, label = x.Name }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { clientVerticals });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured! please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetTimeZones()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var timeZones = db.TimeZones.Select(x => new { value = x.Id, label = x.Name + " ( " + x.Offset + ")  ( " + x.Abbr + " ) " }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { timeZones });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetEmpRoles()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var empRoles = db.AspNetRoles.Select(x => new { value = x.Id, label = x.Name }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { empRoles });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetDoctorGroups()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var doctorGroups = db.DoctorGroups.Select(x => new { value = x.Id, label = x.Name }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { doctorGroups });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetDoctors()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var doctors = db.Doctors.Select(x => new { value = x.Id, label = x.FirstName + " " + x.LastName }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { doctors });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetDoctorsList(int clientId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var doctors = (from doctor in db.Doctors
                                   where (doctor.Client_Id == clientId)
                                   select new { value = doctor.Id, label = doctor.FirstName + " " + doctor.LastName }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { doctors });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured! please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetOrganisations()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var organisations = db.Organisations.Select(x => new { value = x.Id, label = x.OrgName }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { organisations });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try agin later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetCategories(int? deptId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var categories = (from cat in db.Categories where (cat.Dept_Id == deptId) select new { value = cat.Id, label = cat.Name }).OrderBy(x => x.label).ToList();
                    //    var categories = db.Categories.Select(x => new { value = x.Id, label = x.Name }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { categories });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetClients(int? orgId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())

                {
                    /* Get List of client from clients table */

                    if (orgId == null)
                    {
                        var clients = db.Clients.Where(x => x.Active == true).Select(x => new { value = x.Id, label = x.ShortName + " ( " + x.Organisation.OrgName + " )" }).OrderBy(x => x.label).ToList();
                        return Content(HttpStatusCode.OK, new { clients });
                    }

                    else
                    {
                        var clients = db.Clients.Where(x => x.OrgId == orgId && x.Active == true).Select(x => new { value = x.Id, label = x.ShortName }).OrderBy(x => x.label).ToList();
                        return Content(HttpStatusCode.OK, new { clients });
                    }

                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetOrgEmployees(int? orgId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    if (orgId != null)
                    {
                        var employees = db.Employees.Where(x => x.OrgId == orgId && x.Active == true).Select(x => new { value = x.Id, label = x.FirstName + " " + x.LastName }).OrderBy(x => x.label).ToList();
                        return Content(HttpStatusCode.OK, new { employees });
                    }
                    else
                    {
                        var employees = db.Employees.Select(x => new { value = x.Id, label = x.FirstName + " " + x.LastName }).OrderBy(x => x.label).ToList();
                        return Content(HttpStatusCode.OK, new { employees });
                    }

                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetSubCategories(int catId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var subCategories = db.SubCategories.Where(x => x.Category_Id == catId).Select(x => new { value = x.Id, label = x.Name }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { subCategories });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.OK, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetEmployeesForTaskAllocation(string creatorId, int? orgId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {

                    var LoginUser = User.Identity.GetUserId();
                    var user = db.Employees.Where(x => x.AspNetUserId == LoginUser && x.Active == true).Select(x => x.AspNetUserId).FirstOrDefault();
                    var superAdiminsList = db.Employees.Where(x => x.AspNetUserId != user && x.OrgId != orgId && x.Active == true && x.Role_Id == "8").Select(x => new { value = x.AspNetUserId, label = x.FirstName + " " + x.LastName + "(" + x.Organisation.OrgName + ")" }).OrderBy(x => x.label).ToList();

                    if (creatorId == null)
                    {
                        if (orgId != null)
                        {
                            var employeesOfOrg = db.Employees.Where(x => x.AspNetUserId != user && x.OrgId == orgId && x.Active == true && x.Role_Id != "4").Select(x => new { value = x.AspNetUserId, label = x.FirstName + " " + x.LastName }).OrderBy(x => x.label).ToList();
                            var employees = employeesOfOrg.Union(superAdiminsList).ToList();

                            return Content(HttpStatusCode.OK, new { employees });
                        }
                        else
                        {
                            var employees = db.Employees.Where(x => x.AspNetUserId != user && x.Active == true && x.Role_Id != "4").Select(x => new { value = x.AspNetUserId, label = x.FirstName + " " + x.LastName + "(" + x.Organisation.OrgName + ")" }).OrderBy(x => x.label).ToList();
                            return Content(HttpStatusCode.OK, new { employees });
                        }

                    }

                    else
                    {
                        var empList = db.Employees.Where(x => x.AspNetUserId != user).ToList();

                        if (orgId != null)
                        {
                            var orgEmployeesList = empList.Where(x => x.AspNetUserId != creatorId && x.OrgId == orgId && x.Active == true && x.Role_Id != "4").Select(x => new { value = x.AspNetUserId, label = x.FirstName + " " + x.LastName }).OrderBy(x => x.label).ToList();

                            var employees = orgEmployeesList.Union(superAdiminsList).ToList();

                            return Content(HttpStatusCode.OK, new { employees });
                        }
                        else
                        {
                            var employees = db.Employees.Where(x => x.AspNetUserId != user && x.Active == true && x.Role_Id != "4").Select(x => new { value = x.AspNetUserId, label = x.FirstName + " " + x.LastName + "(" + x.Organisation.OrgName + ")" }).OrderBy(x => x.label).ToList();
                            return Content(HttpStatusCode.OK, new { employees });
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured please try again llater");

            }
        }

        [HttpGet]
        public IHttpActionResult GetClientContacts(int clientId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var clientContacts = db.ClientEmployees.Where(x => x.Client_Id == clientId && x.Active == true).Select(x => new { value = x.Id, label = x.FirstName + " " + x.LastName }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { clientContacts });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetManagersOfOrg(int orgId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var managers = (from employee in db.Employees where (employee.OrgId == orgId && (employee.Designation.Name == "Manager" || employee.Designation.Name == "Sr.Manager" || employee.Designation.Name == "V.P")) select new { value = employee.Id, label = employee.FirstName + " " + employee.LastName }).ToList();
                    return Content(HttpStatusCode.OK, new { managers });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.OK, "An error, occured, Please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetOpportunityCategories()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var categories = db.OpportunityCategories.Select(x => new { value = x.Id, label = x.CategoryName }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { categories });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetOpportunitySubCategories(int oppCatId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var oppSubCategories = (from opp in db.OpportunitySubCategories where (opp.OppCategory_Id == oppCatId) select new { value = opp.Id, label = opp.OppSubCategory }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { oppSubCategories });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetAllClients()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    /* Get List of client from clients table */
                    var clients = db.Clients.Select(x => new { value = x.Id, label = x.ShortName }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { clients });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetClientsWithAspNetUserId(int? orgId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())

                {
                    /* Get List of client from clients table */

                    if (orgId == null)
                    {
                        var clients = db.Clients.Where(x => x.Active == true).Select(x => new { value = x.AspNetUserId, label = x.ShortName + " ( " + x.Organisation.OrgName + " )", Id = x.Id }).OrderBy(x => x.label).ToList();
                        return Content(HttpStatusCode.OK, new { clients });
                    }

                    else
                    {
                        var clients = db.Clients.Where(x => x.OrgId == orgId && x.Active == true).Select(x => new { value = x.AspNetUserId, label = x.ShortName }).OrderBy(x => x.label).ToList();
                        return Content(HttpStatusCode.OK, new { clients });
                    }

                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetAllEmployeesWithAspNetUserId(int? orgId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    if (orgId != null)
                    {
                        var employees = db.Employees.Where(x => x.Active == true && x.OrgId == orgId && x.Role_Id != "4").Select(x => new { value = x.AspNetUserId, label = x.FirstName + " " + x.LastName }).OrderBy(x => x.label).ToList();
                        return Content(HttpStatusCode.OK, new { employees });
                    }
                    else
                    {
                        // var orgName = db.Organisations.Where(x => x.Id == orgId).Select(x => x.OrgName).FirstOrDefault();
                        var employees = db.Employees.Where(x => x.Active == true && x.Role_Id != "4").Select(x => new { value = x.AspNetUserId, label = x.FirstName + " " + x.LastName + "( " + x.Organisation.OrgName + " )" }).OrderBy(x => x.label).ToList();
                        return Content(HttpStatusCode.OK, new { employees });
                    }


                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occureed, please try again later");
            }
        }

        public static DateTime GetIndianTime(DateTime time)
        {
            return TimeZoneInfo.ConvertTime(time, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        }

        [HttpGet]
        public IHttpActionResult GetSuppliers()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var suppliers = db.Clients.Where(x => x.ClientType == "Supplier").Select(x => new
                    {
                        value = x.AspNetUserId,
                        label = x.Name
                    }).OrderBy(x => x.label).ToList();

                    return Content(HttpStatusCode.OK, new { suppliers });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetItems()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var items = db.ItemsMasters.Select(x => new { value = x.Id, label = x.ItemName + "[" + x.ModelNumber + "]", description = x.Description, upc = x.UPC, model = x.ModelNumber, serialNoExists = x.SrlNoExists, units = x.Units }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { items });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetOrganisationLocations(int orgId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var orgLocations = db.OrgansationLocations.Where(x => x.OrgId == orgId).Select(x => new
                    {
                        value = x.Id,
                        label = x.AddressLine1 + ", " + x.AddressLine2 + x.City.Name + ", " + x.City.State.Name + "," + x.City.State.Country.Name + "," + x.ZIP
                    }).OrderBy(x => x.label).ToList();

                    return Content(HttpStatusCode.OK, new { orgLocations });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetTDS()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var tds = db.TDS.Select(x => new { value = x.Id, label = x.TaxName + " - [" + x.Rate + "%]", rate = x.Rate }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { tds });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        [HttpGet]
        public IHttpActionResult GetGST()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var gst = db.GSTs.Select(x => new { value = x.Id, label = x.GSTName + "[" + x.Rate + "%]", rate = x.Rate }).OrderBy(x => x.rate).ToList();
                    return Content(HttpStatusCode.OK, new { gst });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }



















        [HttpGet]
        public IHttpActionResult GetEmployees()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var employees = db.Employees.Select(x => new { value = x.Id, label = x.FirstName + " " + x.LastName }).OrderBy(x => x.label).ToList();
                    return Content(HttpStatusCode.OK, new { employees });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, Please try again later!");
            }

        }

        [HttpGet]
        public IHttpActionResult GetProjects(string clientId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var projects = db.Opportunities.Where(x => x.Client_Id == clientId && x.Status == "Accepted").Select(x => new { value = x.Id, label = x.OpportunityName }).OrderBy(x => x.label).ToList();
                    var client = db.Clients.Where(x => x.AspNetUserId == clientId).FirstOrDefault();
                    if (client != null)
                    {
                        var projectLocation = db.ClientLocations.Where(x => x.Client_Id == client.Id).Select(x => new
                        {
                            label = x.AddressLine1 + " " + x.AddressLine2 + "," + x.City.Name + "," + x.State.Name + "," + x.Country.Name
                        }).FirstOrDefault();

                        var ProjectWithLoc = db.Opportunities.Where(x => x.Client_Id == clientId && x.Status == "Accepted").Select(x => new { value = x.Id, label = x.OpportunityName + " ( " + projectLocation.label + " )" }).OrderBy(x => x.label).ToList();
                        return Content(HttpStatusCode.OK, new { projects, ProjectWithLoc });
                    }

                    else
                    {
                        return Content(HttpStatusCode.OK, new { projects });
                    }


                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, Please try again later");
            }
        }
    }
}