using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{

    [Table("add_doc")]
    public class AddDocumentModel
    {

        [Key]
        public int AddDoc_id { get; set; }
        public string Title_Doc { get; set; }
        public string Sub_title_doc { get; set; }
        public string Obj_Doc { get; set; }

        public string Select_Opt { get; set; }
        public string Doc_Confidentiality { get; set; }
        public string OtpMethod { get; set; }
        public DateTime? Eff_Date { get; set; }
        public DateTime? Initial_creation_doc_date { get; set; }
        public string Doc_internal_num { get; set; }

        public string Doc_Inter_ver_num { get; set; }
        public string Doc_Phy_Valut_Loc { get; set; }

        public string Doc_process_Owner { get; set; }
        public string Doc_Approver { get; set; }
        public DateTime? Date_Doc_Approver { get; set; }
        public DateTime? Date_Doc_Revision { get; set; }
        public string Pub_Auth_Type { get; set; }

        public string Keywords_tags { get; set; }

        public int? freq_period { get; set; }
        public string pub_doc { get; set; }

        public string publisher_comments { get; set; }
        public string indicative_reading_time { get; set; }
        public string Time_period { get; set; }
        public int DocTypeID { get; set; }

        public int Doc_CategoryID { get; set; }

        public int Doc_SubCategoryID { get; set; }
        public int NatureOf_Doc_id { get; set; }
        public int AuthoritynameID { get; set; }
        public string? Draft_Status { get; set; }
        public string addDoc_Status { get; set; }
        public string addDoc_createdDate { get; set; }
        public string Document_Id { get; set; }

        public string Document_Name { get; set; }
        public string Publisher_name { get; set; }

         public string Publisher_Date_Range { get; set; }


        public string Nature_Confidentiality { get; set; }

        public string Review_Status { get; set; }

        public int AuthorityTypeID { get; set; }


        public string FilePath { get; set; }

        public string freq_period_type { get; set; }
        public DateTime? review_start_Date { get; set; }
        //public int AuthorityTypeID { get; set; }
        public string AuthorityTypeName { get; set; }

        public string DisableReason { get; set; }
        public string VersionControlNo { get; set; }
        public int? USR_ID { get; set; }
        public string Doc_referenceNo { get; set; }
        public string Revision_summary { get; set; }


        public String status_permission { get; set; }

        public int? Review_Frequency_Status { get; set; }
        public int? Doc_Linking_Status { get; set; }

        public string Entity_Master_id { get; set; }
        public string Unit_location_Master_id { get; set; }

        public string Linking_Doc_names { get; set; }


        public int? MainpageCount { get; set; }

        public int? supportFilesCount { get; set; }
        public DateTime? ChangedOn { get; set; }
        public int? ChangedBy { get; set; }


    }


    public class UpdateDoc
    {

        [Key]

        public int AddDoc_id { get; set; }
        public string Title_Doc { get; set; }
        public string Sub_title_doc { get; set; }
        public string Obj_Doc { get; set; }

        public string Select_Opt { get; set; }
        public string Doc_Confidentiality { get; set; }
        public string OtpMethod { get; set; }
        public DateTime? Eff_Date { get; set; }
        public DateTime? Initial_creation_doc_date { get; set; }
        public string Doc_internal_num { get; set; }

        public string Doc_Inter_ver_num { get; set; }
        public string Doc_Phy_Valut_Loc { get; set; }

        public string Doc_process_Owner { get; set; }
        public string Doc_Approver { get; set; }
        public DateTime? Date_Doc_Approver { get; set; }
        public DateTime? Date_Doc_Revision { get; set; }
        public string Pub_Auth_Type { get; set; }

        public string Keywords_tags { get; set; }

        public int? freq_period { get; set; }
        public string pub_doc { get; set; }

        public string publisher_comments { get; set; }
        public int indicative_reading_time { get; set; }
        public string Time_period { get; set; }
        public int DocTypeID { get; set; }

        public int Doc_CategoryID { get; set; }

        public int Doc_SubCategoryID { get; set; }
        public int NatureOf_Doc_id { get; set; }
        public int AuthoritynameID { get; set; }
        public string Draft_Status { get; set; }
        public string addDoc_Status { get; set; }
        public DateTime? addDoc_createdDate { get; set; }
        public string Document_Id { get; set; }

        public string Document_Name { get; set; }
        public string Publisher_name { get; set; }

        public DateTime? Publisher_Date_Range { get; set; }

        public DateTime? Document_Eff_Date { get; set; }
        public string Nature_Confidentiality { get; set; }

        public string Review_Status { get; set; }

        public string FilePath { get; set; }

        public string freq_period_type { get; set; }
        public DateTime? review_start_Date { get; set; }
        public int AuthorityTypeID { get; set; }
        public string AuthorityTypeName { get; set; }

        public string DisableReason { get; set; }
        public string VersionControlNo { get; set; }
        public int USR_ID { get; set; }
        public int? Doc_referenceNo { get; set; }
        public string Revision_summary { get; set; }

        public string status_permission { get; set; }
        public int? Review_Frequency_Status { get; set; }
        public int? Doc_Linking_Status { get; set; }
        public string Linking_Doc_names { get; set; }
        public int? MainpageCount { get; set; }

        public int? supportFilesCount { get; set; }

        public DateTime ChangedOn { get; set; }
        public int ChangedBy { get; set; }
    }


}