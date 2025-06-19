using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("document_confidentiality")]
    public class DocumentConfidentiality
    {
        [Key]
        public int DocumentConfidentialityID { get; set; }
        public string ConfidentialityName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string WaterMark { get; set; }

    }
}
