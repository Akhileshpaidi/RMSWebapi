using System;

namespace ITR_TelementaryAPI.Models.SignalRTickData
{
    public class ParameterData
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int Volume { get; set; }

        public string ParameterName { get; set; }
        public string ParamData { get; set; }

        public string ParameterTypeID { get; set; }

        public string ParameterID { get; set; }

        public DateTime UpdateDate { get; set; }
        public string SystemName { get; set; }


        public ParameterData(decimal price, int volume, DateTime date, string name, string value, DateTime updatedate, string ParamType, string ParamID, string EquipmentName)
        {
            Date = date;
            Price = price;
            Volume = volume;
            ParameterName = name;
            ParamData = value;
            UpdateDate = Convert.ToDateTime(updatedate);
            ParameterTypeID = ParamType;
            ParameterID = ParamID;
            SystemName = EquipmentName;
        }
    }

    public class SystemParamData
    {
        public int ParameterID { get; set; }
        public int ParameterTypeID { get; set; }
        public string ParameterName { get; set; }
        public string ParamData { get; set; }
        public DateTime Date { get; set; }

        public SystemParamData(int ParamID,int ParamType,string name,string value, DateTime date)
        {
          
            ParameterID = ParamID;
            ParameterTypeID = ParamType;
            ParameterName = name;
            ParamData = value;
            Date = date;
              
        }

    }

    public class ParametersDataXY
    {
        public int ParameterID { get; set; }
        public string ParamData { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }


        public ParametersDataXY(int ParamID, string ParamValue, DateTime date, int Count)
        {
            ParameterID = ParamID;
            ParamData = ParamValue;
            Date = date;
            Count = Count;
        }
    }
}