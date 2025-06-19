using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace DomainModel
{
    [Table("countries")]
    public class CountryModel
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string phonecode { get; set; }
        //public string region { get; set; }
        //public string nationality { get; set; }
        //public int region_id { get; set; }
    }

    [Table("states")]
    public class StateModel
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        //public string phonecode { get; set; }
        //public string region { get; set; }
        //public string nationality { get; set; }
        public int country_id { get; set; }
    }
}