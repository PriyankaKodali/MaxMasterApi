using MaxMaster.Models;
using MaxMaster.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MaxMaster.Controllers
{
    public class EmployeeController : ApiController
    {
        private static ApplicationDbContext appDb = new ApplicationDbContext();

        private UserManager<ApplicationUser> _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(appDb));

        #region   " Random String Generator          "
        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz@$&0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion

        #region " Add Employee  "
        [HttpPost]
        public async Task<IHttpActionResult> AddEmployee()
        {
            try
            {
                /* get data from form */
                var form = HttpContext.Current.Request.Form;  
                var email = form.Get("Email");
                var firstName = form.Get("FirstName");
                var role = form.Get("RoleId");
                var doj = form.Get("DOJ");
                var provisionalPeriod = form.Get("ProvisionalPeriod");
                var managerId = form.Get("Manager");
              
                /* Register to AspNet Users */

                UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

                var user1 = userManager.FindByEmail(email);
                if (user1 != null)
                {
                    return Content(HttpStatusCode.InternalServerError, "User with email " + email + " already exists");
                }

                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var roleName = db.AspNetRoles.Where(x => x.Id == role).FirstOrDefault().Name;
                    
                    Employee emp = new Employee();
                    emp.FirstName = firstName;
                    emp.MiddleName = form.Get("MiddleName");
                    emp.LastName = form.Get("LastName");
                    emp.Email = form.Get("Email");
                    emp.PrimaryPhone = form.Get("PrimaryNum");
                    emp.SecondaryPhone = form.Get("SecondaryNum");
                    emp.Gender = form.Get("Gender");
                    emp.DOB = DateTime.Parse(form.Get("DOB"));
                    emp.BloodGroup = form.Get("BloodGroup");
                    emp.Aadhar = form.Get("Aadhar");
                    emp.PAN = form.Get("Pan");
                    emp.AddressLine1 = form.Get("AddressLine1");
                    emp.AddressLine2 = form.Get("AddressLine2");
                    emp.Country_Id = Convert.ToInt32(form.Get("CountryId"));
                    emp.State_Id = Convert.ToInt32(form.Get("StateId"));
                    emp.City_Id = Convert.ToInt32(form.Get("CityId"));
                    emp.ZIP = form.Get("Zip");
                    emp.Role_Id = (form.Get("RoleId"));
                    emp.Department_Id = Convert.ToInt32(form.Get("DepartmentId"));
                    emp.Designation_Id = Convert.ToInt32(form.Get("DesignationId"));
                    emp.OrgId = Convert.ToInt32(form.Get("OrgId"));
                    emp.Shift_Id = Convert.ToInt32(form.Get("Shift"));
                     
                    if (provisionalPeriod != "")
                    {
                        emp.ProvisionalPeriod = Convert.ToByte(provisionalPeriod);
                    }
                    else
                    {
                        emp.ProvisionalPeriod = 0;
                    }

                    if (!string.IsNullOrEmpty(managerId))
                    {
                        emp.Manager_Id = Convert.ToInt32(form.Get("Manager"));
                    }

                    emp.EmploymentType = form.Get("EmpType");

                    if (!string.IsNullOrEmpty(doj))
                    {
                        emp.DOJ = DateTime.Parse(form.Get("DOJ"));
                    }

                    emp.UpdatedBy = User.Identity.GetUserId();
                    emp.LastUpdated = DateTime.Now; 
                    emp.Active = true; 

                    var prefix = db.Organisations.Where(x => x.Id == emp.OrgId).FirstOrDefault().EmpPrefix;

                    if (emp.EmploymentType == "Consultant")
                    {
                        int Count = db.Employees.Where(x => x.EmploymentType == "Consultant" && x.OrgId == emp.OrgId).Count();

                        if (Count > 0)
                        {
                            var LastEmployeeNumber = db.Employees.Where(x => x.EmploymentType == "Consultant" && x.OrgId == emp.OrgId).OrderByDescending(x => x.LastUpdated).FirstOrDefault().EmployeeNumber;
                            if (LastEmployeeNumber != null)
                            {
                                 int count = Convert.ToInt32(LastEmployeeNumber.Split('V')[1]); 
                                GenerateNewNumber:
                                var employeeNumber = prefix + "V" + (count + 1).ToString("00");
                                var empExists = db.Employees.Where(x => x.EmployeeNumber == employeeNumber).FirstOrDefault();
                                var userexists = await _userManager.FindByNameAsync(employeeNumber);
                                if (empExists != null || userexists!=null)
                                {
                                      count++;
                                    goto GenerateNewNumber;
                                }
                                emp.EmployeeNumber = employeeNumber;
                            }
                        }
                        else
                        {
                            emp.EmployeeNumber = prefix + "V01";
                        }

                    }
                    if (emp.EmploymentType == "Permanent")
                    {
                        int Count = db.Employees.Where(x => x.EmploymentType == "Permanent" && x.OrgId == emp.OrgId).Count();

                        if (Count > 0)
                        {
                            var LastEmployeeNumber = db.Employees.Where(x => x.EmploymentType == "Permanent" && x.OrgId == emp.OrgId).OrderByDescending(x => x.EmployeeNumber).FirstOrDefault().EmployeeNumber;
                            if (LastEmployeeNumber != null)
                            {
                                int count = Convert.ToInt32(LastEmployeeNumber.Split('-')[1]);
                                GenerateNewNumber:
                                var employeeNumber = prefix + (count + 1).ToString("000");
                                if (db.Employees.Any(x => x.EmployeeNumber == employeeNumber))
                                {
                                    count++;
                                    goto GenerateNewNumber;
                                }
                                emp.EmployeeNumber = employeeNumber;
                            }
                        }

                        else
                        {
                            emp.EmployeeNumber = prefix + "001";
                        }

                    }

                    var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileDirectory = HttpContext.Current.Server.MapPath("~/Employee_Images"); 
                        if (!Directory.Exists(fileDirectory))
                        {
                            Directory.CreateDirectory(fileDirectory);
                        }

                        var physicalPath = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], emp.EmployeeNumber + Path.GetExtension(file.FileName));
                        file.SaveAs(physicalPath);
                        var path = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "Employee_Images", emp.EmployeeNumber + Path.GetExtension(file.FileName));
                        emp.PhotoURL = path;
                    } 
                    string organisation = db.Organisations.Where(x => x.Id == emp.OrgId).FirstOrDefault().OrgName;
                    bool UserCreated = new UserController().CreateUser(emp.EmployeeNumber, email, roleName, organisation);
                    var user = userManager.FindByEmail(email);

                    if (user != null)
                    {
                        emp.AspNetUserId = user.Id;
                    }
                    db.Employees.Add(emp);

                    EmployeePayscale empPayScale = new EmployeePayscale();
                     empPayScale.Gross = 0;
                     empPayScale.Emp_Id = emp.Id;

                     db.EmployeePayscales.Add(empPayScale);
                     db.SaveChanges();

                      return Ok();
                }
            }

            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occoured, please try again!");
            }
        }
        #endregion

        #region " Get Employee for Edit"
        [HttpGet]
        public async Task<IHttpActionResult> GetEmployee(int EmpId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    Employee emp = await db.Employees.FindAsync(EmpId);
                    
                    var employee = new EmployeeModel();

                    employee.FirstName = emp.FirstName;
                    employee.MiddleName = emp.MiddleName;
                    employee.LastName = emp.LastName;
                    employee.Email = emp.Email;
                    employee.PrimaryPhoneNum = emp.PrimaryPhone;
                    employee.SecondaryPhoneNum = emp.SecondaryPhone;
                    employee.Gender = emp.Gender;
                    employee.BloodGroup = emp.BloodGroup;
                    employee.Aadhar = emp.Aadhar;
                    employee.Pan = emp.PAN;
                    employee.AddressLine1 = emp.AddressLine1;
                    employee.AddressLine2 = emp.AddressLine2;

                    if (emp.City_Id != 0)
                    {
                        employee.CityId = emp.City_Id;
                        employee.CityName = emp.City.Name;
                        employee.StateId = emp.City.State_Id;
                        employee.StateName = emp.City.State.Name;
                        employee.Country = emp.City.State.Country_Id;
                        employee.CountryName = emp.City.State.Country.Name;
                    }
                    
                    if (emp.Shift_Id != null)
                    {
                        employee.ShiftId = emp.Shift_Id;
                        employee.ShiftTimings = (emp.Shift.InTime).ToString(@"hh\:mm") + "-" + (emp.Shift.OutTime).ToString(@"hh\:mm");
                    }
                    
                    employee.DOB = emp.DOB.ToString("yyyy-MM-dd");
                    employee.DOJ = emp.DOJ.ToString("yyyy-MM-dd");
                    employee.ZIP = emp.ZIP;
                    employee.EmploymentType = emp.EmploymentType;
                    employee.DesgId = emp.Designation_Id;
                    employee.DesgName = emp.Designation.Name;
                    employee.DeptId = emp.Department_Id;
                    employee.DeptName = emp.Department.Name;
                    employee.RoleId = emp.Role_Id;
                    employee.RoleName = emp.AspNetRole.Name;
                    employee.OrgId = emp.OrgId;
                    employee.OrgName = emp.Organisation.OrgName;
                    employee.PhotoUrl = emp.PhotoURL;
                    
                    var manager = emp.Manager_Id;
                    if (manager != null)
                    {
                        employee.ManagerId = emp.Manager_Id;
                        employee.ManagerName = db.Employees.Where(x => x.Id == emp.Manager_Id).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                    }

                    employee.ProvisionalPeriod = emp.ProvisionalPeriod;

                    return Content(HttpStatusCode.OK, new { employee });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured , please try again later");
            }
        }
        #endregion

        #region " Update Employee   "
        [HttpPost]
        public async Task<IHttpActionResult> UpdateEmployee(int EmpId)
        {
            try
            {
                _userManager.UserValidator = new UserValidator<ApplicationUser>(_userManager)
                {
                    AllowOnlyAlphanumericUserNames = false
                };

                var form = HttpContext.Current.Request.Form;
                var firstName = form.Get("FirstName");
                var role = form.Get("RoleId");
                var oldMail = form.Get("OldEmail");
                var newMail = form.Get("Email");
                var managerId = form.Get("Manager");
                var provisionalPeriod = form.Get("ProvisionalPeriod");
                var doj = form.Get("DOJ");
                var dor = form.Get("DOR");

                /* Register to AspNet Users */

                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var roleName = db.AspNetRoles.Where(x => x.Id == (role)).FirstOrDefault().Name;
                    Employee emp = await db.Employees.FindAsync(EmpId);

                    /* remove existing user from AspNetUserRoles table and add the new role to the user */
                    var userName = _userManager.FindByEmail(oldMail);
                    var userId = userName.Id;
                    var roles = await _userManager.GetRolesAsync(userId);

                    if (userName != null)
                    {
                        var result = await _userManager.RemoveFromRolesAsync(userId, roles.ToArray());
                        var addToRole = await _userManager.AddToRoleAsync(userId, roleName);
                    }

                    /* update AspNetUser with new mail if edited */

                    if (oldMail.ToLower() != newMail.ToLower())
                    {
                        var exists = await _userManager.FindByEmailAsync(newMail);

                        if (exists != null)
                        {
                            return Content(HttpStatusCode.InternalServerError, "User with email " + newMail + " already exists");
                        }

                        var res = await _userManager.SetEmailAsync(userId, newMail);
                    }

                    emp.FirstName = firstName;
                    emp.MiddleName = form.Get("MiddleName");
                    emp.LastName = form.Get("LastName");
                    emp.Email = form.Get("Email");
                    emp.PrimaryPhone = form.Get("PrimaryNum");
                    emp.SecondaryPhone = form.Get("SecondaryNum");
                    emp.Gender = form.Get("Gender");
                    emp.DOB = DateTime.Parse(form.Get("DOB"));
                    emp.BloodGroup = form.Get("BloodGroup");
                    emp.Aadhar = form.Get("Aadhar");
                    emp.PAN = form.Get("Pan");
                    emp.AddressLine1 = form.Get("AddressLine1");
                    emp.AddressLine2 = form.Get("AddressLine2");
                    emp.Country_Id = Convert.ToInt32(form.Get("CountryId"));
                    emp.State_Id = Convert.ToInt32(form.Get("StateId"));
                    emp.City_Id = Convert.ToInt32(form.Get("CityId"));
                    emp.ZIP = form.Get("Zip");
                    emp.Role_Id = (form.Get("RoleId"));
                    emp.Department_Id = Convert.ToInt32(form.Get("DepartmentId"));
                    emp.Designation_Id = Convert.ToInt32(form.Get("DesignationId"));
                    emp.OrgId = Convert.ToInt32(form.Get("OrgId"));
                    emp.Shift_Id = Convert.ToInt32(form.Get("Shift"));

                    if (!string.IsNullOrEmpty(managerId))
                    {
                        emp.Manager_Id = Convert.ToInt32(form.Get("Manager"));
                    }

                    emp.EmploymentType = form.Get("EmpType");
                    // emp.DOJ = DateTime.Parse(form.Get("DOJ"));
                    if (!string.IsNullOrEmpty(doj))
                    {
                        emp.DOJ = DateTime.Parse(form.Get("DOJ"));
                    } 
                    if (!string.IsNullOrEmpty(dor))
                    {
                        emp.DOR = DateTime.Parse(form.Get("DOR"));
                        emp.Active = false;
                    }

                    emp.UpdatedBy = User.Identity.GetUserId();
                    emp.LastUpdated = DateTime.Now;

                    if (provisionalPeriod != "")  {
                        emp.ProvisionalPeriod = Convert.ToByte(provisionalPeriod);
                    }
                    else {
                        emp.ProvisionalPeriod = 0;
                    }

                    var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileDirectory = HttpContext.Current.Server.MapPath("~/Employee_Images");
                        if (!Directory.Exists(fileDirectory))
                        {
                            Directory.CreateDirectory(fileDirectory);
                        }

                        var filename = emp.EmployeeNumber + file.FileName;
                        var physicalPath = Path.Combine(fileDirectory,filename);

                        file.SaveAs(physicalPath);

                        var path = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "Employee_Images", filename);
                        emp.PhotoURL = path;
                    }


                    db.Entry(emp).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return Ok();

                }

            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occoured, please try again!");
            }
        }

        #endregion

        #region " Get List Of Employees"
        [HttpGet]
        public IHttpActionResult GetAllEmployees(string empNum, string name, string email, string phoneNum, string department, string designation, string manager, int? orgId, int? page, int? count, string sortCol, string sorDir)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var totalCount = 0;
                    var employees = db.GetEmployees(empNum, name, email, phoneNum, department, designation, manager, orgId, page, count, sortCol, sorDir).ToList();
                    if (employees.Count > 0)
                    {
                        totalCount = (int)employees.FirstOrDefault().TotalCount;
                    }
                    return Content(HttpStatusCode.OK, new { employees, totalCount });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error ocuured, Please try again later!");
            }
        }
        #endregion

        #region " Get Employee Documents"
        [HttpGet]
        public IHttpActionResult GetEmpDocuments(int EmpId, string category, DateTime? uploadDate, DateTime? documentDate, string notes, string keyWords, int page, int count, string sortCol, string sortDir)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var employeeDocumnets = db.GetEmployeeDocuments(EmpId, category, uploadDate, documentDate, notes, keyWords, page, count, sortCol, sortDir).ToList();

                    if (employeeDocumnets.Count > 0)
                    {
                        var EmployeeName = employeeDocumnets.FirstOrDefault().EmployeeName;
                        var EmpNumber = employeeDocumnets.FirstOrDefault().EmployeeNumber;
                        var totalCount = employeeDocumnets.FirstOrDefault().TotalCount;
                        return Content(HttpStatusCode.OK, new { employeeDocumnets, EmployeeName, EmpNumber, totalCount });
                    }

                    else
                    {
                        var totalCount = 0;
                        var EmployeeName = db.Employees.Where(x => x.Id == EmpId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                        var EmpNumber = db.Employees.Where(x => x.Id == EmpId).Select(x => x.EmployeeNumber).FirstOrDefault();
                        return Content(HttpStatusCode.OK, new { employeeDocumnets, EmployeeName, EmpNumber, totalCount });
                    }


                    //EmployeeDocumentModel empDoc = new EmployeeDocumentModel();
                    //empDoc.EmployeeName = employeeDocumnets.FirstOrDefault().EmployeeName;
                    //empDoc.EmployeeNumber = employeeDocumnets.FirstOrDefault().EmployeeNumber;
                    //var totalCount = employeeDocumnets.FirstOrDefault().TotalCount;

                    //foreach (var doc in employeeDocumnets)
                    //{
                    //    var documents = new DocumentModel();
                    //    documents.Category = doc.Category;
                    //    documents.Keywords = doc.Keywords;
                    //    documents.Notes = doc.Notes;
                    //    documents.UploadDate =doc.UploadDate.ToString();
                    //    documents.DocumentDate = doc.DocumentDate.ToString();
                    //    empDoc.Documents.Add(documents);
                    //}

                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region " Add Employee documents "
        public IHttpActionResult AddDocument()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var docDate = form.Get("DocDate");
                    var files = HttpContext.Current.Request.Files;
                    var Document = files["Documents"];

                    var fileExtension = Path.GetExtension(Document.FileName);
                    var validExtensions = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".doc", ".docx", ".pdf", ".txt", ".xls", ".xlsx" };
                    if (!validExtensions.Contains(fileExtension))
                    {
                        return Content(HttpStatusCode.InternalServerError, "Invalid file type for detailed estimate!!");
                    }

                    Document doc = new Document();

                    if (!string.IsNullOrEmpty(docDate))
                    {
                        doc.DocumentDate = DateTime.Parse(docDate);
                    }
                    doc.Category = form.Get("Category");

                    doc.Notes = form.Get("Notes");

                    doc.Keywords = form.Get("KeyWords");
                    doc.UploadDate = DateTime.Now;
                    doc.Employee_Id = Convert.ToInt32(form.Get("EmployeeId"));

                    var fileDirecory = HttpContext.Current.Server.MapPath("~/Employees/Document");
                    if (!Directory.Exists(fileDirecory))
                    {
                        Directory.CreateDirectory(fileDirecory);
                    }
                    var fileName = DateTime.Now.Ticks + "_" + Document.FileName;
                    var filepath = Path.Combine(fileDirecory, fileName);
                    Document.SaveAs(filepath);
                    doc.DocumentURL = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "Employee", "Documents", fileName);

                    db.Documents.Add(doc);
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

        #region " Get Employee Payscale"
        [HttpGet]
        public IHttpActionResult GetEmpPayScale(int EmpId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    EmployeePayScaleModel empPayScale = new EmployeePayScaleModel();
                    var payScaleId = db.EmployeePayscales.Where(x => x.Emp_Id == EmpId).FirstOrDefault().Id;
                    var empPay = db.EmployeePayscales.Find(payScaleId);

                    empPayScale.AccountName = empPay.BankAccName;
                    empPayScale.AccountNumber = empPay.BankAccNo;
                    empPayScale.BankName = empPay.BankName;
                    empPayScale.BranchName = empPay.BankBranch;
                    empPayScale.CTC = empPay.Gross;

                    return Content(HttpStatusCode.OK, new { empPayScale });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured please try again later");
            }
        }
        #endregion

        #region " Update Employee PayScale"
        [HttpPost]
        public IHttpActionResult AddEmpPayScale(int EmpId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;

                    EmployeePayscale emp = db.EmployeePayscales.Where(x => x.Emp_Id == EmpId).FirstOrDefault();

                    //EmployeePayscale empPayScale = new EmployeePayscale();
                    emp.BankAccNo = form.Get("AccountNumber");
                    emp.BankAccName = form.Get("AccountName");
                    emp.BankBranch = form.Get("BranchName");
                    emp.BankName = form.Get("BankName");
                    emp.Gross = Convert.ToInt32(form.Get("CTC"));

                    db.Entry(emp).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return Ok();

                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occureed, please try again later");
            }
        }
        #endregion

        //#region " Get Employee master Report"
        //public IHttpActionResult GetEmployeesMasterReport(int? EmpId, int? ClientId, int? DoctorId, string JobWorkLevel, DateTime? fromdate, DateTime? todate, int? page, int? count, string sortCol, string sortDir)
        //{
        //    try
        //    {

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
        //        return Content(HttpStatusCode.InternalServerError, "An error occoured, please try again!");
        //    }

        //}
        //#endregion


    }
}