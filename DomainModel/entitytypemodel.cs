using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("entitytypemaster")]
    public class entityTypeModel

    {
        [Key]
        public int entitytypeid { get; set; }

        public string entitytypename { get; set; }

        public string entitytypeDesc {  get; set; }
        public string entitytypestatus { get; set; }

        public string createddate {  get; set; }

        public int createdBy {  get; set; }

        public string entitytypetablename {  get; set; }

        public string source { get; set; }
    }
}
