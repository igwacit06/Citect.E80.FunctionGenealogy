using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citect.E80.EquipmentTypeXml
{
    public class EquipmentTypeOutputParam
    {
        public string TagGenLink { get; set; }
        public string FileName { get; set; }
        public string TagName { get; set; }
        public string Suffix { get; set; }
        public string Prefix { get; set; }
        public string Comment { get; set; }
        public string DataType { get; set; }
        public int AddressOffset { get; set; }
        public int RawZero { get; set; }
        public int RawFull { get; set; }
        public int EngZero { get; set; }
        public int EngFull { get; set; }
        public string Units { get; set; }
        public string Format { get; set; }
    }
}
