using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("tpauser")]
    public class tpamodel
    {
        [Key]
      public int  tpauserid {  get; set; }

        public string tpausername { get; set; }

        public string password {  get; set; }

        public string USR_LOGIN {  get; set; }
 
        public string tpamobilenumber {  get; set; }

        public string tpaemailid {  get; set; }

        public string state {  get; set; }
        public int typeofuserid {  get; set; }

        public int tpaenityid {  get; set; }

        public string address { get; set; }
        public string tpadescription { get; set; }

        public string designation { get; set; }

        public string status { get; set; }

        public int defaultrole {  get; set; }
        public string createddate { get; set; }

        public string taskids {  get; set; }
    }

  
}
