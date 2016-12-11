using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SmartPrinting_POC.Models
{
    public class SmartPrinterContext: DbContext
    {
        public SmartPrinterContext() : base("name=SmartPrinterConnectionString") { }
        public DbSet<PrintJob> PrintJobs { get; set; }
        public DbSet<PrintLog> PrintLogs { get; set; }
        public DbSet<Delegate> Delegates { get; set; }
    }
}