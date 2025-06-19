using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    [Table("acknowledgement_tbl")]
    public class AcknowledgementModel
    {
        [Key]
        public int Ack_Id { get; set; }
        public string User_Id { get; set; }
        public string Document_Id { get; set; }
        public string AddDoc_id { get; set; }
        public string DocumentRepId { get; set; }
        public string VersionControlNo { get; set; }
        public string File_Category { get; set; }
        public Boolean Favorite { get; set; }
        public string Created_Date { get; set; }
        public string Status { get; set; }

    }
    public class MailRequestModel
    {
        public string emailToAddress { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
    }
}
