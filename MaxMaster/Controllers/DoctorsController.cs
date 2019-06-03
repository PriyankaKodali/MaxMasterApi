using MaxMaster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using Microsoft.AspNet.Identity;
using MaxMaster.ViewModels;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MaxMaster.Controllers
{
    public class DoctorsController : ApiController
    {

        private static ApplicationDbContext appDb = new ApplicationDbContext();

        private UserManager<ApplicationUser> _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(appDb));

        #region " Get Doctors List"
        [HttpGet]
        public IHttpActionResult GetDoctorsList(string authId, string name, string client, string email, string phoneNum, string jobLevel, string voiceGrade, int? page, int? count, string sortCol, string sortDir)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    // stored procedure [dbo].[DoctorDetailsGet] to retreive list of doctors 
                    int totalCount = 0;
                    var DoctorsList = db.DoctorDetailsGet(authId, name, client, email, phoneNum, jobLevel, voiceGrade, page, count, sortCol, sortDir).ToList();

                    if (DoctorsList.Count > 0)
                    {
                        totalCount = (int)DoctorsList.FirstOrDefault().TotalCount;
                    }
                    return Content(HttpStatusCode.OK, new { DoctorsList, totalCount });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, Please try again later");
            }
        }

        #endregion

        #region " Add Doctors"
        public async Task<IHttpActionResult> AddDoctor()
        {
            try
            {
                var form = HttpContext.Current.Request.Form;
                var specialities = JsonConvert.DeserializeObject<List<keyValueModel>>(form.Get("specialities"));
                var email = form.Get("email");
                var doctorGroup = form.Get("doctorGroup");
                var macropercent = form.Get("macroPercent");

                UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

                var user1 = userManager.FindByEmail(email);
                if (user1 != null)
                {
                    return Content(HttpStatusCode.InternalServerError, "User with email " + email + " already exists");
                }

                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    /* Get the values from form and add to database thorugh Entity Framework */

                    Doctor doc = new Doctor();

                    doc.Salutation = form.Get("salutation");
                    doc.FirstName = form.Get("firstName");
                    doc.MiddleName = form.Get("middleName");
                    doc.LastName = form.Get("lastName");
                    doc.Email = form.Get("email");
                    doc.PrimaryPhone = form.Get("primaryPhoneNum");
                    doc.SecondaryPhone = form.Get("secondaryPhoneNum");
                    doc.Client_Id = Convert.ToInt32(form.Get("clientId"));
                    doc.AddressLine1 = form.Get("addressLine1");
                    doc.AddressLine2 = form.Get("addressLin2");
                    doc.Country_Id = Convert.ToInt32(form.Get("countryId"));
                    doc.State_Id = Convert.ToInt32(form.Get("stateId"));
                    doc.City_Id = Convert.ToInt32(form.Get("cityId"));
                    doc.ZIP = form.Get("zip");
                    doc.IdigitalId = form.Get("idigitalId");
                    doc.IdigitalAuthorId = form.Get("idigitalAuthId");
                    doc.JobLevel = form.Get("jobLevel");
                    doc.DictationMode = form.Get("dictationMode");

                    if (macropercent != "")
                    {
                        doc.MacroPercent = Convert.ToByte(form.Get("macroPercent"));
                    }
                    else
                    {
                        doc.MacroPercent = 0;
                    }

                    doc.VoiceGrade = form.Get("voiceGrade");
                    if (doctorGroup != null)
                    {
                        doc.DoctorGroup_Id = Convert.ToInt32(form.Get("doctorGroup"));
                    }

                    doc.Active = true;
                    doc.LastUpdated = DateTime.Now;
                    doc.UpdatedBy = User.Identity.GetUserId();

                    /* Iterate through the list of specialitites and insert into doctorSpecialities table */
                    for (int i = 0; i < specialities.Count; i++)
                    {
                        var speciality = await db.Specialties.FindAsync(specialities[i].value);
                        if (speciality == null)
                        {
                            continue;
                        }
                        doc.Specialties.Add(speciality);
                    }

                    var idigitalAuthIdExists = db.Doctors.Where(x => x.IdigitalAuthorId == doc.IdigitalAuthorId).FirstOrDefault();
                    if (idigitalAuthIdExists != null)
                    {
                        return Content(HttpStatusCode.InternalServerError, "User with IDigital AuthorId already exists");
                    }
                    int orgId = db.Clients.Where(x => x.Id == doc.Client_Id).FirstOrDefault().OrgId;
                    string organisation = db.Organisations.Where(x => x.Id == orgId).FirstOrDefault().OrgName;
                    var userCreated = new UserController().CreateUser(email, email, "Doctor", organisation);
                    var user = userManager.FindByEmail(email);

                    if (user != null)
                    {
                        doc.AspNetUserId = user.Id;
                    }

                    if (userCreated)
                    {
                        db.Doctors.Add(doc);
                        db.SaveChanges();
                        return Ok();
                    }
                    else
                    {
                        return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
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

        #region " GetDoctor"
        [HttpGet]
        public IHttpActionResult GetDoctor(int doctor_Id)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var docDetails = db.Doctors.Where(x => x.Id == doctor_Id).FirstOrDefault();

                    var specialities = docDetails.Specialties.Select(x => new { value = x.Id, label = x.Name }).OrderBy(x => x.label).ToList(); ;

                    var doctor = new DoctorsModel();
                    if (doctor != null)
                    {
                        doctor.Salutation = docDetails.Salutation;
                        doctor.FirstName = docDetails.FirstName;
                        doctor.MiddleName = docDetails.MiddleName;
                        doctor.LastName = docDetails.LastName;
                        doctor.PhoneNumber = docDetails.PrimaryPhone;
                        doctor.SecondaryNumber = docDetails.SecondaryPhone;
                        doctor.Email = docDetails.Email;
                        doctor.Client_Id = docDetails.Client_Id;
                        doctor.Client = docDetails.Client.Name;
                        doctor.Addressline1 = docDetails.AddressLine1;
                        doctor.AddressLine2 = docDetails.AddressLine2;

                        if (docDetails.City_Id != 0)
                        {
                            doctor.City_Id = docDetails.City_Id;
                            doctor.City = docDetails.City.Name;
                            doctor.State_Id = docDetails.City.State_Id;
                            doctor.State = docDetails.City.State.Name;
                            doctor.Country_Id = docDetails.City.State.Country_Id;
                            doctor.Country = docDetails.City.State.Country.Name;
                        }

                        doctor.Zip = docDetails.ZIP;
                        doctor.DictationMode = docDetails.DictationMode;
                        doctor.JobLevel = docDetails.JobLevel;
                        doctor.MacroPercent = docDetails.MacroPercent;
                        doctor.IdigitalId = docDetails.IdigitalId;
                        doctor.IdigitalAuthorId = docDetails.IdigitalAuthorId;
                        doctor.DoctorGroup_Id = docDetails.DoctorGroup_Id;
                        doctor.DoctorGroup = docDetails.DoctorGroup.Name;
                        doctor.VoiceGrade = docDetails.VoiceGrade;
                        doctor.IsActive = docDetails.Active;

                    }

                    return Content(HttpStatusCode.OK, new { doctor, specialities });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region " Update Doctor"
        [HttpPost]
        public async Task<IHttpActionResult> UpdateDoctor(int doctor_Id)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var specialities = JsonConvert.DeserializeObject<List<keyValueModel>>(form.Get("specialities"));
                    var newMail = form.Get("email");
                    var oldMail = form.Get("OldMail");
                    var macropercent = form.Get("macroPercent");

                    if (newMail.ToLower() != oldMail.ToLower())
                    {
                        var userName = _userManager.FindByEmail(oldMail);
                        var userId = userName.Id;

                        var exists = await _userManager.FindByEmailAsync(newMail);

                        if (exists != null)
                        {
                            return Content(HttpStatusCode.InternalServerError, "User with email " + newMail + " already exists");
                        }

                        var res = await _userManager.SetEmailAsync(userId, newMail);
                    }

                    Doctor doc = db.Doctors.Find(Convert.ToInt32(doctor_Id));

                    doc.Salutation = form.Get("salutation");
                    doc.FirstName = form.Get("firstName");
                    doc.MiddleName = form.Get("middleName");
                    doc.LastName = form.Get("lastName");
                    doc.Email = newMail;
                    doc.PrimaryPhone = form.Get("primaryPhoneNum");
                    doc.SecondaryPhone = form.Get("secondaryPhoneNum");
                    doc.Client_Id = Convert.ToInt32(form.Get("clientId"));
                    doc.AddressLine1 = form.Get("addressLine1");
                    doc.AddressLine2 = form.Get("addressLin2");
                    doc.Country_Id = Convert.ToInt32(form.Get("countryId"));
                    doc.State_Id = Convert.ToInt32(form.Get("stateId"));
                    doc.City_Id = Convert.ToInt32(form.Get("cityId"));
                    doc.ZIP = form.Get("zip");
                    doc.IdigitalId = form.Get("idigitalId");
                    doc.IdigitalAuthorId = form.Get("idigitalAuthId");
                    doc.JobLevel = form.Get("jobLevel");
                    doc.DictationMode = form.Get("dictationMode");
                    if (macropercent != "")
                    {
                        doc.MacroPercent = Convert.ToByte(form.Get("macroPercent"));
                    }
                    else
                    {
                        doc.MacroPercent = 0;
                    }

                    doc.VoiceGrade = form.Get("voiceGrade");
                    doc.DoctorGroup_Id = Convert.ToInt32(form.Get("doctorGroup"));
                    doc.Active = true;
                    doc.LastUpdated = DateTime.Now;
                    doc.UpdatedBy = User.Identity.GetUserId();
                    doc.Active = Convert.ToBoolean(form.Get("IsActive"));


                    /* Remove previous specialities of this doctor */
                    foreach (var speciality in doc.Specialties.ToList())
                    {
                        doc.Specialties.Remove(speciality);
                    }

                    /* Iterate through the list of specialitites and insert into doctorSpecialities table */
                    for (int i = 0; i < specialities.Count; i++)
                    {
                        var speciality = await db.Specialties.FindAsync(specialities[i].value);
                        if (speciality == null)
                        {
                            continue;
                        }
                        doc.Specialties.Add(speciality);
                    }

                    db.Entry(doc).State = System.Data.Entity.EntityState.Modified;
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

        #region " Add Doctor Group"
        [HttpPost]
        public IHttpActionResult AddDoctorGroup()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var groupName = form.Get("GroupName");

                    DoctorGroup docGroup = new DoctorGroup();
                    docGroup.Name = groupName;
                    db.DoctorGroups.Add(docGroup);
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
    }
}