using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table ("riskassessmenttemplate")]
    public class RiskAssessmentTemplateModel
    {
       
            [Key]
            public int assessmenttemplateid { get; set; }
            public string configuration { get; set; }

            public string controls { get; set; }

            public bool togglecontrols {  get; set; }


            public string remarks { get; set; }
            public int createdby { get; set; }
            public string createddate { get; set; }

            public string status { get; set; }
        }
    }

