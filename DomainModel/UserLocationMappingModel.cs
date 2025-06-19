using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace DomainModel
{
    [Table("user_location_mapping")]
    public class userlocationmappingModel
    {
        [Key]
        public int user_location_mapping_id { get; set; }
        public int Entity_Master_id { get; set; }
        public int Unit_location_Master_id { get; set; }
        public int USR_ID { get; set; }
        public string ROLE_ID { get; set; }
        public DateTime user_location_mapping_start_Date { get; set; }
        public DateTime user_location_mapping_End_Date { get; set; }
        public string user_location_mapping_status { get; set; }
        public string user_location_mapping_createddate { get; set; }


        public int taskID {  get; set; }



    }
}
