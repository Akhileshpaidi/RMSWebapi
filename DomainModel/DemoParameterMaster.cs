using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DomainModel
{
    public class DemoParameterMaster
    {
        [Key]
        public int ParameterID { get; set; }
        public string ParameterName { get; set; }
        public string DataType { get; set; }
        public string StartByte { get; set; }
        public string EndByte { get; set; }
        public string ParamValue { get; set; }
        public string ParamType { get; set; }

    }

    public class DemoParameters
    {
        [Key]

        public int ParameterDataID { get; set; }
        public int ParameterID { get; set; }
        public string ParameterName { get; set; }
        public string ParamValue { get; set; }
        public string ParamType { get; set; }

        public DemoParameters(int parameterDataID, int parameterID, string parameterName, string paramValue, string paramType)
        {
            ParameterDataID = parameterDataID;
            ParameterID = parameterID;
            ParameterName = parameterName;
            ParamValue = paramValue;
            ParamType = paramType;
        }
        public DemoParameters( int parameterID,string paramValue)
        {

            ParameterID = parameterID;
            ParamValue = paramValue;
            
        }
        public DemoParameters()
        {

        }

    }
    public class PlotingDataParameters
    {
        public int PacketID { get; set; }

        public string Time { get; set; }
        public string X1 { get; set; }
        public string X2 { get; set; }
        public string X3 { get; set; }
        public string X4 { get; set; }
        public string X5 { get; set; }

        public string Y1 { get; set; }
        public string Y2 { get; set; }
        public string Y3 { get; set; }
        public string Y4 { get; set; }
        public string Y5 { get; set; }
    }
    public class PlotingData
    {
        public int PacketID { get; set; }
        public string X1 { get; set; }
        public string X2 { get; set; }
        public string X3 { get; set; }
        public string X4 { get; set; }
        public string X5 { get; set; }

        public string Y1 { get; set; }
        public string Y2 { get; set; }
        public string Y3 { get; set; }
        public string Y4 { get; set; }
        public string Y5 { get; set; }
        public string Time { get; set; }
        public string SystemTime { get; set; }
        public string Milliseconds { get; set; }
        public PlotingData(int PacketIDVal, string X1Val, string X2Val, string X3Val, string X4Val, string X5Val, string Y1Val, string Y2Val, string Y3Val, string Y4Val, string Y5Val, string TimeVal,string systemtime, string milliseconds)
        {
            PacketID = PacketIDVal;
            X1 = X1Val;
            X2 = X2Val;
            X3 = X3Val;
            X4 = X4Val;
            X5 = X5Val;
            Y1 = Y1Val;
            Y2 = Y2Val;
            Y3 = Y3Val;
            Y4 = Y4Val;
            Y5 = Y5Val;
            Time = TimeVal;
            SystemTime = systemtime;
            Milliseconds = milliseconds;


        }
    }
}
