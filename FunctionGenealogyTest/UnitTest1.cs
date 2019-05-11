using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Citect.E80.FunctionGenealogyTest
{
    [TestClass]
    public class FunctionDictionaryTest
    {
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void FunctionDictionaryTest_ShouldNotBeNull()
        {
            var functions = FunctionGenealogy.FunctionDictionary.GetFunctions(@"C:\ProgramData\Schneider Electric\Citect SCADA 2018\User");
            Assert.IsNotNull(functions, "cannot be null");

            foreach (var data in functions)
            {
                Assert.IsNotNull(data.FunctionLocations,"function locations cannot be null");
                foreach (var kvp in data.FunctionLocations)
                {
                    TestContext.WriteLine("file:{0},line:{1},funcName:{2}", data.FileName, kvp.Key, kvp.Value);
                }
            }

            FunctionGenealogy.FunctionDictionary.FunctionRefExistInCicode();

            Assert.IsNotNull(FunctionGenealogy.FunctionDictionary.cicodefunctions);
            //list of function received
            //get functions that are referenced
            //FunctionGenealogy.FunctionDictionary.GetProjectDBF(@"C:\ProgramData\Schneider Electric\Citect SCADA 2018\User", "QRTP");

            Assert.IsNotNull(FunctionGenealogy.FunctionDictionary.cicodefunctions);

            //FunctionGenealogy.FunctionDictionary.PrintFunctionReferences();
        
        }
    }
}
