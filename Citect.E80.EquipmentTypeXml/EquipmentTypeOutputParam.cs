using System.Collections.Generic;

namespace Citect.E80.EquipmentTypeXml
{
    public enum BaseAddr
    {
        Invalid,
        Status,
        Analog,
        Alarm
    }

    /// <summary>
    /// Class Defining parameters into EquipmentType Template Output section
    /// </summary>
    public class EquipmentTypeOutputParam
    {
        public string EquipName { get; set; }
        public BaseAddr BaseAddressParam { get; set; }
        public int BaseAddress { get; set; }
        public int TagAddress { get; set; }
        public int AddressOffset { get; set; }
        public string TagGenLink { get; set; }        
        public string TagName { get; set; }
        public string Suffix { get; set; }
        public string Prefix { get; set; }
        public string Comment { get; set; }
        public string Equipment { get; set; }
        public string Cluster { get; set; }
        public string DataType { get; set; }
        public string RawZero { get; set; }
        public string RawFull { get; set; }
        public string EngZero { get; set; }
        public string EngFull { get; set; }
        public string Units { get; set; }
        public string Format { get; set; }        
        public string FuncName { get; set; }
        public string DeviceIO { get; set; }
        public bool SetTrends { get; set; }
        public bool IsCalulated { get; set; }
        public Dictionary<string, int> BaseAddrPairs { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// cstor
        /// </summary>
        public EquipmentTypeOutputParam()
        {
            foreach (BaseAddr item in System.Enum.GetValues(typeof(BaseAddr)))
                BaseAddrPairs.Add(item.ToString(), 0);

            DeviceIO = "{equipment.IODEVICE}";
            Equipment = "{equipment.name}";
            Cluster = "{equipment.cluster}";
            IsCalulated = false;

        }

        /// <summary>
        /// setup the Equipment template
        /// </summary>
        /// <returns></returns>
        public List<templateOutput> GetXmlTemplateOutput()
        {
            var templateOutputs = new List<templateOutput>();            
            TagGenLink = EquipName + "." + Suffix;

            if (BaseAddressParam.Equals(BaseAddr.Status))
            {
                AddressOffset = TagAddress - BaseAddrPairs[BaseAddr.Status.ToString()];
                templateOutputs.Add(TomPriceEquipmentTemplate.GetEquipmentType_VarDiscreteOutputs(this));
            }

            //require both tag and alarm tag
            if (BaseAddressParam == BaseAddr.Alarm)
            {
                AddressOffset = TagAddress - BaseAddrPairs[BaseAddr.Alarm.ToString()];
                templateOutputs.Add(
                TomPriceEquipmentTemplate.GetEquipmentType_VarDiscreteOutputs(this));
                templateOutputs.Add(TomPriceEquipmentTemplate.GetEquipmentType_DigAlmOutputs(this));
            }

            //create tag and trend tag (if required)
            if (BaseAddressParam.Equals(BaseAddr.Analog))
            {
                AddressOffset = TagAddress - BaseAddrPairs[BaseAddr.Analog.ToString()];
                templateOutputs.Add(TomPriceEquipmentTemplate.GetEquipmentType_VarAnalogOutputs(this));

                if (SetTrends)
                    templateOutputs.Add(TomPriceEquipmentTemplate.GetEquipmentType_TrnOutputs(this));
            }

            //create calculated variable (if specified)
            if (IsCalulated)            
                templateOutputs.Add(TomPriceEquipmentTemplate.GetEquipmentType_CalculatedCommandVariable(this));
            
            return templateOutputs;
        }
    }
}
