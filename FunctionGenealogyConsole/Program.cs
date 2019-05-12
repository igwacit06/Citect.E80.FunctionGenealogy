using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citect.E80.FunctionGenealogy;

namespace FunctionGenealogyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var functions = FunctionDictionary.GetFunctions(@"C:\ProgramData\Schneider Electric\Citect SCADA 2016\User");
            LogCsv logCsv = new LogCsv(@"C:\QR_PSCS\Logs\cicodeFunctions.txt", true);
            foreach (var func in functions)
                foreach (var kvp in func.FunctionLocations)
                    logCsv.WriteToFile("{0},{1},{2}", func.FileName, kvp.Key, kvp.Value);
            logCsv.CloseFile();
            //list of function ref in cicode
            FunctionDictionary.FunctionRefExistInCicode();

            
            //list of function ref in dbf           
            FunctionDictionary.GetProjectDBF(@"C:\ProgramData\Schneider Electric\Citect SCADA 2016\User", "QRTP");

        }
    }
}
