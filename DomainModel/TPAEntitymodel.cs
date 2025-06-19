using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("tpaenity")]

    public class TPAEntitymodel
    {
        [Key]

        public int tpaenityid { get; set; }

        public string tpaenityname { get; set; }


        public string tpadescription {  get; set; }

        public string tpaaddress {  get; set; }
       public string  createddate { get; set; }

        public string status { get; set; }  
    }
    
}
