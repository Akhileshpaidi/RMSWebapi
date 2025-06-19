using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace DomainModel
{
    
    public class RiskSup_ImpactRating
    {
        [Key]
        public int ImpactRatingID { get; set; }
        public string RiskImpactRatingName { get; set; }
        public string RiskImpactRatingDescription { get; set; }
        public string RiskImpactRatingScale { get; set; }

    }
}
