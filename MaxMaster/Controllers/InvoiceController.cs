using MaxMaster.Models;
using MaxMaster.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;


namespace MaxMaster.Controllers
{
    public class InvoiceController : ApiController
    {

        #region " Generate Invoice of client for the selected month "
        public IHttpActionResult GetClientInvoiceDetails(int ClientId, DateTime? fromdate, DateTime? todate, string invoiceId)
        {
            try
            {
                //using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                // {
                //     int linesTobeRemoved = 0;
                //     var unsortedClientDetails = db.MaxClientService_Details(ClientId, fromdate, todate).ToList();
                //     var clientDueAmount = db.MaxClientDueDetails(ClientId, invoiceId).FirstOrDefault();

                //     var unSortedDoctorDetails = db.Max_DoctorLineCount(ClientId, fromdate, todate).ToList();
                //     var clientLines = unSortedDoctorDetails.Where(x => x.Client_Id == ClientId).ToList();

                //     List<InvoiceModel> invoices = new List<InvoiceModel>();

                //     var clientServices = unsortedClientDetails.Where(x => x.ClientVertical_Id != null).GroupBy(x => x.ClientVertical_Id).Select(x => x.First()).ToList();

                //     foreach (var client in clientServices)
                //     {
                //         foreach (var doc in clientLines)
                //         {
                //             linesTobeRemoved += Convert.ToInt32(doc.MacroPercent * (doc.LineCount / client.NumberOfCharactersPerLine) / 100);
                //         }

                //         InvoiceModel invoice = new InvoiceModel();
                //         invoice.Client_Id = client.Client_Id;
                //         invoice.Client_Name = client.ClientName;
                //         invoice.Address = client.AddressLine1;
                //         invoice.ServiceType = client.ServiceType;
                //         invoice.ServiceId = Convert.ToInt32(client.ClientVertical_Id);
                //         invoice.PaymentType = client.PaymentType;
                //         invoice.RowNum = Convert.ToInt32(client.RowNum);
                //         invoice.LineCount = Math.Round(Convert.ToDouble(client.TotalCharacters / client.NumberOfCharactersPerLine) - linesTobeRemoved);
                //         invoice.ShortName = client.ShortName;

                //         if (client.PaymentType == "Per Unit")
                //         {
                //             invoice.UnitPrice = Convert.ToDecimal(client.PaymentAmount);
                //             invoice.UnitPriceforDisplay = (client.PaymentAmount).ToString();
                //             invoice.Amount = Convert.ToDecimal(Convert.ToDecimal(invoice.LineCount) * client.PaymentAmount);
                //         }
                //         if (client.PaymentType == "Fixed")
                //         {
                //             invoice.UnitPriceforDisplay = "Fixed";
                //             invoice.UnitPrice = Convert.ToDecimal(client.PaymentAmount);
                //             invoice.Amount = Convert.ToDecimal(client.PaymentAmount);
                //         }

                //         invoice.DueAmount = invoice.DueAmount;

                //         invoice.MonthYear = fromdate.Value.ToString("MMMM yyyy");

                //         bool GeneratedBy = db.Invoices.Any(x => x.Client_Id == ClientId && x.InvoiceCreatedDate.Value.Month == fromdate.Value.Month);
                //         invoice.GeneratedBy = GeneratedBy;

                //         bool ApprovedBy = db.Invoices.Any(x => x.Client_Id == ClientId && x.InvoiceCreatedDate.Value.Month == fromdate.Value.Month && x.InvoiceApprovedBy.HasValue);
                //         invoice.ApprovedBy = ApprovedBy;
                //         invoice.Addresses = unsortedClientDetails.GroupBy(x => x.LocationId).Select(x => x.First()).Select(x => new ClientAddressModel
                //         {

                //             Id = x.LocationId,
                //             Client_Id = x.Client_Id,
                //             Line1 = x.AddressLine1,
                //             City = x.City,
                //             State = x.StateName,
                //             Country = x.CountryName,
                //             IsInvoiceAddress = x.IsInvoiceAddress.GetValueOrDefault(),
                //             Zip = x.ZIP,
                //         }).OrderBy(x => x.IsInvoiceAddress).ToList();

                //         invoices.Add(invoice);

                //     }
                //     return Content(HttpStatusCode.OK, new { invoices, clientDueAmount });
                // }
                return Ok();
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return InternalServerError();
            }
        }
        #endregion

