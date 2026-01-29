using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;

namespace C8.eServices.Mvc.Controllers
{
    
    public class Section49Controller : Controller
    {
        private eServicesDbContext _context = new eServicesDbContext();
        private Helpers.BaseHelper _base = new BaseHelper();
        // GET: Section49
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.LetterType = new SelectList(_context.Section49Types.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Key", "Name");
            var SecFT = _context.Section49FTs.Where(o => o.IsActive && !o.IsDeleted).Select(c => new
            {
                id = c.Id,
                c.Address
            }).ToList();

            var SecST = _context.Section49STs.Where(o => o.IsActive && !o.IsDeleted).Select(c => new
            {
                id = c.Id,
                c.Address
            }).ToList();
            SecFT.AddRange(SecST);
            ViewBag.Address = new SelectList(SecFT, "Id", "Address");
            return View();
        }
        /*
         * string GenerationDate, string ValuationPlacedDate, int GVRNoth,
            string EffectiveDateFrom, string EffectiveDateTo, string PostalAccount,
            string PublishedDate, string GVRNo, string ImplementionDate, string Type
        */
        [HttpPost]
        public ActionResult Index(Section49ViewModel Sec49)
        {
            _base.Initialise(_context);
            if (!Request.IsAuthenticated)
            {
                TempData["Error"] = "Please login again";
                return RedirectToAction("Login", "Account");
            }
            //Format  parameters into Single Delimited String
            string enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                Sec49.GenerationDate, Sec49.ValuationPlacedDate, Sec49.EffectiveDateFrom, Sec49.EffectiveDateTo,
                Sec49.Address, Sec49.PublishedDate, Sec49.GVRNo, Sec49.ImplementionDate, Sec49.Type);
            //Encrypt the Single String to and Encrypted string e
            var e = new AesCrypto().Encrypt(enc);
            _context.Logs.Add(new Models.Log()
            {
                LogTypeId = 1,
                LogEntry = enc,
                ReferenceId = 1,
                ReferenceTypeId = 1,
                CreatedBySystemUserId = _base.SystemUser.Id,
                CreatedDateTime = DateTime.Now,
                ModifiedBySystemUserId = _base.SystemUser.Id,
                ModifiedDateTime= DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                IsLocked = false
            }); 
            _context.SaveChanges();
            
