using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("key_impr_indicator")]
    public class KeyImprovementIndicatorsModel
    {

        [Key]
        public int Key_Impr_Indicator_id { get; set; }
        public string Key_Impr_Indicator_Name { get; set; }


        public int Score_id {  get; set; }

        public int competency_id {  get; set; }
        public string Key_Impr_Indicator_DESC { get; set; }
        public string Key_Impr_Indicator_Status { get; set; }

        public String Key_Impr_Indicator_CreatedDate { get; set; }

        

    }
}
