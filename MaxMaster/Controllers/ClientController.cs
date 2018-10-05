using MaxMaster.Models;
using MaxMaster.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
//using MaxMaster.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MaxMaster.Controllers
{
    public class ClientController : ApiController
    {

        private static ApplicationDbContext appDb = new ApplicationDbContext();

        private UserManager<ApplicationUser> _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(appDb));

        #region   "    Random String Generator          "
        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz@$&0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion

        #region "   Add Client     "
        [HttpPost]
        public async Task<IHttpActionResult> AddClient()
        {
            try
            {
                var form = HttpContext.Current.Request.Form;
                var clientLocations = JsonConvert.DeserializeObject<List<ClientLocationModel>>(form.Get("ClientLocations"));
                var verticals = JsonConvert.DeserializeObject<List<ClientVerticalsModel>>(form.Get("ClientVerticals"));
                var v = form.Get("ClientVerticals");
                var email = form.Get("Email");
                var currency = form.Get("Currency");
                var paymentType = form.Get("PaymentType");
                var paymentAmount = form.Get("PaymentAmount");
                var gst = form.Get("gst");
                var pan = form.Get("pan");
                var creditPeriod = form.Get("creditPeriod");

                var count = 0;

                /* Register to AspNet Users */

                UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));


                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    Client client = new Client();

                    int organisationId = Convert.ToInt32(form.Get("Organisation"));
                    client.OrgId = Convert.ToInt32(form.Get("Organisation"));

                    var user1 = userManager.FindByEmail(email);
                    if (user1 != null)
                    {
                        //client.AspNetUserId = user1.Id;
                        return Content(HttpStatusCode.InternalServerError, "User with email " + email + " already exists");
                    }
                    else
                    {
                        string organisation = db.Organisations.Where(x => x.Id == client.OrgId).FirstOrDefault().OrgName;
                        var userCreated = new UserController().CreateUser(email, email, "Client", organisation);
                    }

                    client.Name = form.Get("Name");
                    client.ShortName = form.Get("ShortName");
                    client.Email = email;
                    client.Fax = form.Get("Fax");
                    client.PrimaryPhone = form.Get("PrimaryPhone");
                    client.SecondaryPhone = form.Get("SecondaryPhone");
                    client.ClientType = form.Get("ClientType");

                    client.LastUpdated = DateTime.Now;
                    client.UpdatedBy = User.Identity.GetUserId();
                    client.EncrytionKey = RandomString(5);
                    client.Active = true;
                    client.ClientStatus = form.Get("ClientStatus");

                    if (form.Get("ClientType") == "Supplier")
                    {
                        client.AccountName = form.Get("accountName");
                        client.AccountNumber = form.Get("accountNumber");
                        client.BankName = form.Get("bankName");
                        client.BranchName = form.Get("branchName");
                        client.IFSCCode = form.Get("ifscCode");
                    }

                    if (gst != null)
                    {
                        client.GST = gst;
                    }

                    if (pan != null)
                    {
                        client.PAN = pan;
                    }
                    if (creditPeriod != "")
                    {
                        client.CreditPeriod = Convert.ToInt32(form.Get("creditPeriod"));
                    }

                    if (paymentType != null)
                    {
                        client.PaymentType = form.Get("PaymentType");
                    }
                    if (paymentAmount != "")
                    {
                        client.PaymentAmount = Convert.ToDecimal(form.Get("PaymentAmount"));
                    }
                    if (currency != null)
                    {
                        client.Currency = form.Get("Currency");
                    }

                    var user = userManager.FindByEmail(email);
                    if (user != null)
                    {
                        client.AspNetUserId = user.Id;
                    }

                    for (int i = 0; i < clientLocations.Count; i++)
                    {
                        ClientLocation clientloc = new ClientLocation();
                        clientloc.AddressLine1 = clientLocations[i].AddressLine1;
                        clientloc.AddressLine2 = clientLocations[i].AddressLine2;
                        clientloc.Landmark = clientLocations[i].Landmark;
                        clientloc.Country_Id = clientLocations[i].Country;
                        clientloc.State_Id = clientLocations[i].State;
                        clientloc.City_Id = clientLocations[i].City;
                        clientloc.ZIP = clientLocations[i].zip;
                        clientloc.TimeZone_Id = clientLocations[i].TimeZone;
                        clientloc.IsInvoiceAddress = clientLocations[i].IsInvoice;
                        clientloc.Client_Id = client.Id;

                        client.ClientLocations.Add(clientloc);
                    }


                    for (int i = 0; i < verticals.Count; i++)
                    {
                        var vertical = await db.ClientVerticals.FindAsync(verticals[i].value);
                        if (vertical == null)
                        {
                            continue;
                        }
                        client.ClientVerticals.Add(vertical);
                    }



                    db.Clients.Add(client);
                    db.SaveChanges();

                    var clientId = db.Clients.Where(x => x.AspNetUserId == user.Id).FirstOrDefault().Id;
                    return Content(HttpStatusCode.OK, new { Id = clientId, Name = form.Get("ShortName") });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occoured, please try again!");
            }
        }
        #endregion

        #region "   Add Client Employee"
        [HttpPost]
        public IHttpActionResult AddClientEmployee()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {

                    /* Get the inputs from form */
                    var form = HttpContext.Current.Request.Form;

                    var email = form.Get("Email");

                    int OrgId = Convert.ToInt32(form.Get("OrgId"));

                    /* Register to AspNet Users */

                    UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

                    /* create an object of clientEmployees table */
                    ClientEmployee clientEmp = new ClientEmployee();

                    /* assign values from form to table */
                    clientEmp.FirstName = form.Get("FirstName");
                    clientEmp.MiddleName = form.Get("MiddleName");
                    clientEmp.LastName = form.Get("LastName");
                    clientEmp.Email = email;
                    clientEmp.PrimaryPhone = form.Get("PrimaryPhoneNum");
                    clientEmp.SecondaryPhone = form.Get("SecondaryPhoneNumber");
                    clientEmp.Client_Id = Convert.ToInt32(form.Get("ClientId"));
                    clientEmp.Fax = form.Get("Fax");
                    clientEmp.Department = form.Get("Department");
                    clientEmp.LastUpdated = DateTime.Now;
                    clientEmp.Active = true;
                    clientEmp.UpdatedBy = User.Identity.GetUserId();

                    var files = HttpContext.Current.Request.Files;


                    if (files.Count > 0)
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            if (files.GetKey(i).Contains("ImageOne"))
                            {
                                var fileDirectory = HttpContext.Current.Server.MapPath("~/ClientContactsImages");

                                if (!Directory.Exists(fileDirectory))
                                {
                                    Directory.CreateDirectory(fileDirectory);
                                }
                                var fileName = files[i].FileName;
                                var filePath = Path.Combine(fileDirectory, fileName);
                                files[i].SaveAs(filePath);
                                clientEmp.ImageOne = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "ClientContactsImages", fileName);
                            }

                            else
                            {
                                var fileDirectory = HttpContext.Current.Server.MapPath("~/ClientContactsImages");
                                if (!Directory.Exists(fileDirectory))
                                {
                                    Directory.CreateDirectory(fileDirectory);
                                }
                                var fileName = files[i].FileName;
                                var filePath = Path.Combine(fileDirectory, fileName);
                                files[i].SaveAs(filePath);
                                clientEmp.ImageTwo = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "ClientContactsImages", fileName);
                            }
                        }
                    }


                    string organisation = db.Organisations.Where(x => x.Id == OrgId).FirstOrDefault().OrgName;

                    var user = userManager.FindByEmail(email);
                    if (user != null)
                    {
                        return Content(HttpStatusCode.InternalServerError, "User with email " + email + " already exists");
                    }

                    var userCreated = new UserController().CreateUser(email, email, "ClientEmployee", organisation);


                    var userId = userManager.FindByEmail(email);
                    if (user != null)
                    {
                        clientEmp.AspNetUserId = user.Id;
                    }


                    /* Add record to the clientEmployee table */
                    db.ClientEmployees.Add(clientEmp);
                    db.SaveChanges();
                }

                /* If all the entites with validation is successful then return status code as ok  */
                return Ok();
            }
            catch (Exception ex)
            {
                /* store the error log in database */
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occoured, please try again!");
            }
        }
        #endregion

        #region "   Update Client Employee"
        [HttpPost]
        public async Task<IHttpActionResult> UpdateClientEmployee(int EmployeeId)
        {
            try
            {
                var form = HttpContext.Current.Request.Form;
                var id = form.Get("Id");
                var clientId = form.Get("ClientId");
                var oldMail = form.Get("OldEmail");
                var email = form.Get("Email");


                if (oldMail.ToLower() != email.ToLower())
                {
                    var userName = _userManager.FindByEmail(oldMail);
                    var userId = userName.Id;

                    var exists = _userManager.FindByEmailAsync(email);
                    if (exists != null)
                    {
                        return Content(HttpStatusCode.InternalServerError, "User with email " + email + "already exists");
                    }

                    var res = await _userManager.SetEmailAsync(userId, email);
                }

                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    ClientEmployee clEmp = db.ClientEmployees.Find(Convert.ToInt32(id));

                    if (clEmp == null)
                    {
                        return Content(HttpStatusCode.InternalServerError, "Invalid Data");
                    }

                    clEmp.FirstName = form.Get("FirstName").ToString();
                    clEmp.LastName = form.Get("LastName").ToString();
                    clEmp.MiddleName = form.Get("MiddleName").ToString();
                    clEmp.Email = form.Get("Email");
                    clEmp.Fax = form.Get("Fax").ToString();
                    clEmp.PrimaryPhone = form.Get("PrimaryPhoneNum").ToString();
                    clEmp.SecondaryPhone = form.Get("SecondaryPhoneNum").ToString();
                    clEmp.LastUpdated = DateTime.Now;
                    clEmp.Department = form.Get("Department").ToString();
                    clEmp.Client_Id = Convert.ToInt32(clientId);

                    //   clEmp.Active = true;

                    var files = HttpContext.Current.Request.Files;

                    if (files.Count > 0)
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            if (files.GetKey(i).Contains("ImageOne"))
                            {
                                var fileDirectory = HttpContext.Current.Server.MapPath("~/ClientContactsImages");

                                if (!Directory.Exists(fileDirectory))
                                {
                                    Directory.CreateDirectory(fileDirectory);
                                }
                                var fileName = files[i].FileName;
                                var filePath = Path.Combine(fileDirectory, fileName);
                                files[i].SaveAs(filePath);
                                clEmp.ImageOne = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "ClientContactsImages", fileName);
                            }

                            else if (files.GetKey(i).Contains("ImageTwo"))
                            {
                                var fileDirectory = HttpContext.Current.Server.MapPath("~/ClientContactsImages");
                                if (!Directory.Exists(fileDirectory))
                                {
                                    Directory.CreateDirectory(fileDirectory);
                                }
                                var fileName = files[i].FileName;
                                var filePath = Path.Combine(fileDirectory, fileName);
                                files[i].SaveAs(filePath);
                                clEmp.ImageTwo = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "ClientContactsImages", fileName);
                            }
                        }
                    }


                    db.Entry(clEmp).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occoured, please try again!");
            }
        }
        #endregion

        #region " Get clients list"
        [HttpGet]
        public IHttpActionResult GetAllClients(string name, string phone, string email, string clientType, string fax, int? orgId, int? page, int? count)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var totalCount = 0;
                    /* get list of clients through stored procedure db.GetClients */
                    var Clients = db.GetClients(name, phone, email, clientType, fax, orgId, page, count).ToList();
                    if (Clients.Count > 0)
                    {
                        totalCount = (int)Clients.FirstOrDefault().TotalCount;
                    }
                    return Content(HttpStatusCode.OK, new { Clients, totalCount });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }
        #endregion

        #region " Get Client Employees"
        [HttpGet]
        public IHttpActionResult GetClientEmployees(int? clientId, string firstName, string lastName, string email, string primaryPhone, string department, int? orgId, int? page, int? count)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var totalCount = 0;
                    // Get the list of client employees through  db.GetClientEmployees stored procedure
                    var clientEmployees = db.GetClientEmployees(clientId, firstName, lastName, email, primaryPhone, department, orgId, page, count).ToList();

                    if (clientEmployees.Count > 0)
                    {
                        totalCount = (int)clientEmployees.FirstOrDefault().TotalCount;
                    }
                    return Content(HttpStatusCode.OK, new { clientEmployees, totalCount });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occoured, please try again!");
            }
        }
        #endregion

        #region " Get Client Employee "
        [HttpGet]
        public IHttpActionResult GetClientEmployee(int empId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    /* Get employee having Id as empId from database */
                    var Employee = (from emp in db.ClientEmployees where emp.Id == empId select emp).FirstOrDefault();

                    var clientEmp = new ClientEmployeeModel();
                    clientEmp.Client_Id = Employee.Client_Id;
                    clientEmp.client = Employee.Client.ShortName;
                    clientEmp.FirstName = Employee.FirstName;
                    clientEmp.LastName = Employee.LastName;
                    clientEmp.MiddleName = Employee.MiddleName;
                    clientEmp.PhoneNumber = Employee.PrimaryPhone;
                    clientEmp.SecondaryNumber = Employee.SecondaryPhone;
                    clientEmp.Email = Employee.Email;
                    clientEmp.Fax = Employee.Fax;
                    clientEmp.Department = Employee.Department;
                    clientEmp.ImageOne = Employee.ImageOne;
                    clientEmp.ImageTwo = Employee.ImageTwo;

                    /* return an object of clientEmployee */
                    return Content(HttpStatusCode.OK, new { clientEmp });
                }
            }

            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        #endregion

        #region " Get Client for edit"
        [HttpGet]
        public IHttpActionResult GetClient(int ClientId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {

                    var client = db.Clients.Where(x => x.Id == ClientId).FirstOrDefault();
                    var clientLocations = db.ClientLocations.Where(x => x.Client_Id == ClientId).ToList();
                    var clientVerticals = client.ClientVerticals.Select(x => new { value = x.Id, label = x.Name }).ToList();

                    ClientModel clientModel = new ClientModel();

                    clientModel.Id = client.Id;
                    clientModel.Name = client.Name;
                    clientModel.ShortName = client.ShortName;
                    clientModel.Email = client.Email;
                    clientModel.PrimaryNum = client.PrimaryPhone;
                    clientModel.SecondaryNum = client.SecondaryPhone;
                    clientModel.Fax = client.Fax;
                    clientModel.ClientType = client.ClientType;
                    clientModel.PaymentType = client.PaymentType;
                    clientModel.PaymentAmount = client.PaymentAmount;
                    clientModel.Currency = client.Currency;
                    clientModel.IsActive = client.Active;
                    clientModel.OrgId = client.OrgId;
                    clientModel.OrgName = client.Organisation.OrgName;
                    clientModel.GST = client.GST;
                    clientModel.ClientStatus = client.ClientStatus;
                    clientModel.CreditPeriod = client.CreditPeriod;
                    clientModel.PAN = client.PAN;
                    clientModel.BranchName = client.BranchName;
                    clientModel.BankName = client.BankName;
                    clientModel.AccountName = client.AccountName;
                    clientModel.AccountNumber = client.AccountNumber;
                    clientModel.IFSCCode = client.IFSCCode;

                    clientModel.ClientLocations = new List<ClientLocationModel>();

                    foreach (var loc in clientLocations)
                    {
                        var clientLoc = new ClientLocationModel();

                        clientLoc.LocationId = loc.Id;
                        clientLoc.AddressLine1 = loc.AddressLine1;
                        clientLoc.AddressLine2 = loc.AddressLine2;
                        clientLoc.Landmark = loc.Landmark;
                        clientLoc.TimeZone = loc.TimeZone_Id;
                        clientLoc.TimeZoneLabel = loc.TimeZone.Name + " ( " + loc.TimeZone.Offset + ")  ( " + loc.TimeZone.Abbr + " ) ";
                        clientLoc.zip = loc.ZIP;

                        if (loc.City_Id != 0)
                        {
                            clientLoc.City = loc.City_Id;
                            clientLoc.CityName = loc.City.Name;
                            clientLoc.State = loc.City.State_Id;
                            clientLoc.StateName = loc.City.State.Name;
                            clientLoc.Country = loc.City.State.Country_Id;
                            clientLoc.CountryName = loc.City.State.Country.Name;
                            clientLoc.IsInvoice = loc.IsInvoiceAddress;
                        }

                        clientModel.ClientLocations.Add(clientLoc);
                    }


                    clientModel.Verticals = new List<ClientVerticalsModel>();

                    foreach (var vertical in clientVerticals)
                    {
                        var verticalModel = new ClientVerticalsModel();
                        verticalModel.value = vertical.value;
                        verticalModel.label = vertical.label;

                        clientModel.Verticals.Add(verticalModel);

                    }

                    return Content(HttpStatusCode.OK, new { clientModel, clientVerticals });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region " Update Client"
        [HttpPost]
        public async Task<IHttpActionResult> UpdateClient()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var id = form.Get("ClientId");
                    var clientLocations = JsonConvert.DeserializeObject<List<ClientLocationModel>>(form.Get("ClientLocations"));
                    var verticals = JsonConvert.DeserializeObject<List<ClientVerticalsModel>>(form.Get("ClientVerticals"));
                    var oldMail = form.Get("OldEmail");
                    var email = form.Get("Email");
                    var count = 0;
                    var currency = form.Get("Currency");
                    var paymentType = form.Get("PaymentType");
                    var paymentAmount = form.Get("PaymentAmount");
                    var gst = form.Get("gst");
                    var pan = form.Get("pan");
                    var creditPeriod = form.Get("creditPeriod");

                    //locationId

                    Client client = db.Clients.Find(Convert.ToInt32(id));

                    if (oldMail.ToLower() != email.ToLower())
                    {
                        var userName = _userManager.FindByEmail(oldMail);
                        var userId = userName.Id;

                        var exists = _userManager.FindByEmailAsync(email);
                        if (exists != null)
                        {
                            //   return Content(HttpStatusCode.InternalServerError, "User with email " + email + "already exists");
                        }

                        var res = await _userManager.SetEmailAsync(userId, email);
                    }

                    if (client == null)
                    {
                        return Content(HttpStatusCode.InternalServerError, "Invalid Data");
                    }

                    var clientVerticalList = client.ClientVerticals.ToList();

                    if (clientVerticalList.Count > 0)
                    {
                        foreach (var vertical in client.ClientVerticals.ToList())
                        {
                            client.ClientVerticals.Remove(vertical);
                        }
                    }

                    client.Name = form.Get("Name");
                    client.ShortName = form.Get("ShortName");
                    client.Email = form.Get("Email");
                    client.PrimaryPhone = form.Get("PrimaryPhone");
                    client.SecondaryPhone = form.Get("SecondaryPhone");
                    client.Fax = form.Get("Fax");
                    client.ClientType = form.Get("ClientType");
                    client.Active = Convert.ToBoolean(form.Get("IsActive"));
                    client.OrgId = Convert.ToInt32(form.Get("Organisation"));
                    client.ClientStatus = form.Get("ClientStatus");

                    for (int i = 0; i < verticals.Count(); i++)
                    {
                        var clientVertical = await db.ClientVerticals.FindAsync(verticals[i].value);
                        client.ClientVerticals.Add(clientVertical);
                    }

                    for (int i = 0; i < clientLocations.Count; i++)
                    {
                        var loc = clientLocations[i].LocationId;

                        if (loc != null)
                        {
                            var clientLoc = db.ClientLocations.Find(clientLocations[i].LocationId);
                            clientLoc.AddressLine1 = clientLocations[i].AddressLine1;
                            clientLoc.AddressLine2 = clientLocations[i].AddressLine2;
                            clientLoc.Landmark = clientLocations[i].Landmark;
                            clientLoc.Country_Id = clientLocations[i].Country;
                            clientLoc.State_Id = clientLocations[i].State;
                            clientLoc.City_Id = clientLocations[i].City;
                            clientLoc.ZIP = clientLocations[i].zip;
                            clientLoc.TimeZone_Id = clientLocations[i].TimeZone;
                            clientLoc.IsInvoiceAddress = clientLocations[i].IsInvoice;

                            //if (clientLoc.Country_Id == 101)
                            //{
                            //    count++;
                            //}

                            db.Entry(clientLoc).State = System.Data.Entity.EntityState.Modified;
                        }
                        else
                        {
                            ClientLocation clientloc = new ClientLocation();
                            clientloc.AddressLine1 = clientLocations[i].AddressLine1;
                            clientloc.AddressLine2 = clientLocations[i].AddressLine2;
                            clientloc.Landmark = clientLocations[i].Landmark;
                            clientloc.Country_Id = clientLocations[i].Country;
                            clientloc.State_Id = clientLocations[i].State;
                            clientloc.City_Id = clientLocations[i].City;
                            clientloc.ZIP = clientLocations[i].zip;
                            clientloc.TimeZone_Id = clientLocations[i].TimeZone;
                            clientloc.IsInvoiceAddress = clientLocations[i].IsInvoice;
                            client.ClientLocations.Add(clientloc);
                            db.SaveChanges();
                        }
                    }

                    if (form.Get("ClientType") == "Supplier")
                    {
                        client.AccountName = form.Get("accountName");
                        client.AccountNumber = form.Get("accountNumber");
                        client.BankName = form.Get("bankName");
                        client.BranchName = form.Get("branchName");
                        client.IFSCCode = form.Get("ifscCode");
                    }

                    if (gst != null)
                    {
                        client.GST = gst;
                    }

                    else
                    {
                        client.GST = null;
                    }

                    if (pan != null)
                    {
                        client.PAN = pan;
                    }
                    if (creditPeriod != "")
                    {
                        client.CreditPeriod = Convert.ToInt32(form.Get("creditPeriod"));
                    }

                    else
                    {
                        client.GST = null;
                        client.PAN = null;
                        client.CreditPeriod = null;
                    }

                    if (client.ClientType != "Supplier" && client.ClientType != "Vendor")
                    {
                        if (paymentType != null)
                        {
                            client.PaymentType = form.Get("PaymentType");
                        }
                        else
                        {
                            client.PaymentType = null;
                        }

                        var Amount = (form.Get("PaymentAmount"));
                        if (Amount != "")
                        {
                            client.PaymentAmount = Convert.ToDecimal(form.Get("PaymentAmount"));
                        }
                        else
                        {
                            client.PaymentAmount = null;
                        }

                        if (currency != null)
                        {
                            client.Currency = form.Get("Currency");
                        }
                        else
                        {
                            client.Currency = null;
                        }
                    }
                    else
                    {
                        client.PaymentType = null;
                        client.Currency = null;
                        client.PaymentAmount = null;
                    }

                    db.Entry(client).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, Please try again later");
            }
        }
        #endregion

        #region "Get Client locations"
        [HttpGet]
        public IHttpActionResult GetClientLocations(int? clientId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var locations = db.ClientLocations.Where(x => x.Client_Id == clientId).Select(x => new
                    {
                        value = x.Id,
                        label = x.AddressLine1 + "," + x.AddressLine2 + x.City.Name + "," + x.State.Name + "," + x.Country.Name
                     + "," + x.ZIP
                    }).ToList();

                    return Content(HttpStatusCode.OK, new { locations });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, Please try again later");
            }
        }
        #endregion

        #region "Get Projects with location "
        [HttpGet]
        public IHttpActionResult GetClientProjects(string clientId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var clientProjects = (from opp in db.Opportunities
                                          where (opp.Client_Id == clientId && opp.ActualCompletedDate == null && (opp.Status == "Accepted" || opp.Status == "InProcess"))
                                          select (new
                                          {
                                              value = opp.Id,
                                              label = opp.OpportunityName + " ( " +
                                                          (db.ClientLocations.Where(x => x.Id == opp.Location_Id).
                                                          Select(x => x.AddressLine1 + ", " + x.City.Name + " ," +
                                                          x.State.Name + " ," + x.Country.Name + " ," + x.ZIP).FirstOrDefault()
                                               ) + " )"
                                          })).ToList();

                    return Content(HttpStatusCode.OK, new { clientProjects });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region "Get Suppliers bank details"
        [HttpGet]
        public IHttpActionResult GetSuppliersBankDetails(string supplierId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var supplier = db.Clients.Where(x => x.AspNetUserId == supplierId).Select(x => new { x.AccountName, x.AccountNumber, x.BranchName, x.BankName, x.IFSCCode }).FirstOrDefault();
                    return Content(HttpStatusCode.OK, new { supplier });
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