using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    [Table("parametertypemaster")]
    public class ParameterTypeModel
    {
        [Key]
        public int ParameterTypeID { get; set; }
        public string ParameterType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string DateTime { get; set; }
    }
}
