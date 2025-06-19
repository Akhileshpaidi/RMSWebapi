using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace DomainModel
{
    [Table("scheduled_mapped_user")]
   
         public class sheduledAssessmentmappeduser
    {
        [Key]
       public int Scheduled_mapped_user_id {  get; set; }
        public int? map_user_id {  get; set; } 
        public int? Schedule_Assessment_id { get; set; }
        public  string created_date { get; set; }
        public int? docTypeID { get; set; }
        public int ?doc_CategoryID { get; set; }
        public int ?doc_SubCategoryID { get; set; }
        public int tpaenityid { get; set; }
        public int tpauserid { get; set; }
        public int ?entity_Master_id { get; set; }
        public int ?unit_location_Master_id { get; set; }
        public string ?department_Master_id { get; set; }
        public string? usr_ID { get; set; }
        public string? excludedusrid { get; set; }
        public string? exculededdescription {  get; set; }

        public string defaultkey {  get; set; }

        public string requstingperson { get; set; }

        public string tpaenitydescription {  get; set; }

        public string scheduledstatus {  get; set; }

        public int riskAssesserid {  get; set; }
        public string  status { get; set; }
    }
}
