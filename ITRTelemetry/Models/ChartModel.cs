using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITRTelemetry.Models
{
    public class ChartModel
    {
        //public string Criteria { get; set; }
        //public int TodaySales { get; set; }
        //public int ThisMonthUnits { get; set; }
        //public int YesterdaySales { get; set; }
        //public int LastMonthUnits { get; set; }
        public List<decimal> Data { get; set; }
        public string Label { get; set; }

        public string ParameterID { get; set; }
        public ChartModel()
        {
            Data = new List<decimal>();
        }


    }
    //public class MyArray
    //{
       
    //}

    //public class Root
    //{
    //    public List<MyArray> MyArray { get; set; }
    //}
}
