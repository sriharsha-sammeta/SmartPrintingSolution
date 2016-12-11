using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SmartPrinting_POC.Models
{
    [Table("PrintLog")]
    public class PrintLog
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }        
        [Column("UserName")]
        public string UserName { get; set; }
        [Column("FileName")]
        public string FileName { get; set; }
        [Column("NumberOfCopies")]
        public int NumberOfCopies { get; set; }
        [Column("DelegatedBy")]
        public string DelegatedBy  { get; set; }
        [Column("PrintedOn")]
        public DateTime? PrintedOn { get; set; }      
    }
}