﻿using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using ExcelDataReader;
using System.IO;
using Citect.E80.FunctionGenealogy;
using System.Text.RegularExpressions;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Citect.E80.BulkDBFUpdates
{
    public enum ToAddEnum
    {
        none,
        varonly,
        varalarm,
        vartrend,
        all
    }

    /// <summary>
    /// function: to output excel data into csv format
    /// this was setup to mainly deal with the work done at EDL. the excel was a customized formatted document.
    /// 
    /// </summary>
    public class DBFAutomation
    {
        public bool ExcelError { get; set; }
        public bool Validate { get; set; }

        private readonly string[] WorkSheetNames = new string[] { "Gens", "DMC Alarms", "Load Demand", "LV&Tx CBs", "Gen Annunciator", "Gas Gens", "Mill Starting", "BOP", "Q101-8 CBs", "Tariff Meters", "Dsl Flow", "Air Compressor", "751 PRs", "700G PRs", "Gas Skid", "HMI Cmds", "Misc Analogs", "Batt Chargers", "VSDs", "Command Statuses", "Gas Flow Meters", "FIP" };

        private readonly List<string> DigiAlmFields = new List<string>() { "TAG", "NAME_1", "DESC", "VAR_A", "VAR_B", "CATEGORY", "HELP", "PRIV", "AREA", "COMMENT_1", "SEQUENCE", "DELAY", "CUSTOM1", "CUSTOM2", "CUSTOM3", "CUSTOM4", "CUSTOM5", "CUSTOM6", "CUSTOM7", "CUSTOM8", "CLUSTER_1" };

        private readonly List<string> TrendFields = new List<string>() { "NAME_2", "EXPR", "TRIG", "SAMPLEPER\nMANUAL", "PRIV_1", "AREA_1", "ENG_UNITS_1", "FORMAT", "FILENAME", "FILES", "TIME", "PERIOD", "COMMENT_2", "TYPE_1", "SPCFLAG", "LSL", "USL", "SUBGRPSIZE", "XDOUBLEBAR", "RANGE", "SDEVIATION", "STORMETHOD", "CLUSTER_2" };

        private readonly List<string> VariableFields = new List<string>() { "NAME", "TYPE", "UNIT_2", "ADDR", "RAW_ZERO\nMANUAL", "RAW_FULL\nMANUAL", "ENG_ZERO\nMANUAL", "ENG_FULL\nMANUAL", "ENG_UNITS", "FORMAT\nMANUAL", "COMMENT", "EDITCODE", "LINKED", "OID", "REF1", "REF2", "DEADBAND", "CUSTOM", "TAGGENLINK", "CLUSTER" };

        private string[] WorkSheets;

        private readonly List<DataTable> CitectConfigDBF = new List<DataTable>();
        private readonly List<string> TagNoAddr = new List<string>();
        private readonly List<string> TagsNotAdded = new List<string>();
        private readonly Dictionary<string, List<string>> variableDataMap = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, List<string>> trendDataMap = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, List<string>> alarmDataMap = new Dictionary<string, List<string>>();

        private readonly Dictionary<string, ToAddEnum> TagsToAdd = new Dictionary<string, ToAddEnum>();
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DataSet DataMapDS = new DataSet();

        public DBFAutomation(string path = "")
        {
            WorkSheets =
               !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["WorkSheets"].ToString()) ?
                System.Configuration.ConfigurationManager.AppSettings["WorkSheets"].Split(new char[] { ',', '|', '.' }) : WorkSheetNames;

            path = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["MapDataPath"].ToString()) ?
                System.Configuration.ConfigurationManager.AppSettings["MapDataPath"] : "";

            Validate = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["DiagnosticsRequired"]);
            OpenDataMap(path);

            path = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["CitectUserPath"].ToString()) ?
                System.Configuration.ConfigurationManager.AppSettings["CitectUserPath"] : "";
            
            OpenCurrentDBFConfiguration(path);
        }

        public bool ExecuteGeneration()
        {
            ReadVariableDataMap();
            //ReadAlarmDataMap();
            //ReadTrendDataMap();
            return true;
        }

        /// <summary>
        /// open excel sheet
        /// </summary>
        /// <param name="filePath"></param>
        private void OpenDataMap(string filePath)
        {
            var configuredatatable = new ExcelDataTableConfiguration { UseHeaderRow = true, ReadHeaderRow = rowreader => rowreader.Read() };
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {

                        DataMapDS = reader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = (tableReader) => configuredatatable });
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("ReadDataMap", ex);
                ExcelError = true;
            }
            ExcelError = false;
        }

        /// <summary>
        /// open dbf variable,trend,digalarm
        /// </summary>
        /// <param name="pathname"></param>
        private void OpenCurrentDBFConfiguration(string pathname)
        {
            var dbfs = new List<string> { "Variable", "Digalm", "Trend" };
            dbfs.ForEach(s => CitectConfigDBF.Add(ReadDbf.GetTable(pathname, s)));
        }

        /// <summary>
        /// deprecated
        /// </summary>
        private void ReadTrendDataMap()
        {

            var DataMap = new Dictionary<string, List<string>>();
            foreach (DataTable dt in DataMapDS.Tables)
            {
                if (!WorkSheets.Contains(dt.TableName)) continue;

                var rowLines = new List<string>();
                foreach (DataRow row in dt.Rows) //row 1 is title, row 2 are fieldName in data map sheet
                {
                    //bypass tags not needed 
                    //var toAdd = (ToAddEnum)Enum.Parse(typeof(ToAddEnum), !row.IsNull("ToAdd") ? row["ToAdd"].ToString() : "0");
                    //if (toAdd == ToAddEnum.none || toAdd == ToAddEnum.varonly) continue;

                    if (row.IsNull("EXPR") || int.TryParse(row["EXPR"].ToString(), out int result)) continue;

                    if (TagsToAdd.ContainsKey(row["EXPR"].ToString()))
                    {
                        var toadd = TagsToAdd[row["EXPR"].ToString()];
                        if (toadd == ToAddEnum.none || toadd == ToAddEnum.varonly || toadd == ToAddEnum.varalarm) continue;
                    }

                    string rowline = string.Empty;
                    bool addToMap = false;
                    foreach (var fieldName in TrendFields)
                    {
                        if (fieldName.Equals("EXPR"))
                            addToMap = TagNoAddr.Contains(row["EXPR"].ToString()) ? false : true;


                        if (dt.Columns.Contains(fieldName))
                            rowline += row.IsNull(fieldName) ? "," : Regex.Replace(row[fieldName].ToString(), ",", "") + ",";
                        else
                            rowline += ",";
                    }
                    if (addToMap)

                        rowLines.Add(rowline);
                }

                DataMap.Add(dt.TableName, rowLines);
            }

            OutputToCSV("Trendcsv", TrendFields, DataMap);

        }

        /// <summary>
        /// deprecated
        /// </summary>
        private void ReadAlarmDataMap()
        {
            var DataMap = new Dictionary<string, List<string>>();
            foreach (DataTable dt in DataMapDS.Tables)
            {
                if (!WorkSheets.Contains(dt.TableName)) continue;

                var rowLines = new List<string>();
                foreach (DataRow row in dt.Rows) //row 1 is title, row 2 are fieldName in data map sheet
                {

                    if (row.IsNull("VAR_A") || int.TryParse(row["VAR_A"].ToString(), out int result)) continue;

                    if (TagsToAdd.ContainsKey(row["VAR_A"].ToString()))
                    {
                        var toadd = TagsToAdd[row["VAR_A"].ToString()];
                        if (toadd == ToAddEnum.none || toadd == ToAddEnum.varonly || toadd == ToAddEnum.vartrend) continue;
                    }

                    string rowline = string.Empty;
                    bool addToMap = false;
                    foreach (var fieldName in DigiAlmFields)
                    {
                        if (fieldName.Equals("VAR_A"))
                            addToMap = TagNoAddr.Contains(row["VAR_A"].ToString()) ? false : true;


                        if (dt.Columns.Contains(fieldName))
                            rowline += row.IsNull(fieldName) ? "," : Regex.Replace(row[fieldName].ToString(), ",", "") + ",";
                        else
                            rowline += ",";
                    }
                    if (addToMap)
                        rowLines.Add(rowline);
                }

                DataMap.Add(dt.TableName, rowLines);
            }

            OutputToCSV("DigiAlmcsv", DigiAlmFields, DataMap);

        }

        /// <summary>
        /// read map from excel tabs and process variables/alarms/trend
        /// </summary>
        private void ReadVariableDataMap()
        {
            foreach (DataTable dt in DataMapDS.Tables)
            {
                try
                {
                    //worksheet not required
                    if (!WorkSheets.Contains(dt.TableName)) continue;
                    var variableLines = new List<string>();
                    var alarmLines = new List<string>();
                    var trendLines = new List<string>();

                    foreach (DataRow row in dt.Rows) //row 1 is title, row 2 are fieldName in data map sheet
                    {
                        //bypass tags not needed 
                        var toAdd = (ToAddEnum)Enum.Parse(typeof(ToAddEnum), !row.IsNull("ToAdd") ? row["ToAdd"].ToString() : "0");
                        if (toAdd == ToAddEnum.none) continue;

                        if (row.IsNull("NAME") || int.TryParse(row["NAME"].ToString(), out int result)) continue;
                        var tagname = row["NAME"].ToString();

                        string rowline = string.Empty;
                        bool noaddr = false;
                        foreach (var fieldName in VariableFields)
                        {
                            if (dt.Columns.Contains(fieldName))
                            {
                                if (fieldName == "ADDR")
                                {
                                    noaddr = row.IsNull(fieldName) ? true : false;
                                    if (noaddr)
                                        TagNoAddr.Add(row["NAME"].ToString());
                                }

                                //noaddr = dt.Columns.Equals("ADDR") && row.IsNull(fieldName) ? true : false;
                                rowline += row.IsNull(fieldName) ? "," : Regex.Replace(row[fieldName].ToString(), ",", "") + ",";
                            }
                            else
                                rowline += ",";
                        }
                        if (noaddr) continue;

                        //check for duplicates
                        if (!TagsToAdd.ContainsKey(tagname))
                            TagsToAdd.Add(tagname, toAdd);
                        else
                        {
                            log.WarnFormat("possible duplicate check tag {0} in tab {1}", tagname, dt.TableName);
                            continue;
                        }

                        variableLines.Add(rowline.TrimEnd(new char[] { ',' }));

                        //alarms
                        if (toAdd == ToAddEnum.varalarm || toAdd == ToAddEnum.all)
                        {
                            rowline = string.Empty;
                            DigiAlmFields.ForEach(s => rowline += row.IsNull(s) ? "," : Regex.Replace(row[s].ToString(), ",", "") + ",");
                            if (!string.IsNullOrEmpty(rowline))
                                alarmLines.Add(rowline.TrimEnd(new char[] { ',' }));
                        }

                        //trends
                        if (toAdd == ToAddEnum.vartrend || toAdd == ToAddEnum.all)
                        {
                            rowline = string.Empty;
                            TrendFields.ForEach(s => rowline += row.IsNull(s) ? "," : Regex.Replace(row[s].ToString(), ",", "") + ",");
                            if (!string.IsNullOrEmpty(rowline))
                                trendLines.Add(rowline.TrimEnd(new char[] { ',' }));
                        }
                    }
                    TagNoAddr.ForEach(s => log.DebugFormat("Tags Missing Address: {0}", s));

                    variableDataMap.Add(dt.TableName, variableLines);
                    alarmDataMap.Add(dt.TableName, alarmLines);
                    trendDataMap.Add(dt.TableName, trendLines);
                }
                catch (Exception ex)
                {
                    log.Error("ReadVariableDataMap:", ex);
                }
            }

            OutputToCSV("VariableCsv", VariableFields, variableDataMap);
            OutputToCSV("DigAlmCsv", DigiAlmFields, alarmDataMap);
            OutputToCSV("TrendCsv", TrendFields, trendDataMap);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool PerformValidate()
        {
            bool result;

            result = ProcessValidation("Variable", variableDataMap);
            result = ProcessValidation("Digalm", alarmDataMap);
            result = ProcessValidation("Trend", trendDataMap);
            return result;
        }

        /// <summary>
        /// process validate
        /// </summary>
        /// <param name="dbfTable"></param>
        /// <param name="csvRowLines"></param>
        /// <returns></returns>
        private bool ProcessValidation(string dbfTable, Dictionary<string, List<string>> csvRowLines)
        {
            var resultdt = CitectConfigDBF.FirstOrDefault(s => s.TableName == dbfTable);
            if (resultdt == null)
                return false;

            var tagDiagnostics = new List<DBFTagDiagnostics>();
            
            foreach (DataRow row in resultdt.Rows)
            {          
                if (row.IsNull(0)) continue;

                var dbftag = row[0].ToString().Trim();
                string csvrowline = "";
                string tabName = "";

                foreach(var excelmaptab in csvRowLines)
                {
                    if (!excelmaptab.Value.Exists(s => s.Split(new char[] { ',' })[0] == dbftag))
                        continue;

                    tabName = excelmaptab.Key;
                    csvrowline = excelmaptab.Value.FirstOrDefault(s => s.Split(new char[] { ',' })[0] == dbftag);
                }

                if (!string.IsNullOrEmpty(csvrowline))
                {
                    var linedetails = csvrowline.Split(new char[] { ',' });
                    var unmatchedlist = new Dictionary<string, string>();
                    //check csv line against each dbf cell
                    for (int colIdx = 0; colIdx < linedetails.Length; colIdx++)
                    {
                        //ignore oid
                        if (resultdt.Columns[colIdx].ColumnName.Equals("oid")) continue;

                        if (row.IsNull(colIdx))
                        {
                            //col is null but result is not?
                            if (string.IsNullOrEmpty(linedetails[colIdx]))
                                continue;
                            else
                            {
                                unmatchedlist.Add(resultdt.Columns[colIdx].ColumnName, string.Format("empty,{0}", linedetails[colIdx]));
                                continue;
                            }
                        }

                        if (row[colIdx].ToString().Trim().Equals(linedetails[colIdx].Trim()))
                            continue;

                        //add to dbf field difference list                            
                        unmatchedlist.Add(resultdt.Columns[colIdx].ColumnName, string.Format("{0},{1}", string.IsNullOrEmpty(row[colIdx].ToString()) ? "empty" : row[colIdx].ToString().Trim(), linedetails[colIdx]));
                    }
                    if (unmatchedlist.Count > 0)
                        tagDiagnostics.Add(new DBFTagDiagnostics { ExcelTabName = tabName, TagName = dbftag, UnMatchedField = unmatchedlist });
                }
                else
                {
                    tagDiagnostics.Add(new DBFTagDiagnostics { ExcelTabName = "None", TagName = dbftag, UnMatchedField = new Dictionary<string, string>() });
                }               
            }

            OutputToDiagnosticCSV(dbfTable, tagDiagnostics);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbfType"></param>
        /// <param name="dBFTagDiagnostics"></param>
        private void OutputToDiagnosticCSV(string dbfType, IList<DBFTagDiagnostics> dBFTagDiagnostics)
        {
            var outputpath = System.Configuration.ConfigurationManager.AppSettings["OutputPath"];
            var path = outputpath + dbfType + "Diagnostics.csv";
            var csv = new LogCsv(path, true);

            csv.WriteToFile("tab,tagname,fieldname,old value,new value");
            foreach (var tagdiagnostic in dBFTagDiagnostics)
            {
                if (!tagdiagnostic.UnMatchedField.Any())
                {
                    csv.WriteToFile("{0},{1},,", tagdiagnostic.ExcelTabName, tagdiagnostic.TagName);
                    continue;
                }
                foreach (var fieldDiag in tagdiagnostic.UnMatchedField)
                    csv.WriteToFile("{0},{1},{2},{3}", tagdiagnostic.ExcelTabName, tagdiagnostic.TagName, fieldDiag.Key, fieldDiag.Value);
            }
            csv.CloseFile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbfType"></param>
        /// <param name="dbfFields"></param>
        /// <param name="DataMap"></param>
        private void OutputToCSV(string dbfType, List<string> dbfFields, Dictionary<string, List<string>> DataMap)
        {
            var outputpath = System.Configuration.ConfigurationManager.AppSettings["OutputPath"];
            var path = outputpath + dbfType + ".csv";
            var csv = new LogCsv(path, true);

            var newFieldList = dbfFields.Select(s => Regex.Replace(s, "\n|\t|\r", "")).ToList();

            csv.WriteToFile("{0}", string.Join(",", newFieldList));

            foreach (var kvp in DataMap)
            {
                log.DebugFormat("writing {0} tags for {1} with {2} tags", dbfType, kvp.Key, kvp.Value.Count);
                kvp.Value.ForEach(s => csv.WriteToFile("{0}", s));
            }
            csv.CloseFile();
        }
    }

    /// <summary>
    /// Store tag diagnostic info needed for dbf automation validation
    /// </summary>
    public class DBFTagDiagnostics
    {
        public string ExcelTabName { get; set; }
        public string TagName { get; set; }
        public Dictionary<string, string> UnMatchedField { get; set; }
    }
}
