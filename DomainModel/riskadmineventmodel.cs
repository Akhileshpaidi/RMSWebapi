using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("riskeventfrequency")]
    public class riskadmineventmodel
    {

        [Key]
        public int eventfrequencyid { get; set; }

        public string eventfrequencyname { get; set; }

        public string eventfrequencyDesc { get; set; } 

        public string createddate { get; set; }

        public string status {  get; set; }

        public string isImported { get; set; }

        public int createdby {  get; set; }
    }
}
