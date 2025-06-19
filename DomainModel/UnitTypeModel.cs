using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("unittypemaster")]
    public class UnitTypeModel

    {
        [Key]
        public int UnitTypeID { get; set; }

        public string UnitTypeName { get; set; }
        public string UnitTypeStatus { get; set; }

        public string unittypeDesc { get; set; }

        public int createdBy { get; set; }

        public string createddate { get; set; }

        public string unittypetablename {  get; set; }
        public string source { get; set; }

    }
}
