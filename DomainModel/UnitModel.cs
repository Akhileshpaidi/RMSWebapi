using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("unitmaster")]
    public class UnitModel

    {
        [Key]
        public int UnitMasterID { get; set; }

        public string UnitName { get; set; }
        public string UnitStatus { get; set; }
        public string Discription { get; set; }
        public int UnitTypeID { get; set; }
       

    }
}
