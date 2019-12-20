using System;
using System.IO;
using System.Collections.Generic;
using log4net;
using System.Data;
using System.Linq;
using DbfDataReader;
using System.Text.RegularExpressions;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Citect.E80.FunctionGenealogy
{
    public class FunctionDictionary
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static List<CicodeFunctions> cicodefunctions = new List<CicodeFunctions>();
        private static LogCsv LogCsv;
        private static string[] ExcludeDBF = new string[] { "computer", "contype", "cluster", "English", "groups", "include", "modems", "netaddr", "oid", "OPCSrv", "profile", "protdir", "reports", "roles", "users", "units", "profile", "labels", "EWSSrv", "remap", "pglang", "ports", "scanner", "scrprfl", "trnsrv", "fonts", "devices", "boards" };

        /// <summary>
        /// get list of functions from ci file in root directory (includes subs)
        /// </summary>
        /// <param name="root"></param>
        public static List<CicodeFunctions> GetFunctions(string root)
        {
            //get list of files
            var path = Path.GetDirectoryName(root);
            var searchList = Directory.GetFiles(path, "st_*.ci", SearchOption.AllDirectories);

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
                            if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                            {
                                lineNum++;
                                continue;
                            }

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
                                {
                                    funcLoc.Add(lineNum, funcName.Trim());
                                }
                            }
                            lineNum++;
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exception:", ex);
                        }
                    }
                }
                functionList.FunctionLocations = funcLoc;
                cicodefunctions.Add(functionList);
            }
            return cicodefunctions;
        }

        /// <summary>
        /// find the functions that exist in Cicode
        /// </summary>
        public static void FunctionRefExistInCicode()
        {
            var cicodefilelist = cicodefunctions.Select(a => new { a.FileName }).ToList();


            foreach (var funcLoc in cicodefunctions)
            {
                //search against all list cicode file contents for function ref.
                foreach (var kvp in funcLoc.FunctionLocations)
                {
                    foreach (var file in cicodefilelist)
                    {
                        //open the file stream
                        //check function against file stream
                        var found = 0;
                        var lineNum = 1;
                        //get file name
                        var ciFile = file.FileName.Substring(file.FileName.LastIndexOf('\\'), file.FileName.IndexOf('.') - file.FileName.LastIndexOf('\\')).TrimStart(new char[] { '\\', '*', '?' });
                        using (var streamReader = new StreamReader(file.FileName))
                        {
                            while (!streamReader.EndOfStream)
                            {
                                //for debugging only
                                //if (funcLoc.FileName.Contains("multimonitors") && file.FileName.Contains("multimonitors"))
                                //    if (lineNum == 1148 && kvp.Value.Contains("_MMNumPadKey0"))
                                //        log.Debug("stop test");

                                try
                                {
                                    var line = streamReader.ReadLine().Trim();
                                    if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.Contains("FUNCTION") || line.EndsWith("*/") || line.StartsWith("/*") || line.StartsWith("!"))
                                    {
                                        lineNum++;
                                        continue;
                                    }

                                    //use pattern recognition
                                    if (Regex.IsMatch(line, string.Format(@"(?<=^([^\""]|\""[^\""]*\"")*)\b{0}\b", kvp.Value)))
                                    {
                                        if (funcLoc.FileName.Equals(file.FileName) && lineNum.Equals(kvp.Key))
                                        {
                                            lineNum++;
                                            continue;
                                        }

                                        found++;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error("streamreader:error", ex);
                                }
                                lineNum++;
                            }
                        }

                        if (found > 0)
                        {
                            //locate function in the reference
                            if (funcLoc.FunctionCicodeReferences.ContainsKey(kvp.Value))
                            {
                                var result = funcLoc.FunctionCicodeReferences[kvp.Value];
                                var metadata = result.Find(s => s.Name.Equals(ciFile));
                                if (metadata != null)
                                    metadata.ReferenceCount += found;
                                else
                                    result.Add(new CicodeMetaDetails { Name = ciFile, ReferenceCount = found });
                            }
                            else
                            {
                                var metadata = new List<CicodeMetaDetails> { new CicodeMetaDetails { Name = ciFile, ReferenceCount = found } };
                                funcLoc.FunctionCicodeReferences.Add(kvp.Value, metadata);
                            }
                        }
                    }
                }
            }

            LogCsv = new LogCsv(@"C:\QR_PSCS\Logs\FuncRefInCicode.csv", true);
            LogCsv.WriteToFile("cicodeFile,Function Name,cicode File,References");
            foreach (var funcLoc in cicodefunctions)
            {
                //search against all list cicode file contents for function ref.
                var ciFile = funcLoc.FileName.Substring(funcLoc.FileName.LastIndexOf('\\'), funcLoc.FileName.IndexOf('.') - funcLoc.FileName.LastIndexOf('\\')).TrimStart(new char[] { '\\', '*', '?' });

                foreach (var kvp in funcLoc.FunctionLocations)
                {
                    //get value of kvp.value from dictionary
                    if (funcLoc.FunctionCicodeReferences.ContainsKey(kvp.Value))
                    {
                        var metaDetails = funcLoc.FunctionCicodeReferences[kvp.Value];
                        metaDetails.ForEach(s => LogCsv.WriteToFile("{0},{1},{2},{3}", ciFile, kvp.Value, s.Name, s.ReferenceCount));
                    }
                    else
                        LogCsv.WriteToFile("{0},{1},{2},{3}", ciFile, kvp.Value, "", 0);
                }
            }

            LogCsv.CloseFile();
            //foreach function check existance in all cicode files?

            //get filepath into list
            //foreach function open file and check existance in that cicode file.
            //ignore lines where it begins with // or /* or */

        }

        /// <summary>
        /// setup the project dbfs.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="projectprefix"></param>
        /// <returns></returns>
        public static bool FunctionRefExistInDBFs(string root, string projectprefix)
        {
            var path = Path.GetDirectoryName(root);
            var searchList = Directory.GetFiles(path, "*.dbf", SearchOption.AllDirectories);
            var dbflistToSearch = new Dictionary<string, string>();
            var dbfReadList = new Dictionary<string, DbfTable>();

            foreach (var file in searchList)
            {
                if (!file.Contains(projectprefix) || file.Contains("_FUNCSYM") || file.Contains("_funcsym")) continue;

                var tableName = file.Substring(file.LastIndexOf('\\'), file.IndexOf('.') - file.LastIndexOf('\\')).TrimStart(new char[] { '\\', '*', '?' });

                //exclude dbf in excluded list
                if (ExcludeDBF.Contains(tableName)) continue;

                //dbfReadList.Add(file, new DbfTable(file, System.Text.Encoding.UTF8));

                dbflistToSearch.Add(file, tableName);

                //update list of CicodeFunctions
                foreach (var listfunctions in cicodefunctions)
                {
                    foreach (var kvp in listfunctions.FunctionLocations)
                    {
                        if (!listfunctions.FunctionDBFReferences.ContainsKey(kvp.Value))
                        {
                            var metadetails = new List<CicodeMetaDetails>() { new CicodeMetaDetails { Name = tableName, ReferenceCount = 0 } };
                            listfunctions.FunctionDBFReferences.Add(kvp.Value, metadetails);
                        }
                        else
                        {
                            var metadetails = listfunctions.FunctionDBFReferences[kvp.Value];
                            if (metadetails == null || !metadetails.Exists(s => s.Name == tableName))
                            {
                                metadetails.Add(new CicodeMetaDetails { Name = tableName, ReferenceCount = 0 });
                            }

                        }
                    }
                }
            }


            LogCsv = new LogCsv(@"C:\QR_PSCS\Logs\cicodefuncDBFRef.csv", true);
            LogCsv.WriteToFile("FunctionName,Path,DBF,References");
            foreach (var funcLoc in cicodefunctions)
            {
                foreach (var kvpfunc in funcLoc.FunctionLocations)  //key is the line number, value is the functionName
                {
                    var funcsearch = DateTime.UtcNow;
                    foreach (var kvp in dbflistToSearch)    //key is tablename, value is dbftable object
                    {
                        var tableName = kvp.Key.Substring(kvp.Key.LastIndexOf('\\'), kvp.Key.IndexOf('.') - kvp.Key.LastIndexOf('\\')).TrimStart(new char[] { '\\', '*', '?' });

                        var datettime = DateTime.UtcNow;
                        //var refcount = FunctionRefExistsInDBF(kvp.Value, kvpfunc.Value);
                        var refcount = FunctionRefExistsInDBF(kvp.Key, kvpfunc.Value);
                        var dbfmeta = funcLoc.FunctionDBFReferences[kvpfunc.Value];
                        CicodeMetaDetails metaDetails = null;

                        //find from cicodemeta list in dbf references
                        if (funcLoc.FunctionDBFReferences.ContainsKey(kvpfunc.Value))
                        {
                            metaDetails = dbfmeta.FirstOrDefault(s => s.Name == tableName);
                        }

                        if (metaDetails == null)
                        {
                            log.DebugFormat("unable to locate {0}", kvpfunc.Value);
                            continue;
                        }

                        if (refcount > 0)
                            metaDetails.ReferenceCount += refcount;

                        LogCsv.WriteToFile("{0},{1},{2},{3}", kvpfunc.Value, kvp.Key, metaDetails.Name, metaDetails.ReferenceCount);

                        //log.DebugFormat("time to search dbf {0}-{1}", metaDetails.Name, DateTime.UtcNow.Subtract(datettime).TotalMilliseconds);
                    }
                    log.DebugFormat("Total time:{0} - dbfs {1} for {2} ", DateTime.UtcNow.Subtract(funcsearch).TotalMilliseconds, dbflistToSearch.Count, kvpfunc.Value);
                }
            }

            //release all resources
            //foreach (var kvp in dbfReadList)
            //    kvp.Value.Dispose();

            LogCsv.CloseFile();
            return true;
        }



        /// <summary>
        /// overloaded
        /// </summary>
        /// <param name="dbfTable"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private static int FunctionRefExistsInDBF(DbfTable dbfTable, string functionName)
        {
            var dbfRecord = new DbfRecord(dbfTable);
            var found = 0;
            var skipDeleted = true;

            while (dbfTable.Read(dbfRecord))
            {
                if (skipDeleted && dbfRecord.IsDeleted)
                    continue;

                if (dbfRecord.Values.Any(s => s.GetValue().ToString().Contains(functionName)))
                {
                    found++;
                }
            }

            return found;
        }



        /// <summary>
        /// find reference of function name in dbf
        /// </summary>
        /// <param name="path"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private static int FunctionRefExistsInDBF(string path, string functionName)
        {
            var skipDeleted = true;
            //DbfDataReader.DbfDataReader reader = new DbfDataReader.DbfDataReader(kvp.Key);
            using (var dbfTable = new DbfTable(path, System.Text.Encoding.UTF8))
            {
                try
                {
                    var dbfRecord = new DbfRecord(dbfTable);
                    var found = 0;
                    while (dbfTable.Read(dbfRecord))
                    {
                        if (skipDeleted && dbfRecord.IsDeleted)
                            continue;

                        if (dbfRecord.Values.Any(s => s.GetValue().ToString().Contains(functionName)))
                        {
                            found++;
                            break;
                        }
                    }
                    return found;
                }
                catch (Exception ex)
                {
                    log.Error("error:", ex);
                    return 0;
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private static bool CheckFunctionRefInDBF(DataTable dataTable, string tableName)
        {
            //foreach (var listfunctions in cicodefunctions)
            //{
            //    foreach (var kvp in listfunctions.FunctionDBFReferences)
            //    {
            //        var found = 0;
            //        foreach (DataRow row in dataTable.Rows)
            //        {
            //            foreach (DataColumn col in dataTable.Columns)
            //            {
            //                if (row.IsNull(col)) continue;
            //                if (row[col].ToString().Contains(kvp.Key))
            //                    found++;
            //            }
            //        }
            //        if (found > 0)
            //        {
            //            var metadetails = kvp.Value.Find(s => s.Name == tableName);
            //            if (metadetails != null)
            //                metadetails.ReferenceCount++;
            //        }
            //    }
            //}
            //return true;
            throw new NotImplementedException();
        }


        private static bool CheckFunctionRefInCode()
        {
            throw new NotImplementedException();
        }

    }
}
