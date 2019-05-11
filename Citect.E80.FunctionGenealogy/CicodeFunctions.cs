using System.Collections.Generic;
//[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Citect.E80.FunctionGenealogy
{
    /// <summary>
    /// 
    /// </summary>
    public class CicodeFunctions
    {
        public string FileName { get; set; }

        /// <summary>
        /// dictionary for function consisting of line number of function in cicode and name of function
        /// </summary>
        public Dictionary<int, string> FunctionLocations { get; set; }

        /// <summary>
        /// dictionary for individual function and any references made in dbf or cicode
        /// </summary>
        public Dictionary<string, List<CicodeMetaDetails>> FunctionDBFReferences { get; set; }

        public Dictionary<string, List<CicodeMetaDetails>> FunctionCicodeReferences { get; set; }

        public CicodeFunctions()
        {
            FunctionDBFReferences = new Dictionary<string, List<CicodeMetaDetails>>();
            FunctionCicodeReferences = new Dictionary<string, List<CicodeMetaDetails>>();
        }
    }
}
