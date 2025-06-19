using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("update_compliance_attachments")]
    public class UpdateComplianceFilesModel
    {
        public int Id { get; set; }
        public string file_name { get; set; }
        public string file_type { get; set; }
        public string file_path { get; set; }
        public string nature_of_attachment { get; set; }
        public int update_compliance_id { get; set; }
        public string status {  get; set; }
    }
}
