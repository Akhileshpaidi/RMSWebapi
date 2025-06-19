using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("reviewstatussettings")]
    public class ReviewStatusSettingsModel
    {
        [Key]
        public int ReviewStatusID { get; set; }
        public string? ReviewStatusName { get; set; }
        public int? MinimumDays { get; set; }
        public int? MaximumDays { get; set; }
        public string? createdate { get; set; }
        public string? Status { get; set; }
        public int? ModifiedBy { get; set; }

    }
}
