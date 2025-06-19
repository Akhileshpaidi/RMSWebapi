using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    
    public class AssessmentTemplateModel
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
        public string check_level { get; set; }
        public float checklevel_weightage { get; set; }
        public int estimated_time { get; set; }
    
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

        public int Assessment_generationID {  get; set; }
        public List<AssessmentBuilder> options { get; set; }

        public DateTime? created_date { get; set; }

        public string? status { get; set; }

        //public int Assessment_generation_details_ID { get; set; }
        public int? assessment_builder_id { get; set; }
        public string? assessment_name { get; set; }
        public string? assessment_description { get; set; }
        public int Type_id { get; set; }
        public int SubType_id { get; set; }
        public string? show_hint { get; set; }
        public int Competency_id { get; set; }
        public string? SubType_Name { get; set; }
        public string Type_Name { get; set; }
        public int no_of_questions_mapped { get; set; }
        public string? ass_template_id { get; set; }

        public string Subject_Name { get; set; }

        public string Topic_Name {  get; set; }
        public string? Competency_Name { get; set; }
        public string? keywords { get; set; }

        public int? show_explaination { get; set; }
        public string? firstname { get; set; }
        public int updated_user_id { get; set; }
        public DateTime? updated_date { get; set; }
        public string? UpdateUsername { get; set; }

        public int? total_questions {  get; set; }

        public int? total_estimated_time { get; set; }

        public int? verson_no {  get; set; }























    }




    public class AssessmentTemplateModelNew
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
        public string check_level { get; set; }
        public float checklevel_weightage { get; set; }
        public int estimated_time { get; set; }

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

        public int Assessment_generationID { get; set; }
        public List<AssessmentBuilder> options { get; set; }

        public DateTime? created_date { get; set; }

        public string? status { get; set; }

        //public int Assessment_generation_details_ID { get; set; }
        public int? assessment_builder_id { get; set; }
        public string? assessment_name { get; set; }
        public string? assessment_description { get; set; }
        public int Type_id { get; set; }
        public int SubType_id { get; set; }
        public string? show_hint { get; set; }
        public int Competency_id { get; set; }
        public string? SubType_Name { get; set; }
        public string Type_Name { get; set; }
        public int no_of_questions_mapped { get; set; }
        public string? ass_template_id { get; set; }

        public string Subject_Name { get; set; }

        public string Topic_Name { get; set; }
        public string? Competency_Name { get; set; }
        public string? keywords { get; set; }

        public int? show_explaination { get; set; }
        public string? firstname { get; set; }
        public int updated_user_id { get; set; }
        public DateTime? updated_date { get; set; }
        public string? UpdateUsername { get; set; }

        public int? total_questions { get; set; }

        public int? total_estimated_time { get; set; }

        public string reason_for_disable { get; set; }

        public DateTime? disabled_date {  get; set; }























    }
    public class AssessmentBuilder
    {
        //[Key]
        //public string assessment_name { get; set; }
        //public string assessment_description { get; set; }
        //public int Type_id { get; set; }
        //public int SubType_id { get; set; }
        //public string show_hint { get; set; }
        //public int Competency_id { get; set; }
        //public string SubType_Name { get; set; }
        //public string Type_Name { get; set; }
        //public int ass_template_id {  get; set; }
     

    }
}
