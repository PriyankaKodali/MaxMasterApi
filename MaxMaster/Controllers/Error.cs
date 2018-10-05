using MaxMaster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaxMaster.Controllers
{
    public class Error
    {
        public void logAPIError(string Method, string Message, string StackTrace)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    ExceptionLog el = new ExceptionLog();
                    el.Method = Method;
                    el.Message = Message;
                    el.StackTrace = StackTrace;
                    el.Time = DateTime.Now;
                    db.ExceptionLogs.Add(el);
                    db.SaveChanges();
                }
                return;
            }
            catch(Exception ex)
            {

                return;
            }
        }

    }
}