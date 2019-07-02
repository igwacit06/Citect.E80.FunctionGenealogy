using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace Citect.E80.FunctionGenealogyTest
{
    [TestClass]
    public class EquipmentTypeXML_Test
    {
        [TestMethod]
        public void EquipmentTypeGenerate_ShouldNotBeNull()
        {
            var testgenerate = new EquipmentTypeXml.EquipmentTypeGenerate();   
            Assert.IsTrue(testgenerate.ConvertTagsInExcelToTemplate());
        }


        [TestMethod]
        public void RegexTest()
        {
            string value = "343532";
            var output = Regex.Match(value, "\\d+");
        }
    }
}
