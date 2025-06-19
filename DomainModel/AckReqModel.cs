using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    public class AckReqModel
    {
        [Key]

        public int AddDoc_id { get; set; }
        public int Document_Id { get; set; }
        public string document_name { get; set; }
        public string Title_Doc { get; set; }
        public string Sub_title_doc { get; set; }
        public string Obj_Doc { get; set; }
        public int NatureOf_Doc_id { get; set; }


        public string NatureOf_Doc_Name { get; set; }

        public int DocTypeID { get; set; }

        public string DocTypeName { get; set; }

        public int Doc_CategoryID { get; set; }
        public string Doc_CategoryName { get; set; }

        public int Doc_SubCategoryID { get; set; }

        public string Doc_SubCategoryName { get; set; }

        public int AuthorityTypeID { get; set; }
        public string AuthorityTypeName { get; set; }

        public int AuthoritynameID { get; set; }
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
        public string Keywords_tags { get; set; }
        public string pub_doc { get; set; }
        public string publisher_comments { get; set; }
        public string indicative_reading_time { get; set; }
        public string Time_period { get; set; }

        public string Publisher_Date_Range { get; set; }

    }
}
