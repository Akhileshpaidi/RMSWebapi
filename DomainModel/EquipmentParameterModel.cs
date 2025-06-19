using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    [Table("equipmentparametermaster")]
    public class EquipmentParameterModel
    {
        [Key]
        ///public int ParameterSrNo { get; set; }
        public int ParameterID { get; set; }

        public int ParameterTypeID { get; set; }
        public string ParameterName { get; set; }
       
        public string StartByte { get; set; }
        public string EndByte { get; set; }
        public string Remark { get; set; }
        public string DataType { get; set; }
        public int EquipmentID { get; set; }
        public string FrameRate { get; set; }
        public string FrameLength { get; set; }
        public string FrameDescription { get; set; }
        public string DateTime { get; set; }
        public string Status { get; set; }
        public string Minimum { get; set; }
        public string Maximum { get; set; }

    }
    public class EquipmentParameterExcelModel
    {
        
        ///public int ParameterSrNo { get; set; }
       
        public string ParameterName { get; set; }

        public string StartByte { get; set; }
        public string EndByte { get; set; }
        public string Remark { get; set; }
        public string DataType { get; set; }
        public int EquipmentID { get; set; }
        public string Minimum { get; set; }
        public string Maximum { get; set; }
        public string  ParameterGroupId { get; set; }



    }
    public class EquipmentParameterExcelModelArr
    {
        public EquipmentParameterExcelModel[] EquipmentParameterExcelModel { get; set; }
    }
}
