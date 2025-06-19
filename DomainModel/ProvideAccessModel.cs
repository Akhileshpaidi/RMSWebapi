using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{


    public class ProvideAccessModel
    {
        [Key]

        public int AddDoc_id { get; set; }
        public string Document_Id { get; set; }
        public string document_name { get; set; }
        public string Title_Doc { get; set; }
        public string Sub_title_doc { get; set; }
        public string Obj_Doc { get; set; }
        public int? NatureOf_Doc_id { get; set; }
        public string OtpMethod { get; set; }
        public string Publisher_name { get; set; }
        public string NatureOf_Doc_Name { get; set; }

        public int? DocTypeID { get; set; }

        public string DocTypeName { get; set; }

        public int? Doc_CategoryID { get; set; }
        public string Doc_CategoryName { get; set; }

        public int? Doc_SubCategoryID { get; set; }

        public string Doc_SubCategoryName { get; set; }

        public int? AuthorityTypeID { get; set; }
        public string AuthorityTypeName { get; set; }
        public string addDoc_createdDate { get; set; }
        public string FilePath { get; set; }

        public int? AuthoritynameID { get; set; }
        public string AuthorityName { get; set; }
        public string Provide_Access_status { get; set; }
        public string Doc_Confidentiality { get; set; }
        public string Initial_creation_doc_date { get; set; }
        public string Eff_Date { get; set; }
        public string Doc_internal_num { get; set; }

        public string Doc_Inter_ver_num { get; set; }
        public string Doc_Phy_Valut_Loc { get; set; }
        public string Doc_process_Owner { get; set; }
        public string Doc_Approver { get; set; }
        public string Date_Doc_Revision { get; set; }
        public string Date_Doc_Approver { get; set; }

        public string freq_period { get; set; }
        public string review_start_Date { get; set; }
        public string freq_period_type { get; set; }
        public string Keywords_tags { get; set; }
        public string pub_doc { get; set; }
        public string publisher_comments { get; set; }
        public string indicative_reading_time { get; set; }
        public string Time_period { get; set; }
        public string firstname { get; set; }
        public string CREATED_DATE { get; set; }
        public string Publisher_Date_Range { get; set; }
        public string Select_Opt { get; set; }
        public string Unit_location_Master_id { get; set; }
        public string Entity_Master_id { get; set; }

        public List<UpdateDoc_referenceNo> Doc_referenceNo { get; set; }
        public List<UpdateRevision_summary> Revision_summary { get; set; }

        public string VersionControlNo { get; set; }
        public int? Review_Frequency_Status { get; set; }
        public int? Doc_Linking_Status { get; set; }

        public string Review_Status { get; set; }
        public int? NoofDays { get; set; }
        public string validations { get; set; }
        public string todaysdate { get; set; }
        public string datesub { get; set; }
        public string startdate { get; set; }
        public string enddate { get; set; }
        public int? USR_ID { get; set; }
        public string status_permission { get; set; }
        public Boolean favorite { get; set; }
        public string Linking_Doc_names { get; set; }
        public string publish_Name { get; set; }
        //public string permission_rights { get; set; }
        public int? MainpageCount { get; set; }

        public int? supportFilesCount { get; set; }
        public DateTime? ChangedOn { get; set; }
        public int? ChangedBy { get; set; }

    }



    public class UpdateVersion
    {

        [Key]

        public int? AddDoc_id { get; set; }

        public string Document_Id { get; set; }

        public string VersionControlNo { get; set; }
        public List<UpdateDoc_referenceNo> Doc_referenceNo { get; set; }
        public List<UpdateRevision_summary> Revision_summary { get; set; }

    }

    public class UpdateDoc_referenceNo
    {
        [Key]
        public int? id { get; set; }
        public string value { get; set; }

    }
    public class UpdateRevision_summary
    {
        [Key]
        public int? id { get; set; }
        public string value { get; set; }

    }

}
