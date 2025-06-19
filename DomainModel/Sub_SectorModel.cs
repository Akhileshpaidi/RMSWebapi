using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("sub_sectormaster")]
    public class Sub_SectorModel
    {
        [Key]
        public int SubSectorMasterID { get; set; }

        public string SubSectorName { get; set; }
        public int SectorID { get; set; }
        public string Description { get; set; }
        public string Sub_SectorStatus { get; set; }
        
    }
}
