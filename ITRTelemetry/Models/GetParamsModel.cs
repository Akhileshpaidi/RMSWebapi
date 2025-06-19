using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITRTelemetry.Models
{
    public class GetParamsModel
    {
        public int ParameterID { get; set; }
        public string label { get; set; }
        public string[] data { get; set; }

        public string[] time { get; set; }
        public string borderColor { get; set; }

        public string fill { get; set; }
        

    }
}