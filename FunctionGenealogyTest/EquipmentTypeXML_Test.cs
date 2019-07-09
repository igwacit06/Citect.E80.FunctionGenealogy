using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace Citect.E80.FunctionGenealogyTest
{
    [TestClass]
    public class EquipmentTypeXML_Test
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void EquipmentTypeGenerate_ShouldNotBeNull()
        {
            var CitectEquipmentTypeGenerate = new EquipmentTypeXml.EquipmentTypeGenerate();   
            Assert.IsTrue(CitectEquipmentTypeGenerate.Execute_SetupTags());
        }


        [TestMethod]
        public void RegexTest()
        {
            string match = "BP04";
            string value = "S_SFC_BP04_COMMS_OK";
            var output = Regex.Match(value, "\\d+");

            var test = value.Substring(value.IndexOf(match) + match.Length);            
            TestContext.WriteLine("test:{0}", test.TrimStart(new char[] { '_', '.', ',' }));


            var test2 = "auto";
            test = char.ToUpper(test2[0]) + test2.Substring(1);
            TestContext.WriteLine("value:{0}", test);
        }
    }
}
