using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITRTelemetry.Models
{
    public class MultiChartModel
    {
        public List<decimal> Data { get; set; }
        public MultiChartModel()
        {
            Data = new List<decimal>();
        }
    }
}
