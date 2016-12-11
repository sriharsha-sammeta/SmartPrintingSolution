using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using SmartPrinting_POC.Models;
using System.Threading.Tasks;
using System.IO;
using SmartPrinting.DPAPI;
using System.Configuration;
using System.Diagnostics;
using System.Web.Http.Cors;

namespace SmartPrinting_POC.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PrintJobsController : ApiController
    {

        string numberOfCopiesText = @"featurebegin{
%%BeginNonPPDFeature: NumCopies #NumberOfCopies#
#NumberOfCopies#  /languagelevel where {pop languagelevel}{1} ifelse
2 ge { 1 dict dup /NumCopies 4 -1 roll put setpagedevice }{ userdict /#copies 3 -1 roll put } ifelse
%%EndNonPPDFeature
}featurecleanup";
        string rootPathToFolders = ConfigurationManager.AppSettings["BasePath"];
        private SmartPrinterContext db = new SmartPrinterContext();


        [HttpGet]
        [Route("api/printjob/print")]
        public IHttpActionResult Print(string userName, string printerName = @"\\HYD-VPRINT-01A.fareast.corp.microsoft.com\B2_WC5745_1FWAZ2")
        {
            try
            {
                if (userName == null || userName.Length == 0)
                {
                    return BadRequest("The supplied username is invalid");
                }
                else
                {
                    IQueryable<PrintJob> printjobs = db.PrintJobs.Where(p =>
                        (p.UserName == userName && p.Status == PrintJobStatus.Queued)
                        || (p.DelegatedTo == userName && p.Status == PrintJobStatus.Queued));

                    foreach (var printJob in printjobs)
                    {
                        string folderName = Path.Combine(rootPathToFolders, printJob.UserName);
                        if (File.Exists(Path.Combine(folderName, printJob.FileName)))
                        {
                            string encryptedFile = File.ReadAllText(Path.Combine(folderName, printJob.FileName));
                            string decryptedFile = DPAPI.Decrypt(encryptedFile);
                            File.WriteAllText(Path.Combine(folderName, printJob.FileName + "_tmp"), decryptedFile);
                            File.Copy(Path.Combine(folderName, printJob.FileName + "_tmp"), @printerName);
                            File.Delete(Path.Combine(folderName, printJob.FileName + "_tmp"));
                            printJob.Status = PrintJobStatus.Printed;
                            printJob.PrintedDateTime = DateTime.Now;

                            db.PrintLogs.Add(new PrintLog
                            {
                                UserName = userName,
                                FileName = printJob.FileName,
                                NumberOfCopies = printJob.NumberOfCopies,
                                DelegatedBy = printJob.DelegatedTo == userName ? printJob.UserName : null,
                                PrintedOn = DateTime.Now
                            });
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    db.SaveChanges();
                    db.Dispose();
                }
                return Ok();
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
          
        }

        [HttpPost]
        [Route("api/printjob/create/{userName}/{fileName}/{numberOfCopies}")]
        public IHttpActionResult Create(string userName, string fileName, int numberOfCopies, [FromBody] string fileContent)
        {
            try
            {
                if (userName == null || userName.Length == 0 || fileContent == null || fileContent.Length == 0)
                {
                    return BadRequest("Incorrect Username or File provided");
                }
                else
                {
                    if (File.Exists(Path.Combine(rootPathToFolders, userName, fileName)))
                    {
                        fileName += "2";
                    }
                    string encryptedFileContent = DPAPI.Encrypt(fileContent);
                    Directory.CreateDirectory(Path.Combine(rootPathToFolders, userName));
                    File.WriteAllText(Path.Combine(rootPathToFolders, userName, fileName), encryptedFileContent);

                    db.PrintJobs.Add(
                    new PrintJob
                    {
                        UserName = userName,
                        FileName = fileName,
                        NumberOfCopies = numberOfCopies,
                        DelegatedTo = null,
                        Status = PrintJobStatus.Queued,
                        CreatedDateTime = DateTime.Now,
                        PrintedDateTime = null
                    });
                    db.SaveChanges();
                    db.Dispose();
                    return Ok();
                }
            }
        catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);                
                return InternalServerError();
            }

        }

        [HttpGet]
        [Route("api/printjob/delete")]
        public IHttpActionResult Delete(string userName, string fileName)
        {
            try
            {
                if (userName == null || userName.Length == 0 || fileName == null || fileName.Length == 0)
                {
                    return BadRequest("The supplied username or filename is invalid");
                }
                else
                {
                    PrintJob printJob = db.PrintJobs.Where(p => p.UserName == userName && p.FileName == fileName).FirstOrDefault();
                    if (printJob == null)
                    {
                        return BadRequest("No printjob exists for the given username and filename");
                    }
                    else
                    {
                        db.PrintJobs.Remove(printJob);
                        db.SaveChanges();
                        string folderName = Path.Combine(rootPathToFolders, userName);

                        if (File.Exists(Path.Combine(folderName, printJob.FileName)))
                        {
                            File.Delete(Path.Combine(folderName, printJob.FileName));
                        }

                        return Ok();
                    }
                }
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }
       
        [HttpGet]
        [Route("api/printjob/updateNumberOfCopies")]
        public IHttpActionResult UpdateNumberOfCopies(string userName, string fileName, int numberOfCopies)
        {
            try
            {
                if (userName == null || userName.Length == 0 || fileName == null || fileName.Length == 0)
                {
                    return BadRequest("Incorrect username or filename provided");
                }
                else if (numberOfCopies <= 0)
                {
                    return BadRequest("Invlaid number of copies");
                }
                else
                {
                    PrintJob printJob = db.PrintJobs.Where(p => p.UserName == userName && p.FileName == fileName).FirstOrDefault();
                    if (printJob == null)
                    {
                        return BadRequest("Incorrect username or filename provided. No corresponding printjob exists.");
                    }
                    else
                    {
                        string filePath = Path.Combine(rootPathToFolders, userName, fileName);
                        if (File.Exists(filePath))
                        {
                            string textData = DPAPI.Decrypt(File.ReadAllText(filePath));
                            int copiesCountOffset = 10;
                            int copies = textData.IndexOf("NumCopies") + copiesCountOffset;
                            int copiesEnd = textData.IndexOf("\r", copies);
                            if (copies == 9)
                            {
                                numberOfCopiesText = numberOfCopiesText.Replace("#NumberOfCopies#", numberOfCopies.ToString());
                                int startPos = textData.LastIndexOf("featurebegin") - 1;
                                textData = textData.Substring(0, startPos) + numberOfCopiesText + textData.Substring(startPos);
                            }
                            else
                            {
                                string actualNumberOfCopiesText = numberOfCopiesText.Replace("#NumberOfCopies#", printJob.NumberOfCopies.ToString());
                                string updatedCopies = numberOfCopiesText.Replace("#NumberOfCopies#", numberOfCopies.ToString().ToString());
                                textData = textData.Replace(actualNumberOfCopiesText, updatedCopies);
                            }
                            File.WriteAllText(filePath, DPAPI.Encrypt(textData));
                        }
                        printJob.NumberOfCopies = numberOfCopies;
                        db.SaveChanges();
                    }
                }
                return Ok();
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("api/printjob/changestate")]
        public IHttpActionResult ChangeState(string userName, string fileName, PrintJobStatus status)
        {
            try
            {
                if (userName == null || userName.Length == 0 || fileName == null || fileName.Length == 0)
                {
                    return BadRequest("Incorrect username or filename provided");
                }
                PrintJob printJob = db.PrintJobs.Where(p => p.UserName == userName && p.FileName == fileName).FirstOrDefault();
                if (printJob == null)
                {
                    return BadRequest("Incorrect username or filename provided. No corresponding printjob exists.");
                }
                if (status == PrintJobStatus.Printed)
                {
                    return BadRequest("Invalid operation. Cannot set status to printed manually");
                }
                if (printJob.Status == PrintJobStatus.Printed && status == PrintJobStatus.Pending)
                {
                    return BadRequest("The status cannot be changed directly from printed to pending");
                }
                printJob.Status = status;
                db.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }

        }


        [HttpGet]
        [Route("api/printjob/assignproxy")]
        public IHttpActionResult AssignProxy(string userName, string fileName, string proxyUserName)
        {
            try
            {
                if (userName == null || userName.Length == 0 || fileName == null || fileName.Length == 0)
                {
                    return BadRequest("Incorrect username or filename provided");
                }
                PrintJob printJob = db.PrintJobs.Where(p => p.UserName == userName && p.FileName == fileName).FirstOrDefault();
                if (printJob == null)
                {
                    return BadRequest("Incorrect username or filename provided. No corresponding printjob exists.");
                }
                Models.Delegate delegateRecord = db.Delegates.Where(d => (d.UserName == userName && d.TrustedUser == proxyUserName) || (d.UserName == proxyUserName && d.TrustedUser == userName)).FirstOrDefault();
                if (delegateRecord == null)
                {
                    return BadRequest("The assigned proxy is not in the trusted zone of the user. Please add to trusted zone and try again");
                }
                printJob.DelegatedTo = proxyUserName;
                db.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }
       
        [HttpGet]
        [Route("api/printjob/getlog")]
        public IHttpActionResult GetLog(string userName)
        {
            try
            {
                if (userName == null || userName.Length == 0)
                {
                    return BadRequest("The supplied username is invalid");
                }
                List<PrintLog> logs = db.PrintLogs.Where(l => l.UserName == userName || l.DelegatedBy == userName).ToList();
                return Ok(logs);
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("api/printjob/all")]        
        public IHttpActionResult GetAllPrintJobs(string userName)
        {
            try
            {
                if (userName == null || userName.Length == 0)
                {
                    return BadRequest("The supplied username is invalid");
                }
                else
                {
                    IQueryable<PrintJob> printJobs = db.PrintJobs.Where(p => p.UserName == userName || p.DelegatedTo == userName);
                    if (printJobs == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        return Ok(printJobs.AsEnumerable<PrintJob>());
                    }
                }
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("api/delegate/all")]
        public IHttpActionResult GetAllDelegates(string userName)
        {
            try
            {
                if (userName == null || userName.Length == 0)
                {
                    return BadRequest("The supplied username is invalid");
                }
                else
                {
                    IQueryable<Models.Delegate> delegates = db.Delegates.Where(d => (d.UserName == userName) || (d.TrustedUser == userName));
                    if (delegates == null)
                    {
                        return Ok();
                    }
                    else
                    {
                        return Ok(delegates.AsEnumerable());
                    }
                }
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("api/delegate/accepted")]
        public IHttpActionResult GetAcceptedDelegates(string userName)
        {
            try
            {
                if (userName == null || userName.Length == 0)
                {
                    return BadRequest("The supplied username is invalid");
                }
                else
                {
                    IQueryable<Models.Delegate> delegates = db.Delegates.Where(d => (d.UserName == userName && d.Status == DelegateStatus.Accepted) || (d.TrustedUser == userName && d.Status == DelegateStatus.Accepted));
                    if (delegates == null)
                    {
                        return Ok();
                    }
                    else
                    {
                        return Ok(delegates.AsEnumerable());
                    }
                }
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("api/delegate/pending")]
        public IHttpActionResult GetPendingDelegates(string userName)
        {
            try
            {
                if (userName == null || userName.Length == 0)
                {
                    return BadRequest("The supplied username is invalid");
                }
                else
                {
                    IQueryable<Models.Delegate> delegates = db.Delegates.Where(d => (d.UserName == userName && d.Status == DelegateStatus.Pending) || (d.TrustedUser == userName && d.Status == DelegateStatus.Pending));
                    if (delegates == null)
                    {
                        return Ok();
                    }
                    else
                    {
                        return Ok(delegates.AsEnumerable());
                    }
                }
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("api/delegate/rejected")]
        public IHttpActionResult GetRejectedDelegates(string userName)
        {
            try
            {
                if (userName == null || userName.Length == 0)
                {
                    return BadRequest("The supplied username is invalid");
                }
                else
                {
                    IQueryable<Models.Delegate> delegates = db.Delegates.Where(d => (d.UserName == userName && d.Status == DelegateStatus.Rejected) || (d.TrustedUser == userName && d.Status == DelegateStatus.Rejected));
                    if (delegates == null)
                    {
                        return Ok();
                    }
                    else
                    {
                        return Ok(delegates.AsEnumerable());
                    }
                }
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("api/delegate/add")]
        public IHttpActionResult AddNewDelegate(string userName, string trustedUserName)
        {
            try
            {
                if (userName == null || userName.Length == 0 || trustedUserName == null || trustedUserName.Length == 0)
                {
                    return BadRequest("The supplied username(s) are invalid");
                }
                else
                {
                    Models.Delegate delegates = db.Delegates.Where(d => (d.UserName == userName && d.TrustedUser == trustedUserName) || (d.TrustedUser == userName && d.UserName == trustedUserName)).FirstOrDefault();
                    if (delegates != null)
                    {
                        return BadRequest("The two users are already in the same trusted zone");
                    }
                    else
                    {
                        db.Delegates.Add(new Models.Delegate() { UserName = userName, TrustedUser = trustedUserName, Status = DelegateStatus.Pending });
                        db.SaveChanges();
                        return Ok();
                    }
                }
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("api/delegate/changeState")]
        public IHttpActionResult ChangeDelegateState(string userName, string trustedUserName, DelegateStatus state)
        {
            try
            {
                if (userName == null || userName.Length == 0 || trustedUserName == null || trustedUserName.Length == 0)
                {
                    return BadRequest("The supplied username(s) are invalid");
                }
                else
                {
                    Models.Delegate delegates = db.Delegates.Where(d => (d.UserName == userName && d.TrustedUser == trustedUserName) || (d.TrustedUser == userName && d.UserName == trustedUserName)).FirstOrDefault();
                    if (delegates == null)
                    {
                        return BadRequest("The trusted zone relationship does not yet exist");
                    }
                    else if (delegates.Status == DelegateStatus.Rejected)
                    {
                        return BadRequest("The trusted zone is in rejected state, hence state cannot be changed");
                    }
                    else if (delegates.Status == DelegateStatus.Accepted && state == DelegateStatus.Pending)
                    {
                        return BadRequest("The trusted zone is in accepted state, hence can't change back to pending");
                    }
                    else
                    {
                        delegates.Status = state;
                        db.SaveChanges();
                        return Ok();
                    }
                }
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        [Route("api/printjob/active")]
        public IHttpActionResult GetActivePrintJobs(string userName)
        {
            try
            {
                if (userName == null || userName.Length == 0)
                {
                    return BadRequest("The supplied username is invalid");
                }
                else
                {
                    IQueryable<PrintJob> printJobs = db.PrintJobs.Where(p => p.UserName == userName || p.DelegatedTo == userName && (p.Status == PrintJobStatus.Pending || p.Status == PrintJobStatus.Queued));
                    if (printJobs == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        return Ok(printJobs.AsEnumerable<PrintJob>());
                    }
                }
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("api/printjob/completed")]
        public IHttpActionResult GetCompletedPrintJobs(string userName)
        {
            try
            {
                if (userName == null || userName.Length == 0)
                {
                    return BadRequest("The supplied username is invalid");
                }
                else
                {
                    IQueryable<PrintJob> printJobs = db.PrintJobs.Where(p => p.UserName == userName || p.DelegatedTo == userName && (p.Status == PrintJobStatus.Printed));
                    if (printJobs == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        return Ok(printJobs.AsEnumerable<PrintJob>());
                    }
                }
            }
            catch (Exception e)
            {
                string sSource;
                string sLog;
                sSource = "Smart Printer Service";
                sLog = "Application";
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                string errorMessage = e.Message + "\n\n";
                while (e.InnerException != null)
                {
                    errorMessage += e.InnerException + "\n";
                    e = e.InnerException;
                }
                EventLog.WriteEntry(sSource, errorMessage, EventLogEntryType.Error);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("api/test")]
        public IHttpActionResult Test()
        {
            return Ok("Works Good!");
        }
    }
}