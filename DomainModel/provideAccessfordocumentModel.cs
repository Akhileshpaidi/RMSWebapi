using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using System.ComponentModel.DataAnnotations;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("doc_user_access_mapping")]
    public class ProvideAccessdocument
    {
        [Key]
        public int Doc_User_Access_mapping_id { get; set; }
        public int Unit_location_Master_id { get; set; }
        //public int USR_ID { get; set; }
        public int Entity_Master_id { get; set; }
        public string Doc_User_Access_mapping_createdDate { get; set; }

        public string Doc_User_Access_mapping_Status { get; set; }

     ///  public string ack_status { get; set; }

        public string Document_Id {  get; set; }
      //  public string  duedate { get; set; }
      //  public string timeline {  get; set; }
          
        /// <summary>
        ///public string trakstatus {  get; set; }
        /// </summary>

        public int AddDoc_id { get; set; }
        public int createdBy { get; set; }
        //public int everyday { get; set; }   

        //public string timeperiod { get; set; }


        //public string reqtimeperiod {  get; set; }

        //public int  noofdays {  get; set; }
        //public string optionalreminder { get; set; }



        //public string validitydocument { get; set; }

        //public string startDate { get; set; } 

        //public string endDate {  get; set; }







    }
    public class UpdateData
    {
        public doctaskuseracknowledmentstatusmodel doctaskuseracknowledmentstatusmodels { get; set; }
        public int UserId { get; set; }

        public int userlocationmappingid {  get; set; }
        public int[] DocPermId { get; set; }


        public string ack_status { get; set; }
        public int permissionupdatedby { get; set; }
    }

}
