using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel

{
    public class CustomerRegModel
    {
      [Key]
        public int CustomerRegID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailID { get; set; }
        public string OrganizationName { get; set; }
        public int GSTRegistered { get; set; }
        public string GSTRegisteredmain { get; set; }
        public string GSTNumber { get; set; }
        public string GST_Org_name { get; set; }
        public string GST_Address { get; set; }
        public int Supply_Country { get; set; }
        public string Supply_Country_name { get; set; }
        public int Supply_State { get; set; }
        public string Supply_State_name { get; set; }
        public string GST_Certificate { get; set; }
        public bool Same_as_Registered_Entity { get; set; }
        public string billing_org_name { get; set; }
        public string billing_gst_number { get; set; }
        public string billing_Address { get; set; }
        public int billing_country { get; set; }
        public int billing_state { get; set; }
        public bool Same_as_Registered_User { get; set; }
        public string Grp_Admin_firstname { get; set; }
        public string Grp_Admin_lastname { get; set; }
        public string Grp_Admin_Mobilenumber { get; set; }
        public string Grp_Admin_EmailID { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string SupplyStateName { get; set; }
        public string billingStateName { get; set; }
        public string billingcountryName { get; set; }
        public string supplycountryName { get; set; }



    }
    public class taskmodulesmodel
    {
        public int task_id { get; set; }
        public string task_name { get; set; }
        public string task_desc { get; set; }
    }


    public class customer_subscriptionmodel
    {
        [Key]
        public int Customer_SubscriptionID { get; set; }
        public int CustomerRegID { get; set; }
        public string task_ids { get; set; }
        public int No_of_Users { get; set; }
        public int No_of_Companys { get; set; }
        public int No_of_Locations { get; set; }
        public string BankName { get; set; }
        public string IFSC_Code { get; set; }
        public string BankAccountNo { get; set; }
        public string Unique_Sub_ID { get; set; }
        public string Status { get; set; }
        public DateTime Amt_Collection_Date { get; set; }
        public DateTime CreatedDate { get; set; }
        public String Subscriptiontype { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
 

    }
}
