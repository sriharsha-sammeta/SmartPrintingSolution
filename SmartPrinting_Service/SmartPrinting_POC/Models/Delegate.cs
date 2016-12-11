using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SmartPrinting_POC.Models
{    
    [Table("Delegate")]
    public class Delegate
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Column("UserName")]
        public string UserName { get; set; }

        [Column("TrustedUser")]
        public string TrustedUser { get; set;}

        [Column("Status")]
        public DelegateStatus Status { get; set; }
    }
}