        #region " Get invoice data if approved"
        public IHttpActionResult GetApprovedInvc(string InvoiceId)
        {
            try

            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var approved = (from inv in db.Invoices where inv.Id == InvoiceId select new { inv.InvoiceApprovedBy }).FirstOrDefault();

                    return Content(HttpStatusCode.OK, new { approved });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return InternalServerError();
            }
        }

        #endregion

        #region " Add invoice to database "
        [HttpPost] 
        public IHttpActionResult AddInvoice()
        {
            try
            {
                var form = HttpContext.Current.Request.Form;
                var clientAddress = form.Get("ClientAddress");
                var dueAmount = form.Get("UnpaidBalance");
                var month = form.Get("InvoiceMonth");
                var clientToAddress = form.Get("ClientToAddress");

                if (dueAmount == "null")
                {
                    dueAmount = 0.ToString();
                }
                var CurrentAmount = form.Get("CurrentAmount");
                var serviceId = form.Get("ClientService");
                var createdBy = form.Get("CreatedBy");
                var invoiceSer = JsonConvert.DeserializeObject<List<string>>(form.Get("InvoiceServices"));

                using (MaxMasterDbEntities db = new MaxMasterDbEntities())

                {
                    Invoice inData = new Invoice();
                    InvoiceService inService = new InvoiceService();

                    inData.Client_Id = Convert.ToInt32(form.Get("Client_Id"));
                    inData.Id = form.Get("InvoiceId"); ;
                    inData.CompanyAddress = form.Get("CompanyAddress");
                    inData.ClientAddress = clientAddress;
                    inData.InvoiceCreatedBy = (from Emp in db.Employees where Emp.EmployeeNumber == createdBy select Emp.Id).FirstOrDefault();
                    inData.InvoiceCreatedDate = DateTime.Now;
                    inData.InvoiceMonth = month;
                    inData.InvoiceYear = Convert.ToInt32(form.Get("invoiceYear"));

                    string currency = (from client in db.Clients where client.Id == inData.Client_Id select client.Currency).FirstOrDefault();
                    inData.CurrencyType = currency;

                    inData.CurrentBillAmount = Convert.ToDecimal(CurrentAmount);
                    inData.TotalDueAmount = Convert.ToDecimal(dueAmount);
                    var totalInvoice = Convert.ToDecimal(CurrentAmount) + Convert.ToDecimal(dueAmount);
                    inData.TotalInvoiceAmount = Convert.ToDecimal(totalInvoice);
                    inData.Status = "Generated";

                    foreach (string id in invoiceSer)
                    {
                        string unitprice = (from client in db.Clients where client.Id == inData.Client_Id select client.PaymentAmount).FirstOrDefault().ToString();
                        inService.InvoiceId = inData.Id;
                        inService.ClientId = Convert.ToInt32(form.Get("Client_Id"));
                        inService.ServiceId = Convert.ToInt32(serviceId);
                        inService.UnitPrice = Convert.ToDecimal(form.Get("UnitPrice"));
                        inService.TotalLines = Convert.ToInt32(form.Get("TotalLines"));
                        inService.BillAmount = Convert.ToDecimal(CurrentAmount);
                        inService.DueAmount = inService.BillAmount;
                    }
                    db.Invoices.Add(inData);
                    db.InvoiceServices.Add(inService);
                    db.SaveChanges();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return InternalServerError();
            }
        }
        #endregion

        #region " Approve the generated invoice"
        [HttpPost]
        public IHttpActionResult ApproveInvoice(string InvoiceId)
        {
            try
            {
                //var form = HttpContext.Current.Request.Form;
                //var approvedBy = form.Get("ApprovedBy");

                //using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                //{
                //    ApprovedInvoice appInv = new ApprovedInvoice();
                //    ApprovedInvoiceService appInvSer = new ApprovedInvoiceService();

                //    var invoices = db.Invoices.First(x => x.Id == InvoiceId);
                //    var invoiceServices = db.InvoiceServices.Where(x => x.InvoiceId == InvoiceId).ToList();
                //    invoices.InvoiceApprovedBy = (from emp in db.Employees where emp.EmployeeNumber == approvedBy select emp.Id).FirstOrDefault();
                //    invoices.ApprovedDate = DateTime.Now;
                //    invoices.Status = "Approved";

                //    appInv.Id = invoices.Id;
                //    appInv.Client_Id = invoices.Client_Id;
                //    appInv.CompanyAddress = invoices.CompanyAddress;
                //    appInv.ClientAddress = invoices.ClientAddress;
                //    appInv.CurrencyType = invoices.CurrencyType;
                //    appInv.CurrentBillAmount = invoices.CurrentBillAmount;
                //    appInv.InvoiceCreatedBy = invoices.InvoiceCreatedBy;
                //    appInv.InvoiceCreatedDate = invoices.InvoiceCreatedDate;
                //    appInv.InvoiceApprovedBy = (from emp in db.Employees where emp.EmployeeNumber == approvedBy select emp.Id).FirstOrDefault();
                //    appInv.ApprovedDate = DateTime.Now;
                //    appInv.TotalDueAmount = invoices.TotalDueAmount;
                //    appInv.TotalInvoiceAmount = invoices.TotalInvoiceAmount;
                //    appInv.Status = "Generated";
                //    appInv.InvoiceMonth = form.Get("invoiceMonth");
                //    appInv.InvoiceYear = Convert.ToInt32(form.Get("invoiceYear"));

                //    for (int i = 0; i < invoiceServices.Count; i++)
                //    {
                //        appInvSer.InvoiceId = invoiceServices[i].InvoiceId;
                //        appInvSer.PaymentId = invoiceServices[i].PaymentId;
                //        appInvSer.ServiceId = invoiceServices[i].ServiceId;
                //        appInvSer.TotalLines = invoiceServices[i].TotalLines;
                //        appInvSer.UnitPrice = invoiceServices[i].UnitPrice;
                //        appInvSer.ClientId = invoiceServices[i].ClientId;
                //        appInvSer.DueAmount = invoiceServices[i].DueAmount;
                //        appInvSer.AmountPaidDate = invoiceServices[i].AmountPaidDate;
                //        appInvSer.BillAmount = invoiceServices[i].BillAmount;
                //    }
                //    db.Entry(invoices).State = System.Data.Entity.EntityState.Modified;
                //    db.ApprovedInvoices.Add(appInv);
                //    db.ApprovedInvoiceServices.Add(appInvSer);
                //    db.SaveChanges();
                //}
                return Ok();
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return InternalServerError();
            }
        }

        #endregion

        #region " Delete invoice from database"
        [HttpPost]
        public IHttpActionResult DeleteInvoice(string InvoiceId)
        {
            try
            {
                //var form = HttpContext.Current.Request.Form;
                //var description = form.Get("Description");
                //var invoiceId = form.Get("InvoiceId");

                //using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                //{
                //    var invoiceSer = db.InvoiceServices.First(x => x.InvoiceId == InvoiceId);
                //    var Inv = db.Invoices.First(x => x.Id == InvoiceId);

                //    if (InvoiceId == null)
                //    {
                //        return Content(HttpStatusCode.InternalServerError, "Invalid data, please try again!");
                //    }

                //    if (invoiceSer.PaymentId == null)
                //    {
                //        var appInvoices = db.ApprovedInvoices.First(x => x.Id == InvoiceId);
                //        appInvoices.Status = "Cancelled";
                //        appInvoices.Description = description.Trim();

                //        db.Entry(appInvoices).State = System.Data.Entity.EntityState.Modified;
                //        db.InvoiceServices.Remove(invoiceSer);
                //        db.Invoices.Remove(Inv);
                //        db.SaveChanges();

                //    }
                //    else
                //    {
                //        return Content(HttpStatusCode.InternalServerError, "Cant delete Invoice as payment is done.");
                //    }

                    return Ok();
               // }
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