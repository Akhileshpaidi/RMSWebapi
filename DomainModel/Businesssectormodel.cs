using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("Businesssector")]
    public class Businesssectormodel
    {
        [Key]
       public int   businesssectorid {  get; set; }

       public string businesssectorname { get; set; }
      public string businesssectordescriptio {  get; set; }
      public string createddate {  get; set; }

        public int createdBy {  get; set; }
         public string status { get; set; }

        public string businesssectortable { get; set; }
        public string source { get; set; }

    }
}
