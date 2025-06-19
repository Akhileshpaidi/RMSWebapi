using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DomainModel
{ 
     [Table("folderupload")]
public class FolderUpload
{
    [Key]
    public int folderID { get; set; }
    public string folderName { get; set; }
    public string folderNameUnique { get; set; }
    public string dateTime { get; set; }
    public int flightID { get; set; }
    public int tsID { get; set; }
    public string year { get; set; }
    public int missionId { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }

        public int loginID { get; set; }//vishnu
    }
    public class ViewFolderUpload
    {
        [Key]
        public int folderID { get; set; }
        public string folderName { get; set; }
        public string folderNameUnique { get; set; }
        public string fileName { get; set; }
        public string dateTime { get; set; }
        public int flightID { get; set; }
        public string year { get; set; }
        public string flightName  { get; set; }
       
       
        public int tsID { get; set; }
        public string stationName { get; set; }
        public int missionId { get; set; }
        public string missionName { get; set; }
        public string fileExtension { get; set; }

        public int loginID { get; set; }//vishnu
    }
}
