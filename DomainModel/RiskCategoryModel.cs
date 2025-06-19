using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("risk_categorymaster")]
    public class RiskCategoryModel
    {
        [Key]
        public int RiskCategoryMasterID { get; set; }
        public string RiskCategoryName { get; set; }
        public string RiskCategoryStatus { get; set; }
        public string Description { get; set; }

    }
}
