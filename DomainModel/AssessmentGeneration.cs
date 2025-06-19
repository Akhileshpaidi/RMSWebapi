using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Spreadsheet;


namespace DomainModel
{
    public class AssessmentGeneration
    {
        [Key]
        public int Assessment_generationID { get; set; }
        public string QuestionMixtype { get; set; }
        public int No_of_Questions { get; set; }
        public string MostUsedQuestions { get; set; }
        public string FavouritesDefaults { get; set; }
        public string RecentlyAdded { get; set; }
        public string TimeEstimate { get; set; }
        public int TimeEstimateInputMin { get; set; }
        public int TimeEstimateInputMax { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }

        public string[] Topics { get; set; }

        public List<Competancychecks> Competancychecks { get; set; }

       // public string no_of_times_used { get; set; }

    }

    public class AssessmentGenerationNew
    {
        [Key]
        public int Assessment_generationID { get; set; }
        public string QuestionMixtype { get; set; }
        public int No_of_Questions { get; set; }
        public string MostUsedQuestions { get; set; }
        public string FavouritesDefaults { get; set; }
        public string RecentlyAdded { get; set; }
        public string TimeEstimate { get; set; }
        public int TimeEstimateInputMin { get; set; }
        public int TimeEstimateInputMax { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }

        public string[] Topics { get; set; }

        public List<Competancychecks> Competancychecks { get; set; }

         public string QuestionsListFrom { get; set; }

    }



    public class Competancychecks
    {
        [Key]
        public int id { get; set; }
        public string value { get; set; }
    }




    public class AssessmentGenerationRand
    {
        [Key]
        //public int Assessment_generationID { get; set; }
        public string QuestionMixtype { get; set; }
        public int No_of_Questions { get; set; }
        
        public int TimeEstimateInputMin { get; set; }
        public int TimeEstimateInputMax { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }

        public string[] Topics { get; set; }

        public int[] RandomserialNumbers { get; set; }

        // public string no_of_times_used { get; set; }

    }



    public class FormRequest
    {
        public List<int> SelectedTopics { get; set; }
        public List<int> UsersList { get; set; }
    }



}
