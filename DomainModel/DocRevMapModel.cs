using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("document_revision_mapping")]
    public class DocRevMapModel
    {
        [Key]
        public int Doc_rev_map_id { get; set; }
        public string Doc_referenceNo { get; set; }
        public string Revision_summary { get; set; }
        public int no_of_selectionchoices { get; set; }
        public DateTime Doc_rev_map_createdDate { get; set; }
        public string Doc_rev_map_status { get; set; }
        public int AddDoc_id { get; set; }
        public string Document_Id { get; set; }

        public string VersionControlNo { get; set; }

    }
}
