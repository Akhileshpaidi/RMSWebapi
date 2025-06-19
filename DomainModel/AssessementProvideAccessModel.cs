using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("assement_provideacess")]
    public class assement_provideacess
    {
        [Key]
        public int Assessement_ProvideAccessID { get; set; }
        public string AssessementTemplateID { get; set; }
        public int assessment_builder_id { get; set; }
        public int EntityMasterID { get; set; }
        public int UnitLocationMasterID { get; set; }
        public int UserID { get; set; }
        public string Access_Permissions { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string GrantUserPermission { get; set; }

        public string UserloactionmappingID { get;set; }

        public int createdby {  get; set; }

    }


    [Table("ass_user_permissionlist")]
    public class ass_user_permissionlistModel
    {
        [Key]
        public int Ass_User_PermissionListid { get; set; }
        public string AssUserPermissionName { get; set; }

    }


    public class AssessmentProvideAccess
    {
        [Key]
        public int Assessement_ProvideAccessID { get; set; }
        public string AssessementTemplateID { get; set; }
        public int EntityMasterID { get; set; }
        public int user_id { get; set; }
        public int UnitLocationMasterID { get; set; }
        public int assessment_builder_id { get; set; }
        public string Access_Permissions { get; set; }
        public string GrantUserPermission { get; set; }
        public string Entity_Master_Name { get; set; }
        public string Unit_location_Master_name { get; set; }
        public string firstname { get; set; }
        public string assessment_name { get; set; }
        public int ass_User_PermissionListid { get; set; }

        public string UserloactionmappingID { get; set; }


    }



    public class GetAssUsersByAssTempModel
    {
        [Key]
        public int UserID { get; set; }
        public string firstname { get; set; }

        public string UserloactionmappingID { get; set; }
        public string Entity_Master_Name { get; set; }
        public string Unit_location_Master_name { get; set; }
    }


    public class assement_provideacessNew
    {
        [Key]
        public int Assessement_ProvideAccessID { get; set; }
        public string AssessementTemplateID { get; set; }
        public int assessment_builder_id { get; set; }
        public int EntityMasterID { get; set; }
        public int UnitLocationMasterID { get; set; }
        public string UserID { get; set; }
        public string Access_Permissions { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string GrantUserPermission { get; set; }

        public string UserloactionmappingID { get; set; }
        public string Entity_Master_Name { get; set; }
        public string Unit_location_Master_name { get; set; }
        public string assessment_name { get; set; }

    }

}
