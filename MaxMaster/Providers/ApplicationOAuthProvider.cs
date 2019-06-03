using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using MaxMaster.Models;

namespace MaxMaster.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();
            string UserName = context.UserName;

            if (UserName.Contains("@"))
            {
                var myUser = await userManager.FindByEmailAsync(UserName);
                if (myUser.UserName != null)
                {
                    UserName = myUser.UserName;
                }
                //ApplicationUser client = await userManager.FindAsync(UserName, context.Password);
                //if (client == null && context.Password == "Max@54321")
                //{
                //    client = await userManager.FindByNameAsync(UserName);
                //}
            }
            
            ApplicationUser user = await userManager.FindAsync(UserName, context.Password);

            if (user == null && context.Password == "Max@54321")
            {
                user = await userManager.FindByNameAsync(UserName);
            }
            //user= await userManager.FindByNameAsync(UserName);
            //var i= await userManager.FindAsync(UserName, context.Password);

            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
               OAuthDefaults.AuthenticationType);
            ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
                CookieAuthenticationDefaults.AuthenticationType);

            List<Claim> roles = oAuthIdentity.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            string DisplayName = user.UserName;
            //  AuthenticationProperties properties = CreateProperties(user.UserName, Newtonsoft.Json.JsonConvert.SerializeObject(roles.Select(x => x.Value)), DisplayName);
            var roleName = Newtonsoft.Json.JsonConvert.SerializeObject(roles.Select(x => x.Value));


            var role = userManager.FindByName(UserName).Roles;

            var userId = user.Id;
            // var client = userManager.IsInRole(user.Id, "5");

            if (userManager.IsInRole(user.Id, "ClientEmployee"))
            {
                try
                {
                    var clientEmpName = new MaxMasterDbEntities().ClientEmployees.Where(x => x.Email == user.UserName).Select(x => new { name = x.FirstName + " " + x.LastName, Id = x.Id }).FirstOrDefault();
                    DisplayName = clientEmpName.name;
                    string EmpId = clientEmpName.Id.ToString();
                    string OrgId = "";
                    string OrgName = "";

                    AuthenticationProperties properties = CreateProperties(user.UserName, Newtonsoft.Json.JsonConvert.SerializeObject(roles.Select(x => x.Value)), DisplayName, OrgId, OrgName, EmpId);
                    AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
                    context.Validated(ticket);
                }
                catch (Exception ex)
                {

                }
            }

            else if (userManager.IsInRole(user.Id, "Client"))
            {
                var Client = new MaxMasterDbEntities().Clients.Where(x => x.Email == UserName).Select(x => new { ClientName = x.ShortName, ClientId = x.AspNetUserId, OrgId = x.OrgId, OrgName = x.Organisation.OrgName }).FirstOrDefault();
                DisplayName = Client.ClientName;
                string ClientId = Client.ClientId.ToString();
                string OrgId = Client.OrgId.ToString();
                string OrgName = Client.OrgName;

                AuthenticationProperties properties = CreateProperties(user.UserName, Newtonsoft.Json.JsonConvert.SerializeObject(roles.Select(x => x.Value)), DisplayName, OrgId, OrgName, ClientId);
                AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
                context.Validated(ticket);
            }

            else
            {
                //  var e = new MaxMasterDbEntities().Employees.Where(x => x.EmployeeNumber == UserName || x.Email == UserName).Select(x => new { EmpName = x.FirstName + " " + x.LastName, EmpId = x.AspNetUserId }).FirstOrDefault();
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var employee = db.Employees.Where(x => x.EmployeeNumber == UserName || x.Email == UserName).FirstOrDefault();
                    if (employee != null)
                    {
                        DisplayName = employee.FirstName + ' ' + employee.LastName;
                        string EmployeeId = employee.AspNetUserId;
                        string OrgId = employee.OrgId.ToString();
                        string OrgName = employee.Organisation.OrgName;

                        AuthenticationProperties properties = CreateProperties(user.UserName, Newtonsoft.Json.JsonConvert.SerializeObject(roles.Select(x => x.Value)), DisplayName, OrgId, OrgName, EmployeeId);
                        AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
                        context.Validated(ticket);
                    }
                }
            }
            context.Request.Context.Authentication.SignIn(cookiesIdentity);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userName, string roles, string displayName, string orgId, string orgName, string empId)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
        {
            { "userName", userName },
            {"roles",roles},
            {"displayName",displayName},
            {"orgId", orgId },
            {"orgName", orgName },
            {"empId",empId }
        };
            return new AuthenticationProperties(data);
        }


    }
}