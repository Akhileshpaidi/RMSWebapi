using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("tpaexternalmappeduser")]
    public class tpaexternalmappedusersmodel
    {
        [Key]

        public int tpaexternalmappeduserid   { get; set; }

        public int  tpaenityid { get; set; }

        public int externaluserid {  get; set; }
        public string tpaenitydescription {  get; set; }
        public string  externalstatus {  get; set; }
        public string externalcreateddate { get; set; }
       

    }
}
