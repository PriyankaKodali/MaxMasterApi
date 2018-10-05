using MaxMaster.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MaxMaster.Controllers
{
    public class ChangePasswordController : ApiController
    {

        private ApplicationUserManager _userManager;
        
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

       [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> ChangePassword(string userName, string password)
        {
            ApplicationUser user = await UserManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }
            user.PasswordHash = UserManager.PasswordHasher.HashPassword(password);
            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Content(HttpStatusCode.InternalServerError, "Password updation not successfull");
            }
            return Content(HttpStatusCode.OK, "Password updation successfull new password is user " + userName + " is " + password);
        }
    }
}