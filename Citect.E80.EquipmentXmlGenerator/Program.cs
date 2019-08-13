using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citect.E80.EquipmentXmlGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Preparing Equipment Type xml");
            var CitectEquipmentTypeGenerate = new EquipmentTypeXml.EquipmentTypeGenerate();
            if (CitectEquipmentTypeGenerate.Execute_SetupTags())
                Console.WriteLine("Equipment xml generated");
            else
                Console.WriteLine("check logs for errors?");
            
            Console.ReadKey();
        }
    }
}