            // {e} is your encrypted String
            return RedirectToAction("PDFEngine", new { q = e });
        }
        public ActionResult PDFEngine(string q)
        {
            try
            {
                var fixq = q.Replace(" ", "+");
                AesCrypto aes = new AesCrypto();
                var decrypted = aes.Decrypt(q);
                var values = decrypted.Split('|');
                AppSetting app = _context.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.DownloadDomain);

                DateTime GenerationDate = Convert.ToDateTime(values[0]); 
                DateTime ValuationPlacedDate = Convert.ToDateTime(values[1]);
                DateTime EffectiveDateFrom = Convert.ToDateTime(values[2]);
                DateTime EffectiveDateTo = Convert.ToDateTime(values[3]);
                string Address = values[4];
                string PublishedDate = values[5];
                int GVRNo = Convert.ToInt32(values[6]);
                DateTime ImplementionDate = Convert.ToDateTime(values[7]);
                string Type = values[8];

                var GenType = _context.Section49Types.FirstOrDefault(x=>x.Key==Type);

                string Domain = app.Value;

                string Link = "Section49/GeneratePDFLetter?q=";
                string EncPT = string.Empty;
                string Encid = string.Empty;
                string ReceiptLink = string.Empty;
                string enc = string.Empty;

                string systemApp = string.Empty;
                string GenerationDateF = GenerationDate.ToLongDateString().Replace(GenerationDate.DayOfWeek.ToString()+", ","");
                string PostalOwner = string.Empty;
                string Postal1 = string.Empty;
                string Postal2 = string.Empty;
                string Postal3 = string.Empty;
                string PostalCode = string.Empty;
                string ValuationPlacedDateF = ValuationPlacedDate.ToLongDateString().Replace(ValuationPlacedDate.DayOfWeek.ToString() + ", ", "");
                string GVRNoth = PdfHelper.NumberToPos(GVRNo);
                string EffectiveDateFromF = EffectiveDateFrom.ToLongDateString().Replace(EffectiveDateFrom.DayOfWeek.ToString() + ", ", "");
                string EffectiveDateToF = EffectiveDateTo.ToLongDateString().Replace(EffectiveDateTo.DayOfWeek.ToString() + ", ", "");
                string PIN = string.Empty;
                string Property = string.Empty;
                string UnitNr = string.Empty;
                string SchemeName = string.Empty;
                string PropertyType = string.Empty;
                string OwnerName = string.Empty;
                string AddressF = string.Empty;
                string UseCodeDescription = string.Empty;
                string UseCode = string.Empty;
                string Cat = string.Empty;
                string RatingCategoryCode = string.Empty;
                string Extent = string.Empty;
                string PostalAccount = string.Empty;
                string VenusCode = string.Empty;
                string Value = string.Empty;
                string PublishedDateF = PublishedDate;
                string GVRNoF = PdfHelper.NumberToPosition(GVRNo);
                string ImplementionDateF = ImplementionDate.ToLongDateString().Replace(ImplementionDate.DayOfWeek.ToString() + ", ", "");
                string Signature = string.Empty;
                string pdfname = string.Empty;
                string receiptName = string.Empty;
                List<string> FilesView = new List<string>();
                string filV = string.Empty;
                if (Type == Section49Keys.Single)
                {
                    string ee = "Unknown Error";
                    return RedirectToAction("ErrorPage", new { message = ee });
                }
                else if( Type== Section49Keys.Bulk)
                {
                    var fullTitle = _context.Section49FTs.Where(x => x.IsActive == true && x.IsLocked == true && x.IsDeleted == false).ToList();
                    foreach (var ft in fullTitle)
                    {
                        systemApp = Section49Keys.FullTitle;
                        PostalOwner = (ft.PostalOwner != null) ? Convert.ToString(ft.PostalOwner) : string.Empty;
                        Postal1 = (ft.Postal1 != null) ? Convert.ToString(ft.Postal1) : string.Empty;
                        Postal2 = (ft.Postal2 != null) ? Convert.ToString(ft.Postal2) : string.Empty;
                        Postal3 = (ft.Postal3 != null) ? Convert.ToString(ft.Postal3) : string.Empty;
                        PostalCode = (ft.PostalCode != null) ? Convert.ToString(ft.PostalCode) : string.Empty;
                        PIN = (ft.PIN != null) ? Convert.ToString(ft.PIN) : string.Empty;
                        Property = (ft.Property != null) ? Convert.ToString(ft.Property) : string.Empty;
                        UnitNr = string.Empty;
                        SchemeName = string.Empty;
                        PropertyType = (ft.PropertyType != null) ? Convert.ToString(ft.PropertyType) : string.Empty;
                        OwnerName = (ft.OwnerName != null) ? Convert.ToString(ft.OwnerName) : string.Empty;
                        AddressF = (ft.Address != null) ? Convert.ToString(ft.Address) : string.Empty;
                        UseCodeDescription = (ft.UseCodeDescription != null) ? Convert.ToString(ft.UseCodeDescription) : string.Empty;
                        UseCode = (ft.UseCode != null) ? Convert.ToString(ft.UseCode) : string.Empty;
                        Cat = (ft.Cat != null) ? Convert.ToString(ft.Cat) : string.Empty;
                        RatingCategoryCode = (ft.RatingCategoryCode != null) ? Convert.ToString(ft.RatingCategoryCode) : string.Empty;
                        Extent = (ft.Extent != null) ? Convert.ToString(ft.Extent) : string.Empty;
                        PostalAccount = (ft.PostalAccount != null) ? Convert.ToString(ft.PostalAccount) : string.Empty;
                        VenusCode = (ft.VenusCode != null) ? Convert.ToString(ft.VenusCode) : string.Empty;
                        Value = (ft.Value != null) ? Convert.ToString(ft.Value) : string.Empty;
                        var root = Server.MapPath("~/Section49PDFs/FT");

                        pdfname = "Full Title_" + PostalAccount + ".pdf";
                        receiptName = "Section 49 Full Title Letter";

                        EncPT = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}|{26}|{27}|{28}|{29}|{30}|{31}|{32}|{33}|{34}",
                            systemApp, GenerationDateF, PostalOwner, Postal1, Postal2, Postal3, PostalCode, ValuationPlacedDateF, GVRNo, GVRNoth,
                            EffectiveDateFromF, EffectiveDateToF, PIN, Property, UnitNr, SchemeName, PropertyType, OwnerName, AddressF, UseCodeDescription, UseCode, Cat, RatingCategoryCode, Extent, PostalAccount, VenusCode, Value, PublishedDateF, GVRNoF, ImplementionDateF, Signature, root, pdfname, receiptName, Domain);
                        Encid = aes.Encrypt(EncPT);
                        ReceiptLink = string.Format("{0}{1}{2}", Domain, Link, Encid);

                        Section49FT FtLin = _context.Section49FTs.FirstOrDefault(x => x.Id == ft.Id);

                        FtLin.PDFLink = ReceiptLink;
                        FtLin.PDFGeneratedOn = DateTime.Now;
                        _context.Entry(FtLin).State = EntityState.Modified;

                        _context.SaveChanges();
                        string filePath = GeneratePDFLetter(Encid);
                        FilesView.Add(filePath);
                        filV += filePath + "|";

                    }
                    //var sectionalTitle = _context.Section49STs.Where(x => x.IsActive && !x.IsDeleted).ToList();
                    var sectionalTitle = _context.Section49STs.Where(x => x.IsActive == true && x.IsLocked == true && x.IsDeleted == false).ToList();

