using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DomainModel
{
    [Table("sectormaster")]
    public class SectorModel
    {
        [Key]
        public int SectorID { get; set; }

        public string SectorName { get; set; }
        public string Status { get; set; }
        
    }
}
