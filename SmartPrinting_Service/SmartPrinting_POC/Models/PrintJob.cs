using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SmartPrinting_POC.Models
{
    [Table("PrintJob")]
    public class PrintJob
    {
        [Column("ID")]
        [Key]
        public int Id { get; set; }        
        [Column("UserName")]
        public string UserName { get; set; }
        [Column("FileName")]
        public string FileName { get; set; }
        [Column("NumberOfCopies")]
        public int NumberOfCopies{get;set;}
        [Column("DelegatedTo")]
        public string DelegatedTo { get; set; }
        [Column("Status")]
        public PrintJobStatus Status { get; set; }
        [Column("CreatedDateTime")]
        public DateTime? CreatedDateTime { get; set; }
        [Column("PrintedDateTime")]
        public DateTime? PrintedDateTime { get; set; }
    }
}