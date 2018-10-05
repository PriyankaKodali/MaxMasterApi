using MaxMaster.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace MaxMaster.Controllers
{
    public class PaymentsController : ApiController
    {

        #region " Get Clients Currency"
        public IHttpActionResult GetClientsCurrency(int ClientId)
        {
            try
            {
                //        using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                //        {
                //            var clientCurrency = (from client in db.Clients where client.Id == ClientId select new { client.Currency }).FirstOrDefault();
                //            return Content(HttpStatusCode.OK, new { clientCurrency });
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                //        return InternalServerError();
                //    }
                //}

                //#region " Get clients payment list"
                //public IHttpActionResult GetPaymentDetails(int client_id)
                //{
                //    try
                //    {
                //        using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                //        {
                //            decimal overallDue = 0;

                //            var paymentDetails = db.ClientPayments(client_id).ToList().OrderBy(x => x.InvoiceCreatedDate);

                //            foreach (var due in paymentDetails)
                //            {
                //                overallDue += Convert.ToDecimal(due.DueAmount);
                //            }

                //            return Content(HttpStatusCode.OK, new { paymentDetails, overallDue });
                //        }
                return Ok();
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return InternalServerError();
            }

        }
        #endregion

        #region " Pay invoice amount"
        [HttpPost]
        public async Task<IHttpActionResult> PayAmount()
        {
            try
            {

                //        var form = HttpContext.Current.Request.Form;

                //        var clientId = form.Get("Client_Id");
                //        var TotalamountPaid = form.Get("paymentAmount");
                //        var paymentDate = form.Get("paymentDate");
                //        var currency = form.Get("Currency");
                //        var description = form.Get("Description");
                //        var createdBy = form.Get("PaymentCreatedBy");
                //        var invoiceServiceIds = JsonConvert.DeserializeObject<List<string>>(form.Get("ClientPayment"));
                //        var file = HttpContext.Current.Request.Files;
                //        var ChequePdf = file["ChequePdf"];

                //        if (ChequePdf != null)
                //        {

                //            var fileExtension = Path.GetExtension(ChequePdf.FileName);
                //            var validExtensions = new string[] { ".doc", ".docx", ".pdf", ".txt" };

                //            if (!validExtensions.Contains(fileExtension))
                //            {
                //                return Content(HttpStatusCode.InternalServerError, "Invalid file type for resume!!");
                //            }
                //        }


                //        using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                //        {
                //            var invoiceServices = db.InvoiceServices.Where(x => invoiceServiceIds.Contains(x.InvoiceId)).OrderByDescending(x => x.InvoiceId);

                //            ClientReceivable clientRecv = new ClientReceivable();

                //            clientRecv.Client_Id = Convert.ToInt32(clientId);
                //            clientRecv.Currency = currency;
                //            clientRecv.PaymentDate = Convert.ToDateTime(paymentDate);
                //            clientRecv.Description = description;
                //            clientRecv.AmountPaid = Convert.ToDecimal(TotalamountPaid);
                //            clientRecv.CreatedBy = Convert.ToInt32((from emp in db.Employees where emp.EmployeeNumber == createdBy select emp.Id).FirstOrDefault());
                //            clientRecv.CreatedDate = DateTime.Now;

                //            var files = HttpContext.Current.Request.Files;
                //            var ScannedDocument = files["ChequePdf"];
                //            if (ScannedDocument != null)
                //            {
                //                var fileDirecory = HttpContext.Current.Server.MapPath("~/ClientCheque");
                //                if (!Directory.Exists(fileDirecory))
                //                {
                //                    Directory.CreateDirectory(fileDirecory);
                //                }
                //                var fileName = DateTime.Now.Ticks + "_" + ScannedDocument.FileName;
                //                var filepath = Path.Combine(fileDirecory, fileName);
                //                ScannedDocument.SaveAs(filepath);
                //                clientRecv.ScannedDocument = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "ClientCheque", fileName);
                //            }

                //            var totalAmountAvailable = Convert.ToDecimal(TotalamountPaid);

                //            foreach (string id in invoiceServiceIds)
                //            {

                //                var invoiceService = db.InvoiceServices.Where(x => x.InvoiceId == id).FirstOrDefault();
                //                ClientReceivableService clientSer = new ClientReceivableService();

                //                clientSer.Description = description;
                //                clientSer.ServiceId = invoiceService.Id;

                //                if (totalAmountAvailable > 0)
                //                {

                //                    if (Convert.ToDecimal(totalAmountAvailable) > Convert.ToDecimal(invoiceService.DueAmount))
                //                    {
                //                        totalAmountAvailable -= Convert.ToDecimal(invoiceService.DueAmount);
                //                        clientSer.PaidAmount = invoiceService.DueAmount;
                //                        invoiceService.DueAmount = 0;
                //                    }

                //                    else
                //                    {

                //                        invoiceService.DueAmount = invoiceService.DueAmount - totalAmountAvailable;
                //                        clientSer.PaidAmount = Convert.ToDecimal(totalAmountAvailable);
                //                        clientRecv.Balance = 0;
                //                        totalAmountAvailable = 0;
                //                    }

                //                    clientRecv.Balance = totalAmountAvailable;
                //                }

                //                else
                //                {
                //                    invoiceService.DueAmount = Convert.ToDecimal(invoiceService.DueAmount);
                //                    clientSer.PaidAmount = 0;
                //                }

                //                invoiceService.AmountPaidDate = Convert.ToDateTime(paymentDate);
                //                clientSer.ClientReceivable = clientRecv;
                //                db.ClientReceivableServices.Add(clientSer);
                //                invoiceService.ClientReceivable = clientRecv;
                //                db.Entry(invoiceService).State = System.Data.Entity.EntityState.Modified;
                //            }

                //            db.ClientReceivables.Add(clientRecv);
                //            db.SaveChanges();
                //        }
                return Ok();
            }

            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return InternalServerError();
            }

        }
        #endregion
    }
}