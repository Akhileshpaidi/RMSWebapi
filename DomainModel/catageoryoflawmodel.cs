using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("categoey_of_law")]
    public class catageoryoflawmodel
    {
        [Key]
       public int category_of_law_ID {  get; set; } 
      public string law_Categoryname {  get; set; }
      public string category_of_Law_Description { get; set; }
      public string category_of_Law_Create_Date {  get; set; }
      public int createdby {  get; set; }
       public string status {  get; set; }
        public string source { get; set; }

    }
}
