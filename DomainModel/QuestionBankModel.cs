using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.IO;
using Microsoft.AspNetCore.Http;



namespace DomainModel
{
    [Table("questionbank")]
    public class QuestionBankModel
    {
        [Key]
        public int question_id { get; set; }
        public string question { get; set; }
        public int response_type { get; set; }
        public int no_of_selectionchoices { get; set; }
        public int correct_answer { get; set; }
        public string question_hint { get; set; }
        public string questionmarked_favourite { get; set; }
        public int score_weightage { get; set; }
        public int check_level { get; set; }
        public float checklevel_weightage { get; set; }
        public int estimated_time { get; set; }
        public string keywords { get; set; }
        public string assessor_randomselection { get; set; }
        public string assessment_randomsetting { get; set; }
        public int subjectid { get; set; }
        public int topicid { get; set; }
        public string ref_to_governance_control { get; set; }
        public string question_disabled { get; set; }
        public string objective { get; set; }
        public string base64 { get; set; }

        public string fullresponse { get; set; }
         public int userid { get; set; }

        public List<Options> options { get; set; }

        public DateTime created_date { get; set; }

        public string status { get; set; }


    }

    public class Options
    {
        [Key]
        public int index { get; set; }
        public string value { get; set; }
    }

    public class QuestionModel
    {
        [Key]
        public int? question_id { get; set; }
        public string? question { get; set; }
        public int? response_type { get; set; }
        public int? no_of_selectionchoices { get; set; }
        public int? correct_answer { get; set; }
        public string? question_hint { get; set; }
        public string? questionmarked_favourite { get; set; }
        public int? score_weightage { get; set; }
        public int? check_level { get; set; }
        public float? checklevel_weightage { get; set; }
        public int? estimated_time { get; set; }
        public string? keywords { get; set; }
        public string? assessor_randomselection { get; set; }
        public string? assessment_randomsetting { get; set; }
        public int? subjectid { get; set; }
        public int? topicid { get; set; }
        public string? ref_to_governance_control { get; set; }
        public string? question_disabled { get; set; }
        public string? objective { get; set; }
        public string? base64 { get; set; }

        public string? fullresponse { get; set; }
        public int? userid { get; set; }

        public List<Options> options { get; set; }

        public DateTime? created_date { get; set; }

        public string? status { get; set; }
        
             public string? Topic_Name { get; set; }
        public string? Subject_Name { get; set; }
        public string? Skill_Level_Name { get; set; }
        public int? no_of_times_used { get; set; }
    }


    public class BulkUploadQues
    {
        public int userid { get; set; }
        public int subjectid { get; set; }
        public int topicid { get; set; }
        public float checklevel_weightage { get; set; }
        public int check_level { get; set; }
        public IFormFile file { get; set; }
    }


    public class QuestionModelNew
    {
        [Key]
        public int? question_id { get; set; }
        public string? question { get; set; }
        public int? response_type { get; set; }
        public int? no_of_selectionchoices { get; set; }
        public int? correct_answer { get; set; }
        public string? question_hint { get; set; }
        public string? questionmarked_favourite { get; set; }
        public int? score_weightage { get; set; }
        public int? check_level { get; set; }
        public float? checklevel_weightage { get; set; }
        public int? estimated_time { get; set; }
        public string? keywords { get; set; }
        public string? assessor_randomselection { get; set; }
        public string? assessment_randomsetting { get; set; }
        public int? subjectid { get; set; }
        public int? topicid { get; set; }
        public string? ref_to_governance_control { get; set; }
        public string? question_disabled { get; set; }
        public string? objective { get; set; }
        public string? base64 { get; set; }

        public string? fullresponse { get; set; }
        public int? userid { get; set; }

        public List<Options> options { get; set; }

        public DateTime? created_date { get; set; }

        public string? status { get; set; }

        public string? Topic_Name { get; set; }
        public string? Subject_Name { get; set; }
        public string? checklevel_name { get; set; }
        public int? no_of_times_used { get; set; }
        public string authorName { get; set; }
        public string location { get; set; }
        public string departmentName { get; set; }
        public string entity { get; set; }
        public string tpaentity { get; set; }
    }


}