                    foreach(var st in sectionalTitle)
                    {

                        systemApp = Section49Keys.FullTitle;
                        PostalOwner = (st.PostalOwner != null) ? Convert.ToString(st.PostalOwner) : string.Empty;
                        Postal1 = (st.Postal1 != null) ? Convert.ToString(st.Postal1) : string.Empty;
                        Postal2 = (st.Postal2 != null) ? Convert.ToString(st.Postal2) : string.Empty;
                        Postal3 = (st.Postal3 != null) ? Convert.ToString(st.Postal3) : string.Empty;
                        PostalCode = (st.PostalCode != null) ? Convert.ToString(st.PostalCode) : string.Empty;
                        PIN = (st.PIN != null) ? Convert.ToString(st.PIN) : string.Empty;
                        Property = (st.Property != null) ? Convert.ToString(st.Property) : string.Empty;
                        UnitNr = (st.UnitNr != null) ? Convert.ToString(st.UnitNr) : string.Empty;
                        SchemeName = (st.SchemeName != null) ? Convert.ToString(st.SchemeName) : string.Empty;
                        PropertyType = (st.PropertyType != null) ? Convert.ToString(st.PropertyType) : string.Empty;
                        OwnerName = (st.OwnerName != null) ? Convert.ToString(st.OwnerName) : string.Empty;
                        AddressF = (st.Address != null) ? Convert.ToString(st.Address) : string.Empty;
                        UseCodeDescription = (st.UseCodeDescription != null) ? Convert.ToString(st.UseCodeDescription) : string.Empty;
                        UseCode = (st.UseCode != null) ? Convert.ToString(st.UseCode) : string.Empty;
                        Cat = (st.Cat != null) ? Convert.ToString(st.Cat) : string.Empty;
                        RatingCategoryCode = (st.RatingCategoryCode != null) ? Convert.ToString(st.RatingCategoryCode) : string.Empty;
                        Extent = (st.Extent != null) ? Convert.ToString(st.Extent) : string.Empty;
                        PostalAccount = (st.PostalAccount != null) ? Convert.ToString(st.PostalAccount) : string.Empty;
                        VenusCode = (st.VenusCode != null) ? Convert.ToString(st.VenusCode) : string.Empty;
                        Value = (st.Value != null) ? Convert.ToString(st.Value) : string.Empty;
                        var root = Server.MapPath("~/Section49PDFs/ST");

                        pdfname = "Sectional Title_" + PostalAccount + ".pdf";
                        receiptName = "Section 49 Sectional Title Letter";

                        EncPT = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}|{26}|{27}|{28}|{29}|{30}|{31}|{32}|{33}|{34}",
                            systemApp, GenerationDateF, PostalOwner, Postal1, Postal2, Postal3, PostalCode, ValuationPlacedDateF, GVRNo, GVRNoth,
                            EffectiveDateFromF, EffectiveDateToF, PIN, Property, UnitNr, SchemeName, PropertyType, OwnerName, AddressF, UseCodeDescription, UseCode, Cat, RatingCategoryCode, Extent, PostalAccount, VenusCode, Value, PublishedDateF, GVRNoF, ImplementionDateF, Signature, root, pdfname, receiptName, Domain);
                        Encid = aes.Encrypt(EncPT);
                        ReceiptLink = string.Format("{0}{1}{2}", Domain, Link, Encid);

                        Section49FT FtLin = _context.Section49FTs.FirstOrDefault(x => x.Id == st.Id);

                        FtLin.PDFLink = ReceiptLink;
                        FtLin.PDFGeneratedOn = DateTime.Now;
                        _context.Entry(FtLin).State = EntityState.Modified;

                        _context.SaveChanges();
                        string filePath = GeneratePDFLetter(Encid);
                        FilesView.Add(filePath);
                        filV += filePath + "|";
                    }
                    //filV += "#";
                    var ReturnLinkValue = string.Format("{0}{1}", Domain, "Section49/LetterBox");

                    ReturnLinkValue += "?q=" + filV;
                    //var encryptedLinks = new AesCrypto().Encrypt(filV);
                    //return RedirectToAction("FileBox", "Section49", new { q = encryptedLinks });

                    // var test2 = testUrl;
                    //Send Payment Notification
                    //return Redirect(ReturnLinkValue);
                    return RedirectToAction("LetterBox", "Section49");
                }
                else if( Type==Section49Keys.Batch)
                {
                    //TODO for Municipal Valuers
                    string ee = "Service Unavailable";
                    return RedirectToAction("ErrorPage", new { message = ee });
                }
                else
                {
                    //Exception Log
                    _context.Logs.Add(new Models.Log()
                    {
                        LogTypeId = 1,
                        LogEntry = "Error : In S49Letter Generation ENC id: {"+q+"}",
                        ReferenceId = 0,
                        ReferenceTypeId = 1,
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false
                    });
                    _context.SaveChanges();
                    string ee = "Unknown Error";
                    return RedirectToAction("ErrorPage", new { message = ee });
                }
                //if we got this far something went wrong
                string e = "No Reference number Found";
                return RedirectToAction("ErrorPage", new { message = e });
            }
            catch (Exception err)
            {
                string e = err.Message;
                return RedirectToAction("ErrorPage", new { message = e });
            }
        }


        //Receipt PDF Generator
        public string GeneratePDFLetter(string q)
        {
            try
            {
                var fixq = q.Replace(" ", "+");
                AesCrypto aes = new AesCrypto();
                var decrypted = aes.Decrypt(fixq);
                var values = decrypted.Split('|');
                string enc = string.Empty;
                if (values[0] == Section49Keys.FullTitle)
                {
                    enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}|{26}|{27}|{28}|{29}|{30}", values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8], values[9], values[10], values[11], values[12], values[13], values[14], values[15], values[16], values[17], values[18], values[19], values[20], values[21], values[22], values[23], values[24], values[25], values[26], values[27], values[28], values[29], values[30]);
                }
                else if (values[0] == Section49Keys.SectionalTitle)
                {
                    enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}|{26}|{27}|{28}|{29}|{30}", values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8], values[9], values[10], values[11], values[12], values[13], values[14], values[15], values[16], values[17], values[18], values[19], values[20], values[21], values[22], values[23], values[24], values[25], values[26], values[27], values[28], values[29], values[30]);
                }






                var e = new AesCrypto().Encrypt(enc);
                var root = values[31];
                string pdfname = values[32];
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                //string receiptView = pgMerchantQ.ReceiptView;
                string receiptName = values[33];
                //for Local
                //var urlDomain ="http://localhost:3456/";

                /*For Production Server*/
                var urlDomain = values[34];

                var url = string.Format("{0}Section49/Section49View?q={1}", urlDomain, Url.Encode(e));
                var actionPDF = new Rotativa.UrlAsPdf(url)
                {
                    FileName = receiptName,
                    //SaveOnServerPath = path, // JK.20200404a - Deprecated, save bytes as below.
                    PageSize = Rotativa.Options.Size.A4,
                    PageOrientation = Rotativa.Options.Orientation.Portrait,
                    PageMargins = { Left = 10, Right = 6, Top = 10 }
                };

                //byte[] applicationPDFData = actionPDF.BuildFile(ControllerContext);
                //// JK.20200404a - SaveOnServerPath is deprecated, have to save bytes manually.
                //System.IO.File.WriteAllBytes(path, applicationPDFData);

                //var fullpathtofile = path;
                //var mimetype = "application/pdf";
                //var filecontents = System.IO.File.ReadAllBytes(fullpathtofile);

                //return new FileContentResult(filecontents, mimetype);

                byte[] applicationPDFData = actionPDF.BuildFile(ControllerContext);
                var length = actionPDF.BuildFile(ControllerContext).Length;
                // JK.20200404a - SaveOnServerPath is deprecated, have to save bytes manually.
                System.IO.File.WriteAllBytes(path, applicationPDFData);

                var fullpathtofile = path;
                var mimetype = "application/pdf";
                var filecontents = System.IO.File.ReadAllBytes(fullpathtofile);
                var filePath = string.Format("{0}{1}{2}", urlDomain, root, pdfname);
                string FormatedFile = string.Format("{0}", filePath);
                //return new FileContentResult(filecontents, mimetype);
                return FormatedFile;
            }
            catch (Exception x)
            {
                _context.Logs.Add(new Log()
                {
                    LogTypeId = 1,
                    LogEntry = x.ToString(),
                    ReferenceId = 0,
                    ReferenceTypeId = 1,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                });
                _context.SaveChanges();
                //throw x;
            }

            return null;
        }
        public ActionResult ViewPDFLetter(string q)
        {
            try
            {
                var fixq = q.Replace(" ", "+");
                AesCrypto aes = new AesCrypto();
                var decrypted = aes.Decrypt(fixq);
                var values = decrypted.Split('|');
                string enc = string.Empty;
                if(values[0]==Section49Keys.FullTitle)
                {
                    enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}|{26}|{27}|{28}", values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8], values[9], values[10], values[11], values[12], values[13], values[16], values[17], values[18], values[19], values[20], values[21], values[22], values[23], values[24], values[25], values[26], values[27], values[28], values[29], values[30]);
                }
                else if(values[0] == Section49Keys.SectionalTitle)
                {
                    enc = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}|{26}|{27}|{28}|{29}|{30}", values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8], values[9], values[10], values[11], values[12], values[13], values[14], values[15], values[16], values[17], values[18], values[19], values[20], values[21], values[22], values[23], values[24], values[25], values[26], values[27], values[28], values[29], values[30]);
                }
                 



                var e = new AesCrypto().Encrypt(enc);
                var root = values[31];
                string pdfname = values[32];
                var path = System.IO.Path.Combine(root, pdfname);
                path = System.IO.Path.GetFullPath(path);
                //string receiptView = pgMerchantQ.ReceiptView;
                string receiptName = values[33];
                //for Local
                //var urlDomain ="http://localhost:3456/";

                /*For Production Server*/
                var urlDomain = values[34];


                var url = string.Format("{0}Section49/Section49View?q={1}", urlDomain, Url.Encode(e));
                var actionPDF = new Rotativa.UrlAsPdf(url)
                {
                    FileName = receiptName,
                    //SaveOnServerPath = path, // JK.20200404a - Deprecated, save bytes as below.
                    PageSize = Rotativa.Options.Size.A4,
                    PageOrientation = Rotativa.Options.Orientation.Portrait,
                    PageMargins = { Left = 10, Right = 6, Top = 10 }
                };

                //var actionPDF = new Rotativa.ActionAsPdf(receiptView, new { q = e })
                //{
                //    FileName = receiptName,
                //    SaveOnServerPath = path,
                //    PageSize = Rotativa.Options.Size.A4,
                //    PageOrientation = Rotativa.Options.Orientation.Portrait,
                //    PageMargins = { Left = 10, Right = 6, Top = 10 }

                //};

                byte[] applicationPDFData = actionPDF.BuildFile(ControllerContext);
                // JK.20200404a - SaveOnServerPath is deprecated, have to save bytes manually.
                System.IO.File.WriteAllBytes(path, applicationPDFData);

                var fullpathtofile = path;
                var mimetype = "application/pdf";
                var filecontents = System.IO.File.ReadAllBytes(fullpathtofile);

                return new FileContentResult(filecontents, mimetype);
            }
            catch (Exception x)
            {
                _context.Logs.Add(new Log()
                {
                    LogTypeId = 1,
                    LogEntry = x.ToString(),
                    ReferenceId = 0,
                    ReferenceTypeId = 1,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                });
                _context.SaveChanges();
                //throw x;
            }

            return null;
        }
        public ActionResult Section49View(string q)
        {
            try
            {
                AesCrypto aes = new AesCrypto();
                var fixq = q.Replace(" ", "+");
                var decrypted = aes.Decrypt(fixq);
                var values = decrypted.Split('|');
                var systemMerc = values[0];
                Section49Letter pgMerchantQ = _context.Section49Letters.FirstOrDefault(o => o.Key == systemMerc);

                var pageView = pgMerchantQ.LetterTemplate;
                for (int j = 0; j < values.Length; j++)
                {
                    pageView = pageView.Replace("@e[" + j + "]", values[j]);
                }
                ViewBag.Value = values;
                ViewBag.Test = pageView;

                return View();
            }
            catch (Exception x)
            {
                _context.Logs.Add(new Log()
                {
                    LogTypeId = 1,
                    LogEntry = x.ToString(),
                    ReferenceId = 0,
                    ReferenceTypeId = 1,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                });
                _context.SaveChanges();
                //throw x;
            }

            return View();

        }

        [HttpGet]
        public ActionResult LetterBox(string q)
        {
            //var values = q.Split('|');
            ViewBag.Value = "~/Section49PDFs";
            return View();
        }

        public ActionResult ErrorPage(string message)
        {
            ViewBag.pgErrMess = message;
            return View();
        } 
        public ActionResult EmtPage(string message)
        {
            ViewBag.pgErrMess = message;
            return View();
        }

        public ActionResult FileBox(string q)
        {
            List<string> links = new List<string>();
            AesCrypto aes = new AesCrypto();
            var fixq = q.Replace(" ", "+");
            var decrypted = aes.Decrypt(fixq);
            var values = decrypted.Split('|');
            foreach (var item in values)
            {
                if (!string.IsNullOrEmpty(item)||!string.IsNullOrWhiteSpace(item))
                {
                    links.Add(item);
                }          
            }
            ViewBag.doclist = links;
            //var values = q.Split('|');
            //ViewBag.Value = "~/Section49PDFs";
            return View();
        }
    }
}