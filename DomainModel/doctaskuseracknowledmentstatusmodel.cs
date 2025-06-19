using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("doc_taskuseracknowledment_status")]
    public class doctaskuseracknowledmentstatusmodel
    {

        [Key]
        public int doc_taskuseracknowledment_id { get; set; }

       

        public int USR_ID { get; set; }

        public int AddDoc_id { get; set; }

        public string Document_Id { get; set; }

        public string ack_status { get; set;}

        public string duedate { get; set; }

        public DateTime? acknowledged_date { get; set; }
        public DateTime? readComplete_date { get; set; }
        public DateTime? readLater_date { get; set; }

        public Boolean Favorite { get; set; }

        public string status { get; set; }

        public string createddate { get; set; }

        public int user_location_mapping_id { get; set; }


        public int Doc_User_Access_mapping_id {  get; set; }

        public string validitydocument { get; set; }
        public string startDate {  get; set; }

        public string endDate { get; set; }

        public string optionalreminder { get; set; }
        public string trakstatus { get; set; }

        public int everyday { get; set; }

        public string timeperiod { get; set; }


        public string reqtimeperiod { get; set; }

        public int noofdays { get; set; }

        public string provideack_status {  get; set; }







    }
}
