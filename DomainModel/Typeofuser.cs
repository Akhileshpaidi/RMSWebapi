using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("typeofuser")]

    public class typeofusermodel
    {
        [Key]

        public int typeofuserid { get; set; }

        public string typeofusername { get; set; }

      
    }
}
