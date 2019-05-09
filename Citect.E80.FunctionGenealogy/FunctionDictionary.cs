using System;
using System.IO;
using System.Collections.Generic;

namespace Citect.E80.FunctionGenealogy
{
    public class FunctionDictionary
    {
        /// <summary>
        /// get list of functions from ci file in root directory (includes subs)
        /// </summary>
        /// <param name="root"></param>
        public static List<CicodeFunctions> GetFunctions(string root)
        {
            //get list of files
            var searchList = Directory.GetFiles(root, "st_*.ci", SearchOption.AllDirectories);
            var cicodefunctions = new List<CicodeFunctions>();
            
            foreach (var file in searchList)
            {
                //open the ci file and get list of functions
                var lineNum = 1;
                var funcDefInNextLine = false;
                var funcLoc = new Dictionary<int, string>();
                var functionList = new CicodeFunctions() { FileName = file };

                using (var streamReader = new StreamReader(file))
                {
                    while (!streamReader.EndOfStream)
                    {
                        try
                        {
                            string funcName = string.Empty;
                            var line = streamReader.ReadLine();
                            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

                            if (funcDefInNextLine)
                            {
                                if (line.IndexOf('(') > 0)
                                {
                                    funcName = line.Substring(0, line.IndexOf('('));
                                    if (funcName.Length > 1)
                                        if (!funcLoc.ContainsKey(lineNum))
                                            funcLoc.Add(lineNum, funcName.Trim());
                                }
                                funcDefInNextLine = false;
                            }

                            //use regex to find function definition?
                            if (line.Contains("FUNCTION"))
                            {
                                //name of function follows the keyword
                                if (line.IndexOf('(') > 0)
                                {
                                    var first = line.IndexOf("FUNCTION") + "FUNCTION".Length;
                                    funcName = line.Substring(first, line.IndexOf('(') - first);
                                    if (funcName.Length > 1)
                                        if (!funcLoc.ContainsKey(lineNum))
                                            funcLoc.Add(lineNum, funcName.Trim());
                                }
                                else
                                    funcDefInNextLine = true;
                            }

                            if (funcName.Length > 1)
                            {
                                if (!funcLoc.ContainsKey(lineNum))
                                    funcLoc.Add(lineNum, funcName.Trim());
                            }
                            lineNum++;
                        }
                        catch (Exception ex)
                        {
                            
                        }
                    }
                }
                functionList.FunctionLocations = funcLoc;
                cicodefunctions.Add(functionList);
            }
            return cicodefunctions;
        }

        public static bool CheckFunctionUsedInDBF(string root)
        {
            throw new NotImplementedException();
        }
    }

   
    public class CicodeFunctions
    {
        public string FileName { get; set; }
        public Dictionary<int, string> FunctionLocations { get; set; }
    }        
}
