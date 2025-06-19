using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DomainModel
{
   
     [Table("score_indicator")]
    public class ScoreIndicatorModel
    {
        [Key]

            public int Score_id { get; set; }
            public string Score_Name { get; set; }
            public string Score_Range { get; set; }

        public int scoremaxrange { get; set; }
        public int scoreminrange { get; set; }
            public string Score_Desc { get; set; }
            public string score_status { get; set; }
            public string score_createdDate { get; set; }

    }
 }

