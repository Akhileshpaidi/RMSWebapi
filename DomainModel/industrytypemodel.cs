using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("industrytype")]
    public class  industrytypemodel
    {
        [Key]
        public int industrytypeid { get; set; }

        public string industrytypename { get; set; }


        public string industrytypedescription { get; set; }


        public string createddate {  get; set; }

        public int createdBy {  get; set; }

        public string status { get; set; }

        public string industrytyptable {  get; set; }
        public int businesssectorid { get; set; }
        public string source { get; set; }

    }
}
