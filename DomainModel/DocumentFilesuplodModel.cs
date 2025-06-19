using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("documentrepository")]
    public class DocumentFilesuplodModel
    {

        [Key]
        public int DocumentRepID { get; set; }
        //public string Document_Id { get; set; }
        public int AddDoc_id { get; set; }
        public string Document_Id { get; set; }
        public string VersionControlNo { get; set; }
        public string FileCategory { get; set; }
        public string Document_Name { get; set; }
        public string FilePath { get; set; }
        public string Status { get; set; }
        public string documentrepository_createdDate { get; set; }

    }
    public class LinkedDocInfo
    {
        public string DocumentName { get; set; }
        public string FilePath { get; set; }
    }

}
