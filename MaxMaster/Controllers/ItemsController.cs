using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using MaxMaster.Models;
using Microsoft.AspNet.Identity;
using MaxMaster.ViewModels;
using System.Data.Entity;
using Newtonsoft.Json;
using System.IO;
using System.Configuration;
using System.Text;

namespace MaxMaster.Controllers
{
    public class ItemsController : ApiController
    {
        public static string GenerateRandomNumber(int length)
        {
            Random generator = new Random();
            var r = generator.Next(1000, 100000).ToString();
            gotoRandom:
            using (MaxMasterDbEntities db = new MaxMasterDbEntities())
            {
                var item = db.Items.Where(x => x.SerialNumber == r).FirstOrDefault();
                if (item != null)
                {
                    goto gotoRandom;
                }
                return r;
            }
        }

        #region "Add Item Model"
        [HttpPost]
        public IHttpActionResult AddModel()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;

                    ItemsMaster im = new ItemsMaster();
                    im.Brand = form.Get("Brand");
                    im.ModelNumber = form.Get("ModelNumber");
                    im.ItemName = form.Get("ItemName");
                    im.HSNCode = form.Get("HsnCode");
                    im.UPC = form.Get("Upc");
                    im.EAN = form.Get("Ean");
                    im.Description = form.Get("Description");
                    im.ItemContents = form.Get("Contents");
                    im.MadeIn = Convert.ToInt32(form.Get("Country"));
                    im.Manufacturer = form.Get("Manufacturer");

                    if (form.Get("PowerInput") != "")
                    {
                        im.PowerInput = Convert.ToInt32(form.Get("PowerInput"));
                        im.PowerInputUnits = form.Get("PowerUnits");
                    }

                    im.ThresholdQuantity = Convert.ToInt32(form.Get("ThresholdValue"));
                    im.UpdatedBy = User.Identity.GetUserId();
                    im.SrlNoExists = Convert.ToBoolean(form.Get("SrlNoExits"));
                    im.LastUpdated = MasterDataController.GetIndianTime(DateTime.Now);
                    im.Units = form.Get("Units");

                    decimal rate = Convert.ToDecimal(form.Get("Gst"));

                    im.GST = rate / 2;
                    im.CGST = rate / 2;

                    db.ItemsMasters.Add(im);
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

