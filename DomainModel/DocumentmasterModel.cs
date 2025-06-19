using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table ("documentmaster")]
    public class DocumentmasterModel
    {
        [Key]
        public int document_id { get; set; }
        public string document_name { get; set; }
        public string document_description { get; set; }
        public string CreatedBy { get; set; }
        public string GroupName { get; set; }
        public string DocumentType { get; set; }
        public DateTime created_date { get; set; }

    }
    public class DepositorycountModel
    {
        [Key]
        public int SummaryCount { get; set; }
    }
    public class DisaledcountModel
    {
        [Key]
        public int SummaryCount { get; set; }
    }
    public class DraftStatuscountModel
    {
        [Key] 
        public int Status_Count { get; set; }
        public string Draft_Status { get; set; }
    }

    public class AllPublishedDoc
    {
        [Key]
        public int AddDoc_id { get; set; }
        public string Title_Doc { get; set; }
        public string Doc_Confidentiality { get; set; }
        public string Eff_Date { get; set; }
        public string Initial_creation_doc_date { get; set; }
        public string Doc_process_Owner { get; set; }
        public string Doc_Approver { get; set; }
        public string DocTypeName { get; set; }
        public string Doc_CategoryName { get; set; }
        public string Doc_SubCategoryName { get; set; }
        public string Document_Version { get; set; }
        public string addDoc_createdDate { get; set; }
        public string doc_Classification { get; set; }
        public string publish_Authority { get; set; }
        public string name_of_Authority { get; set; }

    }

    public class DocumentsSummaryModel
    {
        [Key]
        public int Status_Count { get; set; }
        public string ReviewStatusName { get; set; }
    }
    public class DocTypeDataModel
    {
        [Key]
        public int Doctype_Count { get; set; }
        public int DocTypeID { get; set; }
        public string DoctypeName { get; set; }
        public string Draft_Status { get; set; }
        public string Doc_CategoryName { get; set; }
        public string Doc_SubCategoryName { get; set; }
        public int Doccategory_Count { get; set; }
        public int Docsubcategory_Count { get; set; }

    }

    public class DocCategoryDataModel
    {
        [Key]
        public int Doccategory_Count { get; set; }
        public int Doc_CategoryID { get; set; }
        public string Doc_CategoryName { get; set; }
        public string Draft_Status { get; set; }
        public string DoctypeName { get; set; }
        public int Doctype_Count { get; set; }
    }

    public class DocSubcategoryDataModel
    {
        [Key]
        public int Docsubcategory_Count { get; set; }
        public int Doc_SubCategoryID { get; set; }
        public string Doc_SubCategoryName { get; set; }
        public string Draft_Status { get; set; }
    }
    public class DocConfidentialityModel
    {
        [Key]
        public int count { get; set; }
        public string Doc_Confidentiality { get; set; }
    }
    public class DocAuthorityDataModel
    {
        [Key]
        public int count { get; set; }
        public string AuthorityTypeName { get; set; }
        public string AuthorityName { get; set; }
    }
    public class RecentDocDataModel
    {
        [Key]
        public int AddDoc_id { get; set; }
        public string Title_Doc { get; set; }
        public string Initial_creation_doc_date { get; set; }
        public string addDoc_createdDate { get; set; }
    }
    public class NatureOfDocModel
    {
        [Key]
        public int Count { get; set; }
        public string NatureOf_Doc_Name { get; set; }
        
    }
    public class DocReadingTimeModel
    {
        [Key]
        public int count { get; set; }
        public string Time_period { get; set; }
        public string indicative_reading_time { get; set; }

    }
    public class AckReqSummaryModel
    {
        [Key]
        public string total_requested { get; set; }
        public string total_acknowledged { get; set; }
        public string Acknowledment { get; set; }
        public int count { get; set; }

    }
}
