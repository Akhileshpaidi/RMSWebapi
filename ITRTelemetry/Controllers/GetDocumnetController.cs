using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Data;
using MySql.Data.MySqlClient;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class GetDocumnetController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;

        public GetDocumnetController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

//        [Route("api/ProvideAccess/GetProvideAccessDetails/{Select_Opt}")]
//        [HttpGet]

//        public IEnumerable<ProvideAccessModel> GetProvideAccessDetails(string Select_Opt)
//        {


//            if (this.mySqlDBContext == null)
//            {
//                // Handle the case when mySqlDBContext is null
//                return new List<ProvideAccessModel>(); // Or return an appropriate result
//            }

//            if (string.IsNullOrEmpty(Select_Opt))
//            {
//                // Handle the case when Select_Opt is null or empty
//                return new List<ProvideAccessModel>(); // Or return an appropriate result
//            }

//            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=risk;sslmode=none;");
//            con.Open();
//            MySqlCommand cmd = new MySqlCommand("SELECT " +
//                "e.Select_Opt, " +
//    "e.AddDoc_id," +
//    "e.Title_Doc," +
//    "t.DocTypeName," +
//    "C.Doc_CategoryName," +
//    "sc.Doc_SubCategoryName," +
//    "a.AuthorityName," +
//   "at.AuthorityTypeName," +
//    "p.NatureOf_Doc_Name " +
//   "FROM " +
//    "risk.add_doc e" +
//" JOIN " +
//    "risk.doctype_master t ON t.DocTypeID = e.DocTypeID" +
//" JOIN " +
//    "risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID" +
//" JOIN " +
//    "risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID" +
//" JOIN " +
//    "risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID" +
//" JOIN " +
//    "risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID" +
//" JOIN " +
//    "risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id" +

//     " WHERE Select_Opt ='"+Select_Opt+"';", con);

//            cmd.CommandType = CommandType.Text;

//            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

//            DataTable dt = new DataTable();
//            da.Fill(dt);
//            con.Close();
//            var pdata = new List<ProvideAccessModel>();
//            if (dt.Rows.Count > 0)
//            {
//                for (var i = 0; i < dt.Rows.Count; i++)
//                {
//                    pdata.Add(new ProvideAccessModel
//                    {
//                        AuthorityName = dt.Rows[i]["AuthorityName"].ToString(),
//                        AddDoc_id = Convert.ToInt32(dt.Rows[i]["AddDoc_id"].ToString()),

//                        AuthorityTypeName = dt.Rows[i]["AuthorityTypeName"].ToString(),
//                        Title_Doc = dt.Rows[i]["Title_Doc"].ToString(),
//                        NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"].ToString(),
//                        DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
//                        Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
//                        Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),

//                    });
//                }
//            }
//            return pdata;

//        }
    

 }
}