        #region "Get Models List"
        [HttpGet]
        public IHttpActionResult GetItemModels(string itemName, string modelNumber, string brand, int? quantity, string quanitySymbol, int? threshold, string thresholdSymbol, int? page, int? count)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var itemModels = db.GetItemModels(itemName, modelNumber, brand, quantity, quanitySymbol, threshold, thresholdSymbol, page, count).OrderBy(x => x.ItemName).ToList();
                    int totalCount = 0;
                    if (itemModels.Count > 0)
                    {
                        totalCount = (int)itemModels.FirstOrDefault().TotalCount;
                    }
                    return Content(HttpStatusCode.OK, new { itemModels, totalCount });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error ocuured, please try again later");
            }
        }
        #endregion

        #region "Get Model for edit"
        [HttpGet]
        public IHttpActionResult GetItemModel(int Id)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var model = db.ItemsMasters.Where(x => x.Id == Id).FirstOrDefault();

                    ItemMasterModel itemModel = new ItemMasterModel();

                    itemModel.ItemName = model.ItemName;
                    itemModel.ModelNum = model.ModelNumber;
                    itemModel.Manufacturer = model.Manufacturer;
                    itemModel.CountryId = model.MadeIn;
                    itemModel.EAN = model.EAN;
                    itemModel.UPC = model.UPC;
                    itemModel.PowerInput = model.PowerInput;
                    itemModel.PowerInputUnits = model.PowerInputUnits;
                    itemModel.Units = model.Units;
                    itemModel.Description = model.Description;
                    itemModel.Contents = model.ItemContents;
                    itemModel.Country = model.Country.Name;
                    itemModel.HSNCode = model.HSNCode;
                    itemModel.Brand = model.Brand;
                    itemModel.ThresholdValue = model.ThresholdQuantity;
                    itemModel.SerialNoExists = model.SrlNoExists;
                    itemModel.GSTRate = model.GST + model.CGST;

                    var gst = db.GSTs.Where(x => x.Rate == itemModel.GSTRate).FirstOrDefault();
                    var cpOfItem = db.ItemPrices.Where(x => x.ItemModelId == Id && x.ToDate == null).FirstOrDefault();

                    if (cpOfItem != null)
                    {
                        itemModel.CostPrice = cpOfItem.DealerPrice;
                    }
                    if (gst != null)
                    {
                        itemModel.GSTID = gst.Id;
                        itemModel.GST = gst.GSTName + "[" + itemModel.GSTRate + "%]";
                        itemModel.GSTRate = gst.Rate;
                    }

                    return Content(HttpStatusCode.OK, new { itemModel });

                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region "Update Model"
        [HttpPost]
        public IHttpActionResult UpdateItemModel(int id)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;

                    ItemsMaster im = db.ItemsMasters.Find(id);
                    im.Brand = form.Get("Brand");
                    im.ModelNumber = form.Get("ModelNumber");
                    im.ItemName = form.Get("ItemName");
                    im.HSNCode = form.Get("HsnCode");
                    im.UPC = form.Get("Upc");
                    im.EAN = form.Get("Ean");
                    im.Description = form.Get("Description");
                    im.ItemContents = form.Get("Contents");
                    im.MadeIn = Convert.ToInt32(form.Get("Country"));
                    im.Manufacturer = form.Get("Manufacturer");
                    // im.PowerInput = Convert.ToInt32(form.Get("PowerInput"));
                    int? powerUnit = im.PowerInput;
                    if (form.Get("PowerInput") != "")
                    {
                        im.PowerInput = Convert.ToInt32(form.Get("PowerInput"));
                        im.PowerInputUnits = form.Get("PowerUnits");
                    }
                    else
                    {
                        im.PowerInput = null;
                        im.PowerInputUnits = null;
                    }

                    im.ThresholdQuantity = Convert.ToInt32(form.Get("ThresholdValue"));
                    im.UpdatedBy = User.Identity.GetUserId();
                    im.LastUpdated = MasterDataController.GetIndianTime(DateTime.Now);
                    im.Units = form.Get("Units");
                    im.GST = Convert.ToDecimal(form.Get("Gst")) / 2;
                    im.CGST = Convert.ToDecimal(form.Get("Gst")) / 2;
                    im.SrlNoExists = Convert.ToBoolean(form.Get("SrlNoExits"));
                    db.Entry(im).State = EntityState.Modified;
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

        #region "Get Items from selected item model "
        [HttpGet]
        public IHttpActionResult GetItems(int modelId, string serialNum, string batchNum, string macAddress, int? page, int? count)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    int totalCount = 0;
                    var items = db.GetItems(modelId, serialNum, batchNum, macAddress, page, count).OrderBy(x=>x.SerialNumber).ToList();
                    if (items.Count > 0)
                    {
                        totalCount = (int)items.FirstOrDefault().TotalCount;
                    }

                    return Content(HttpStatusCode.OK, new { items, totalCount });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try agin later");
            }
        }
        #endregion

        #region "Add item(s) through bill"
        [HttpPost]
        public IHttpActionResult AddItem()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;

                    var items = JsonConvert.DeserializeObject<List<ItemModel>>(form.Get("items"));
                    var warrenty = form.Get("warrenty");
                    var serialNum = form.Get("serialNumberExists");

                    for (int i = 0; i < items.Count; i++)
                    {
                        Item item;
                        if (items[i].Id == "")
                        {
                            item = new Item();
                        }
                        else
                        {
                            int itemId = Convert.ToInt32(items[i].Id);
                            var it = db.Items.Where(x => x.Id == itemId).FirstOrDefault();
                            item = it;
                        }

                        item.ItemModelId = Convert.ToInt32(form.Get("modelNo"));
                        item.BatchNumber = form.Get("batchNumber");
                        item.StockLocationId = Convert.ToInt32(form.Get("stockLocation"));
                        item.StockInDate = Convert.ToDateTime(form.Get("stockInDate"));
                        item.CostPrice = Convert.ToDecimal(form.Get("cp"));
                        item.MinimumSP = Convert.ToDecimal(form.Get("minSp"));
                        item.MRP = Convert.ToDecimal(form.Get("mrp"));
                        item.PONumber = form.Get("poNumber");
                        item.HardwareVersion = form.Get("hardwareVersion");
                        item.SellingPrice = Convert.ToDecimal(form.Get("sp"));
                        item.SupplierId = form.Get("supplierId");

                        item.MACAddress = items[i].MacAddress;
                        item.StockReceiverUserId = User.Identity.GetUserId();
                        item.UpdatedBy = User.Identity.GetUserId();
                        if (items[i].ManufacturedDate != null)
                        {
                            item.ManufacturedDate = Convert.ToDateTime(items[i].ManufacturedDate);
                        }

                        item.LastUpdated = DateTime.Now;
                        item.IsAvailable = true;
                        item.BillNumber = form.Get("billNumber");

                        if (serialNum == "true")
                        {
                            item.SerialNumber = items[i].SerialNo;
                        }
                        else
                        {
                            if (items[i].SerialNo == "")
                            {
                                item.SerialNumber = GenerateRandomNumber(5);
                            }
                            else
                            {
                                item.SerialNumber = items[i].SerialNo;
                            }
                        }

                        decimal itemTaxRate = Convert.ToDecimal(form.Get("gst"));

                        item.GST = Convert.ToDecimal(itemTaxRate / 2);
                        item.CGST = Convert.ToDecimal(itemTaxRate / 2);

                        if (warrenty != null)
                        {
                            int duration = Convert.ToInt32(form.Get("warrenty"));
                            var type = form.Get("warrentyDuration");
                            item.Warrenty = duration;
                            item.WarrentyPeriod = form.Get("warrentyDuration");
                        }

                        var itemPrice = db.ItemPrices.Where(x => x.ItemModelId == item.ItemModelId && x.ToDate == null).FirstOrDefault();

                        if (itemPrice != null)
                        {
                            if (item.SellingPrice != itemPrice.SellingPrice)
                            {
                                itemPrice.ToDate = DateTime.Now;
                                db.Entry(itemPrice).State = EntityState.Modified;

                                ItemPrice ip = new ItemPrice();

                                ip.ItemModelId = item.ItemModelId;
                                ip.FromDate = DateTime.Now;
                                ip.SellingPrice = item.SellingPrice;
                                ip.DealerPrice = item.CostPrice;

                                db.ItemPrices.Add(ip);
                            }
                        }
                        else
                        {
                            ItemPrice ip = new ItemPrice();
                            ip.FromDate = DateTime.Now;
                            ip.SellingPrice = item.SellingPrice;
                            ip.ItemModelId = item.ItemModelId;
                            ip.DealerPrice = item.CostPrice;
                            db.ItemPrices.Add(ip);
                        }

                        if (items[i].Id == "")
                        {
                            db.Items.Add(item);
                        }
                        else
                        {
                            db.Entry(item).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                    }

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

        #region "Add Bill"
        [HttpPost]
        public IHttpActionResult AddBill()
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var BilledItemsList = JsonConvert.DeserializeObject<List<BilledItemsModel>>(form.Get("items"));
                    var roundOffNumber = form.Get("roundOffNumber");
                    Bill bill = new Bill();

                    bill.BillDate = Convert.ToDateTime(form.Get("billDate"));
                    bill.BillNumber = form.Get("billNum");
                    bill.PoNumber = form.Get("poNum");
                    if (form.Get("paymentTerms") != null)
                    {
                        bill.PaymentTerms = form.Get("paymentTerms");
                    }

                    bill.Notes = form.Get("notes");
                    bill.SupplierId = (form.Get("supplierId"));
                    if (form.Get("dueDate") != "")
                    {
                        bill.DueDate = Convert.ToDateTime(form.Get("dueDate"));
                    }

                    bill.Subtotal = Convert.ToDecimal(form.Get("subtotal"));
                    var tds = form.Get("tdsId");
                    if (tds != null)
                    {
                        bill.TDSAmount = Convert.ToDecimal(form.Get("tdsAmount"));
                        bill.TDSId = Convert.ToInt32(form.Get("tdsId"));
                    }
                    if (roundOffNumber != "")
                    {
                        bill.RoundOffNumber = Convert.ToDecimal(roundOffNumber);
                    }
                    bill.Total = Convert.ToDecimal(form.Get("total"));
                    //if (form.Get("taxAmount") != "")
                    //{
                    //    bill.TotalTaxAmount = Convert.ToDecimal(form.Get("taxAmount"));
                    //}

                    if (form.Get("discountAmount") != "")
                    {
                        bill.Discount = Convert.ToDecimal(form.Get("discountAmount"));
                    }
                    if (form.Get("discountType") != null)
                    {
                        bill.DiscountType = form.Get("discountType");
                    }
                    bill.BalanceDue = Convert.ToDecimal(form.Get("total"));
                    if (DateTime.Now < bill.DueDate)
                    {
                        bill.Status = "Due";
                    }
                    else
                    {
                        bill.Status = "OverDue";
                    }
                    if (form.Get("courierCharges") != "")
                    {
                        bill.CourierCharges = Convert.ToDecimal(form.Get("courierCharges"));
                    }

                    var files = HttpContext.Current.Request.Files;
                    var fileAttachments = new List<HttpPostedFile>();
                    if (files.Count > 0)
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            fileAttachments.Add(files[i]);
                        }

                        foreach (var file in fileAttachments)
                        {
                            var fileDirecory = HttpContext.Current.Server.MapPath("~/BillAttachments");

                            if (!Directory.Exists(fileDirecory))
                            {
                                Directory.CreateDirectory(fileDirecory);
                            }

                            var fileName = file.FileName;
                            var filePath = Path.Combine(fileDirecory, fileName);
                            file.SaveAs(filePath);

                            BillAttachment Billatt = new BillAttachment();
                            Billatt.FileName = Path.GetFileNameWithoutExtension(file.FileName);
                            Billatt.AttachmentUrl = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "BillAttachments", fileName);

                            bill.BillAttachments.Add(Billatt);
                        }

                    }

                    for (int i = 0; i < BilledItemsList.Count(); i++)
                    {
                        ItemsBillMapping ibm = new ItemsBillMapping();

                        ibm.ModelId = BilledItemsList[i].ModelId;
                        ibm.PricePerUnit = Convert.ToDecimal(BilledItemsList[i].Price);
                        ibm.Quantity = BilledItemsList[i].Quantity;
                        ibm.GST = Convert.ToDecimal(BilledItemsList[i].Tax) / 2;
                        ibm.CGST = Convert.ToDecimal(BilledItemsList[i].Tax) / 2;
                        ibm.Description = BilledItemsList[i].Description;

                        //ItemPrice itemPrice = new ItemPrice();

                        bill.ItemsBillMappings.Add(ibm);
                    }

                    db.Bills.Add(bill);
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

        #region "Get Bills"
        [HttpGet]
        public IHttpActionResult GetBills(string supplierId, string billNumber, DateTime? billDate, DateTime? dueDate, int? page, int? count)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    int totalCount = 0;
                    var billsList = db.GetBills(supplierId, billNumber, billDate, dueDate, page, count).ToList();
                    var quantity = "";
                    List<BillModel> bills = new List<BillModel>();

                    for (int i = 0; i < billsList.Count(); i++)
                    {
                        BillModel bill = new BillModel();
                        bill.BillNumber = billsList[i].BillNumber;
                        bill.BillDate = billsList[i].BillDate;
                        bill.Supplier = billsList[i].Supplier;
                        bill.DueDate = billsList[i].DueDate;
                        bill.BillId = billsList[i].BillId;
                        bill.BillAmount = billsList[i].BillAmount;
                        bill.QuantityUpdated = billsList[i].QuantityAdded;
                        bill.TotalQuantity = billsList[i].TotalQuantity;

                        var items = db.ItemsBillMappings.Where(x => x.BillId == bill.BillId).Select(x => new { x.ItemsMaster.ItemName, x.Quantity, x.ItemsMaster.Units }).ToList();

                        string str = string.Empty;

                        if (items.Count() > 0)
                        {
                            foreach (var item in items)
                            {
                                if (item.Units.ToUpper() == "NUMBER")
                                {
                                    quantity = item.Quantity.ToString();
                                }
                                else
                                {
                                    quantity = "1";
                                }
                                str = str + item.ItemName + "[No of Items : " + quantity + "]" + ",";
                            }
                            str = str.Remove(str.Length - 1);
                        }

                        bill.Items = str;

                        bills.Add(bill);

                    }

                    if (billsList.Count > 0)
                    {
                        totalCount = (int)billsList.FirstOrDefault().TotalCount;
                    }

                    return Content(HttpStatusCode.OK, new { bills, totalCount });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try agin later");
            }
        }
        #endregion

        #region "Get items billed under given billid "
        [HttpGet]
        public IHttpActionResult GetBilledItems(int billId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var billedItems = db.GetBilledItems(billId).OrderBy(x => x.Id).ToList();
                    var bill = db.Bills.Find(billId);

                    //  var stock = db.Items.Where(x => x.BillNumber == billNumber).ToList();

                    decimal subtotal = 0;
                    decimal? taxAmount = 0;
                    decimal? tdsAmount = 0;
                    decimal? courierCharges = 0;
                    decimal? discount = 0;

                    BillModel billModel = new BillModel();

                    if (bill != null)
                    {
                        billModel.BillDate = bill.BillDate;
                        billModel.BillNumber = bill.BillNumber;
                        if (bill.CourierCharges != null)
                        {
                            billModel.CourierCharges = bill.CourierCharges;
                        }

                        billModel.DueDate = bill.DueDate;
                        // billModel.Id = bill.Id;
                        billModel.Notes = bill.Notes;
                        billModel.PaymentTerms = bill.PaymentTerms;
                        billModel.PONumber = bill.PoNumber;
                        billModel.SupplierId = bill.SupplierId;
                        billModel.Supplier = db.Clients.Where(x => x.AspNetUserId == bill.SupplierId).FirstOrDefault().Name;
                        billModel.DueAmount = bill.BalanceDue;
                        // billModel.TaxAmount = bill.TotalTaxAmount;
                        billModel.RoundOffNumber = bill.RoundOffNumber;
                    }


                    for (int j = 0; j < billedItems.Count(); j++)
                    {
                        subtotal += (billedItems[j].Quantity * billedItems[j].PricePerUnit);
                        billModel.TotalQuantity += billedItems[j].Quantity;
                    }

                    billModel.SubTotal = subtotal;

                    if (bill.TDSId != null)
                    {
                        billModel.TDSId = bill.TDSId;
                        billModel.TDSLabel = bill.TD.TaxName + "- [" + bill.TD.Rate + "%]";
                        billModel.TDSRate = bill.TD.Rate;
                        billModel.TDSAmount = bill.TDSAmount;
                        tdsAmount = billModel.TDSAmount;
                    }
                    else
                    {
                        tdsAmount = 0;
                    }

                    if (bill.CourierCharges != null)
                    {
                        billModel.CourierCharges = bill.CourierCharges;
                    }

                    if (bill.Discount != null)
                    {
                        if (bill.DiscountType == "Percent")
                        {
                            billModel.DiscountAmount = (bill.Discount * subtotal) / 100;
                        }
                        else
                        {
                            billModel.DiscountAmount = bill.Discount;
                        }
                        billModel.Discount = bill.Discount;
                        billModel.DiscountType = bill.DiscountType;
                    }

                    billModel.Total = bill.Total;

                    return Content(HttpStatusCode.OK, new { billedItems, billModel });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An error occured, please try again later");
            }
        }
        #endregion

        #region "Update Bill"
        [HttpPost]
        public IHttpActionResult UpdateBill(int billId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var form = HttpContext.Current.Request.Form;
                    var BilledItemsList = JsonConvert.DeserializeObject<List<BilledItemsModel>>(form.Get("items"));

                    Bill bill = db.Bills.Find(billId);

                    bill.BillDate = Convert.ToDateTime(form.Get("billDate"));
                    bill.BillNumber = form.Get("billNum");
                    bill.PoNumber = form.Get("poNum");
                    if (form.Get("paymentTerms") != null)
                    {
                        bill.PaymentTerms = form.Get("paymentTerms");
                    }

                    bill.Notes = form.Get("notes");
                    bill.SupplierId = (form.Get("supplierId"));
                    if (form.Get("dueDate") != "")
                    {
                        bill.DueDate = Convert.ToDateTime(form.Get("dueDate"));
                    }

                    bill.Subtotal = Convert.ToDecimal(form.Get("subtotal"));
                    var tds = form.Get("tdsId");
                    if (tds != null)
                    {
                        bill.TDSAmount = Convert.ToDecimal(form.Get("tdsAmount"));
                        bill.TDSId = Convert.ToInt32(form.Get("tdsId"));
                    }
                    else
                    {
                        bill.TDSId = null;
                        bill.TDSAmount = null;
                    }
                    bill.Total = Convert.ToDecimal(form.Get("total"));

                    //if (form.Get("taxAmount") != "")
                    //{
                    //    bill.TotalTaxAmount = Convert.ToDecimal(form.Get("taxAmount"));
                    //}
                    //else
                    //{
                    //    bill.TotalTaxAmount = null;
                    //}

                    if (form.Get("discountAmount") != "")
                    {
                        bill.Discount = Convert.ToDecimal(form.Get("discountAmount"));
                    }
                    else
                    {
                        bill.Discount = null;
                    }
                    if (form.Get("discountType") != null)
                    {
                        bill.DiscountType = form.Get("discountType");
                    }
                    else
                    {
                        bill.DiscountType = null;
                    }

                    if (form.Get("courierCharges") != "")
                    {
                        bill.CourierCharges = Convert.ToDecimal(form.Get("courierCharges"));
                    }
                    else
                    {
                        bill.CourierCharges = null;
                    }
                    if (form.Get("roundOffNumber") != "")
                    {
                        bill.RoundOffNumber = Convert.ToDecimal(form.Get("roundOffNumber"));
                    }
                    else
                    {
                        bill.RoundOffNumber = null;
                    }

                    bill.BalanceDue = Convert.ToDecimal(form.Get("total"));
                    if (DateTime.Now < bill.DueDate)
                    {
                        bill.Status = "Due";
                    }
                    else
                    {
                        bill.Status = "OverDue";
                    }


                    var files = HttpContext.Current.Request.Files;
                    var fileAttachments = new List<HttpPostedFile>();
                    if (files.Count > 0)
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            fileAttachments.Add(files[i]);
                        }

                        foreach (var file in fileAttachments)
                        {
                            var fileDirecory = HttpContext.Current.Server.MapPath("~/BillAttachments");

                            if (!Directory.Exists(fileDirecory))
                            {
                                Directory.CreateDirectory(fileDirecory);
                            }

                            var fileName = file.FileName;
                            var filePath = Path.Combine(fileDirecory, fileName);
                            file.SaveAs(filePath);

                            BillAttachment Billatt = new BillAttachment();
                            Billatt.FileName = Path.GetFileNameWithoutExtension(file.FileName);
                            Billatt.AttachmentUrl = Path.Combine(ConfigurationManager.AppSettings["ApiUrl"], "BillAttachments", fileName);

                            bill.BillAttachments.Add(Billatt);
                        }

                    }

                    // update items list 

                    var previouslyBilledItems = db.ItemsBillMappings.Where(x => x.BillId == bill.Id).ToList();

                    for (int i = 0; i < previouslyBilledItems.Count(); i++)
                    {
                        ItemsBillMapping item = db.ItemsBillMappings.Find(previouslyBilledItems[i].Id);
                        db.ItemsBillMappings.Remove(item);
                        //  db.SaveChanges();
                    }

                    for (int i = 0; i < BilledItemsList.Count(); i++)
                    {
                        ItemsBillMapping ibm = new ItemsBillMapping();
                        ibm.ModelId = BilledItemsList[i].ModelId;
                        ibm.PricePerUnit = Convert.ToDecimal(BilledItemsList[i].Price);
                        ibm.Quantity = BilledItemsList[i].Quantity;
                        ibm.GST = Convert.ToDecimal(BilledItemsList[i].Tax) / 2;
                        ibm.CGST = Convert.ToDecimal(BilledItemsList[i].Tax) / 2;
                        ibm.Description = BilledItemsList[i].Description;

                        bill.ItemsBillMappings.Add(ibm);

                        //   db.ItemsBillMappings.Add(ibm);

                    }
                    db.Entry(bill).State = EntityState.Modified;
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

        #region "Check if SerialNumber entered already exists"
        [HttpGet]
        public IHttpActionResult CheckIfSerialNumberExists(string serialNumber)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var Exists = db.Items.Where(x => x.SerialNumber == serialNumber).FirstOrDefault();

                    return Content(HttpStatusCode.OK, new { Result = Exists != null });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An errror occured, please try agian later");
            }
        }
        #endregion

        #region "Check if Bill number exists"
        [HttpGet]
        public IHttpActionResult CheckIfBillNumberExists(string billNum, string supplierId)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var Exists = db.Bills.Where(x => x.BillNumber == billNum && x.SupplierId == supplierId).FirstOrDefault();
                    return Content(HttpStatusCode.OK, new { Result = Exists != null });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An errror occured, please try agian later");
            }
        }
        #endregion

        #region "Get Model and Item details of Bill number "
        [HttpGet]
        public IHttpActionResult GetBilledModelInfo(int modelId, string billNumber, string supplier)
        {
            try
            {
                using (MaxMasterDbEntities db = new MaxMasterDbEntities())
                {
                    var itemDetails = db.Items.Where(x => x.ItemModelId == modelId && x.BillNumber == billNumber && x.SupplierId == supplier).ToList();

                    ItemModel itemInfo = new ItemModel();

                    if (itemDetails.Count() > 0)
                    {
                        itemInfo.BatchNo = itemDetails[0].BatchNumber;
                        itemInfo.HardwareVersion = itemDetails[0].HardwareVersion;
                        itemInfo.StockLocation = itemDetails[0].StockLocationId;
                        itemInfo.CP = itemDetails[0].CostPrice;
                        itemInfo.MinSP = itemDetails[0].MinimumSP;
                        itemInfo.SP = itemDetails[0].SellingPrice;
                        itemInfo.MRP = itemDetails[0].MRP;
                        itemInfo.Warrenty = itemDetails[0].Warrenty;
                        itemInfo.WarrentyPeriod = itemDetails[0].WarrentyPeriod;
                        itemInfo.StockInDate = itemDetails[0].StockInDate;
                        itemInfo.Units = itemDetails[0].ItemsMaster.Units;
                      //  itemInfo.SerialNumExists = itemDetails[0].ItemsMaster.SrlNoExists;

                        var loc = db.OrgansationLocations.Where(x => x.Id == itemInfo.StockLocation).FirstOrDefault();
                        itemInfo.Location = loc.AddressLine1 + ", " + loc.AddressLine2 + loc.City.Name + ", " + loc.City.State.Name + "," + loc.City.State.Country.Name + "," + loc.ZIP;

                        List<ItemDetails> items = new List<ItemDetails>();

                        for (int i = 0; i < itemDetails.Count(); i++)
                        {
                            ItemDetails item = new ItemDetails();
                            item.Id = itemDetails[i].Id;
                            item.SerialNumber = itemDetails[i].SerialNumber;
                            item.MacAddress = itemDetails[i].MACAddress;
                            item.ManufacturedDate = itemDetails[i].ManufacturedDate;

                            items.Add(item);
                        }

                        itemInfo.Items = items;

                    }
                    return Content(HttpStatusCode.OK, new { itemInfo, Result = itemDetails != null });
                }
            }
            catch (Exception ex)
            {
                new Error().logAPIError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString(), ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "An errror occured, please try agian later");
            }
        }
        #endregion
    }
}