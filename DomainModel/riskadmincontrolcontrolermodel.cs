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
    [Table("riskcontrol")]
    public class  riskadmincontrolcontrolermodel
    {

        [Key]
        public int controlid { get; set; }

        public string controlname {  get; set; }

        public string controlDesc {  get; set; }

        public string createddate { get; set;  }

        public string status { get; set; }

        public string isImported { get; set; }
        public int createdby {  get; set; }
    }
}
