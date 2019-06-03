using MaxMaster.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MaxMaster.Controllers
{
   

    public class UserController : ApiController
    {
        private static ApplicationDbContext appDb = new ApplicationDbContext();

        private UserManager<ApplicationUser> _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(appDb));

        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz@$&0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public bool CreateUser(string Username, string Email, string role, string organisation)
        {
            try
            {
                var password = RandomString(8);
                ApplicationUser user = new ApplicationUser() { UserName = Username, Email = Email };

                var Existing = _userManager.FindByEmail(Email);

                if (Existing != null)
                {
                    return false;
                    //return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
                }
                _userManager.UserValidator = new UserValidator<ApplicationUser>(_userManager)
                {
                    AllowOnlyAlphanumericUserNames = false,
                    RequireUniqueEmail = true
                };
                IdentityResult result = _userManager.Create(user, password);

                if (!result.Succeeded)
                {
                    return false;
                    //  return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
                }

                var roleStore = new RoleStore<IdentityRole>(appDb);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                _userManager.AddToRole(user.Id, role);
                 
                string to = Email; 
                string subject = "Welcome to " + organisation; 
                string body = "Hi,Welcome to " + organisation + ". Following are the credentials for your account" + Environment.NewLine + "Username : " + Username + Environment.NewLine + "Password : " + password + Environment.NewLine;
                string from = "maxtranssystems2018@gmail.com";

                if ( role != "ClientEmployee" && role != "Doctor")
                {
                    new EmailController().SendEmail(from, "Max", to, subject, body);
                }
                else
                {
                    new EmailController().SendEmail(from, "Max", "priyankakodali611@gmail.com", subject, body);
                }
                return true;
             
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return false;
                //     return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }

        public bool ChangeUsername(string OldUsername, string NewUsername)
        {
            try
            {
                ApplicationUser user = appDb.Users.Where(x => x.UserName == OldUsername).First();
                user.UserName = NewUsername;
                user.Email = NewUsername;
                appDb.Entry(user).State = System.Data.Entity.EntityState.Modified;
                appDb.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}