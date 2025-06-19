using System;

namespace ITR_TelementaryAPI.Models.SignalR
{

    public class ParametersModel
    {
        public ParametersModel(decimal price)
        {

        }

        public int ParamDataID { get; set; }

        public int ParameterID { get; set; }
        public string ParameterName { get; set; }
        public string ParamData { get; set; }

        public string EquipmentName { get; set; }
        public DateTime Date { get; set; }

        public DateTime LastUpdate { get; set; }

        public void Update()
        {
            bool isNewDay = LastUpdate.Day != DateTime.Now.Day;
            LastUpdate = DateTime.Now;

        }
    }
}

//        public class ParametersModel {
//        public ParametersModel(decimal price)
//        {
//            //Price = price;
//            //DayMax = price;
//            //DayMin = price;
//            //_initPrice = Price;
//        }
//        //public string Symbol { get; set; }
//        //public decimal Price { get; set; }
//        //public decimal DayMax { get; set; }
//        //public decimal DayMin { get; set; }
//        //public decimal DayOpen { get; set; }
//        // public int FlightID { get; set; }
//        // public string UDPPacketSequenceNo { get; set; }
//        // public string UPDPacketID { get; set; }
//        //public string StartByte { get; set; }
//        //public string EndByte { get; set; }
//        //public string Remark { get; set; }
//        //public string DataType { get; set; }
//        //public int EquipmentID { get; set; }

       
//        //public string FrameRate { get; set; }
//        //public string FrameLength { get; set; }
//        //public string FrameDescription { get; set; }
//       // public string Status { get; set; }

//        public int ParamDataID { get; set; }

//        public int ParameterID { get; set; }
//        public string ParameterName { get; set; }
//        public string ParamData { get; set; }

//        public string EquipmentName { get; set; }
//        public DateTime Date { get; set; }

//        public DateTime LastUpdate { get; set; }

//        //public string DateTime { get; set; }
     

//        //public decimal Change {
//        //    get {
//        //        return Price - DayOpen;
//        //    }
//        //}
//        //public double PercentChange {
//        //    get {
//        //        return (double)Math.Round(Change * 100 / DayOpen, 2);
//        //    }
//        //}

//        //decimal _initPrice;
//        public void Update() {
//            bool isNewDay = LastUpdate.Day != DateTime.Now.Day;
//            LastUpdate = DateTime.Now;


//            //decimal change = GenerateChange();
//            //decimal newPrice = _initPrice + _initPrice * change;

//            //Price = newPrice;

//            //if(Price > DayMax || isNewDay)
//            //    DayMax = Price;
//            //if(Price < DayMin || isNewDay)
//            //    DayMin = Price;
//        }

//        //static Random random = new Random();

//        //decimal GenerateChange() {
//        //    return (decimal)random.Next(-200, 200) / 10000;
//        //}
//    }
//}
