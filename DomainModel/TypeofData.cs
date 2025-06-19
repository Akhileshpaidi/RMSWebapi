using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DomainModel
{
    [Table("typeofdata")]
    public class TypeofData
    {

        [Key]
        public int TIID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public string Status { get; set; }
        public string DateTime { get; set; }
    }
}
