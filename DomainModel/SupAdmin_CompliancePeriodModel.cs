using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("compliance_period")]
    public class SupAdmin_CompliancePeriodModel
    {
        [Key]
        public int compliance_period_id { get; set; }
        public string compliance_period_start { get; set; }
        public string compliance_period_end { get; set; }
        public string start_compliance_year_format { get; set; }
        public string end_compliance_year_format { get; set; }
        public string compliance_period_description { get; set; }
        public string compliance_period_status { get; set; }
        public string created_date { get; set; }
        public string updated_date { get; set; }
        public int createdby { get; set; }
      

        public Boolean check_box { get; set; }
    }
}
