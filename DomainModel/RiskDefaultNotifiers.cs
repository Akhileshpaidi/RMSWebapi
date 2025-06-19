using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DomainModel
{
    [Table("risk_default_notifiers")]
    public class RiskDefaultNotifiers
    {
        [Key]
        public int RiskDefaultNotifiersID { get; set; }
        public int Entity_Master_id { get; set; }
        public int Unit_location_Master_id { get; set; }
        public int Department_Master_id { get; set; }
        public string emailid { get; set; }
        public string? additional_emailid_notifiers { get; set; }
        public string Status { get; set; }
    }
}
