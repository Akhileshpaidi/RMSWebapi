using Microsoft.AspNetCore.Http;
using System;
using Org.BouncyCastle.Asn1.Mozilla;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{

    [Table("mitigations")]
    public class MitigationModel
    {
        [Key]
        public int? mitigations_id { get; set; }
        public string? assessment_id { get; set; }
        public string? status { get; set; }
        public DateTime? created_date { get; set; }
        public int? action_required { get; set; }
        public string? ass_template_id { get; set; }
        public string? uq_ass_schid { get; set; }



    }

    //Creating New Mitigation Model by Assessor

    public class MitigationCreatingModel
    {
        [Key]

        public string[] suggestions { get; set; }

        public int suggested_by { get; set; }

        public int action_required { get; set; }
        public string assessment_id { get; set; }
        public string ass_template_id { get; set; }
        public string uq_ass_schid { get; set; }
        public object[] scoreindicators { get; set; }


        public string Remarks { get; set; }
        public string scoreIndicator { get; set; }
        public string key_Impr_Indicator_Name { get; set; }
        public string overallremarks { get; set; }
     public int proposed_by { get; set; }

    }


    [Table("suggestions_tbl")]
    public class suggestionsModel
    {
        [Key]
        public int? suggestions_id { get; set; }
        public int? mitigations_id { get; set; }
        public string? suggestions { get; set; }
        public string? status { get; set; }
        public DateTime? created_date { get; set; }     
        public int? suggested_by { get; set; }
        public int? acknowledge_by { get; set; }
        public string? remarks { get; set; }
        public int? action_required { get; set; }        
        public int? notify_management { get; set; }
        public DateTime? input_date { get; set; }
        public int? assign_responsibility { get; set; }
        public DateTime? tentative_timeline { get; set; }
        public string? suggested_documents { get; set; }
        public int? action_priority { get; set; }        
        public int? acknowledge { get; set; }
        public string? TrackerID { get; set; }
        public string? PO_remarks { get; set; }
        public string? file_path { get; set; }
     
        public string? management_remarks { get; set; }

    }


    public class suggestionsModelforView
    {
        [Key]
        public int? suggestions_id { get; set; }
        public int? mitigations_id { get; set; }
        public string? suggestions { get; set; }
        public string? status { get; set; }
        public DateTime? created_date { get; set; }
        public string company_name { get; set; }
        public string locationName { get; set; }
        public string assessment_name { get; set; }
        public string AssessType { get; set; }
        public string Subtypename { get; set; }
        public int? suggested_by { get; set; }
        public int? acknowledge_by { get; set; }
        public string? remarks { get; set; }
        public int? action_required { get; set; }

        public int? notify_management { get; set; }
        public DateTime? input_date { get; set; }

        public int? assign_responsibility { get; set; }
        public DateTime? tentative_timeline { get; set; }

        public string? suggested_documents { get; set; }
        public int? action_priority { get; set; }
        public int? acknowledge { get; set; }
        public string? Suggester_Name { get; set; }
        public string? Acknowledger_Name { get; set; }
        public string? AssignerName { get; set; }
        public string TrackerID { get; set; }
        public string? PO_remarks { get; set; }
        public string? file_path { get; set; }
        public string? file_name { get; set; }
        public string? management_remarks { get; set; }
        public DateTime? completed_date { get; set; }

        public string uq_ass_schid { get; set; }


        public string Type_Name { get; set; }

        public string SubType_Name { get; set; }
        public string Competency_Name { get; set; }

        public string aassessmentname { get; set; }
    }


    [Table("acknowledge_suggestions")]
    public class AcknowledgeSuggestionssModel
    {
        [Key]
        public int acknowledge_suggestions_id { get; set; }
        public int? suggestions_id { get; set; }

        public string? suggestions { get; set; }
        public string? status { get; set; }
        public DateTime? created_date { get; set; }

        public int? acknowledge_by { get; set; }
        public string? comment { get; set; }
      
                public int? mitigations_id { get; set; }

    }

    public class AssSchedulemitigationModel
    {
        public int assessment_builder_id { get; set; }
        public string ass_template_id { get; set; }
        public int Schedule_Assessment_id { get; set; }
        public int Competency_id { get; set; }
        public string Type_Name { get; set; }
        public string assessment_name { get; set; }
        public string assessment_description { get; set; }
        public string SubType_Name { get; set; }
        public string Competency_Name { get; set; }
        public string AssessementDueDate { get; set; }
        public string AssessementcompletedDate { get; set; }
        public DateTime? created_date { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public string keywords { get; set; }
        public string status { get; set; }
        public string uq_ass_schid { get; set; }
        public int mitigations_id { get; set; }
        public string TrackerID { get; set; }
        public string verson_no { get; set; }

        public string assessorname {  get; set; }

    }






    public class suggestionmodel
    {
        [Key]
        public int? suggestions_id { get; set; }
     
    
        public string? PO_remarks { get; set; }
        public IFormFile file { get; set; }
        public string? file_path { get; set; }
        public string? file_name { get; set; }




    }










}
