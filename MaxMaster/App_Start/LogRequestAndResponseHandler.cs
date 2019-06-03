using MaxMaster.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MaxMaster.App_Start
{
    public class LogRequestAndResponseHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = await base.SendAsync(request, cancellationToken);
            var context = ((HttpContextBase)request.Properties["MS_HttpContext"]);
            var userId = context.User.Identity.GetUserId();
            if (userId != null)
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var employee = db.Employees.Where(x => x.AspNetUserId == userId).FirstOrDefault();
                    if (employee != null)
                    {
                        employee.LastActivityTime = DateTime.Now;
                        db.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChangesAsync();
                    }
                }
            }
            
            return result;
        }
    }
}