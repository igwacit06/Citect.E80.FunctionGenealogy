using log4net;
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
    /// <summary>
    /// function: to output excel data into csv format
    /// 
    /// </summary>
    public class DBFAutomation
    {
        private readonly string[] WorkSheetNames = new string[] { "Gens", "DMC Alarms", "Load Demand", "LV&Tx CBs", "Gen Annunciator", "Gas Gens", "Mill Starting", "BOP", "Q101-8 CBs", "Tariff Meters", "Dsl Flow", "Air Compressor", "751 PRs", "700G PRs", "Gas Skid", "HMI Cmds", "Misc Analogs", "Batt Chargers", "VSDs", "Command Statuses", "Gas Flow Meters", "FIP" };
        private readonly List<string> TagNoAddr = new List<string>();
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DataSet DataMapDS = new DataSet();

        public DBFAutomation(string path = "")
        {
            var worksheetnames =
               !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["WorkSheets"].ToString()) ?
                System.Configuration.ConfigurationManager.AppSettings["WorkSheets"].Split(new char[] { ',', '|', '.' }) : WorkSheetNames;

            path = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["MapDataPath"].ToString()) ?
                System.Configuration.ConfigurationManager.AppSettings["MapDataPath"] : "";

            OpenDataMap(path);
            ReadVariableDataMap(worksheetnames);
            ReadAlarmDataMap(worksheetnames);
            ReadTrendDataMap(worksheetnames);
        }

        private bool OpenDataMap(string filePath)
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
            }
            return false;
        }

        //get all variable,digital,trend data from each table
        private void ReadTrendDataMap(string[] worksheets)
        {
            var TrendFields = new List<string>() { "NAME_2", "EXPR", "TRIG", "SAMPLEPERMANUAL", "PRIV_1", "AREA_1", "ENG_UNITS_1", "FORMAT", "FILENAME", "FILES", "TIME", "PERIOD", "COMMENT_2", "TYPE", "SPCFLAG", "LSL", "USL", "SUBGRPSIZE", "XDOUBLEBAR", "RANGE", "SDEVIATION", "STORMETHOD", "CLUSTER_2" };

            var DataMap = new Dictionary<string, List<string>>();
            foreach (DataTable dt in DataMapDS.Tables)
            {
                if (!worksheets.Contains(dt.TableName)) continue;

                var rowLines = new List<string>();
                foreach (DataRow row in dt.AsEnumerable()) //row 1 is title, row 2 are fieldName in data map sheet
                {
                    if (row.IsNull("TAG") || int.TryParse(row["TAG"].ToString(), out int result)) continue;
                    if (row.IsNull("CLUSTER_2")) continue;

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

        //get all variable,digital,trend data from each table
        private void ReadAlarmDataMap(string[] worksheets)
        {
            var DigiAlmFields = new List<string>() { "TAG", "NAME_1", "DESC", "VAR_A", "VAR_B", "CATEGORY", "HELP", "PRIV", "AREA", "COMMENT_1", "SEQUENCE", "DELAY", "CUSTOM1", "CUSTOM2", "CUSTOM3", "CUSTOM4", "CUSTOM5", "CUSTOM6", "CUSTOM7", "CUSTOM8", "CLUSTER_1" };

            var DataMap = new Dictionary<string, List<string>>();
            foreach (DataTable dt in DataMapDS.Tables)
            {
                if (!worksheets.Contains(dt.TableName)) continue;

                var rowLines = new List<string>();
                foreach (DataRow row in dt.AsEnumerable()) //row 1 is title, row 2 are fieldName in data map sheet
                {
                    if (row.IsNull("TAG") || int.TryParse(row["TAG"].ToString(), out int result)) continue;
                    if (row.IsNull("CLUSTER_1")) continue;

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

        //get all variable,digital,trend data from each table
        private void ReadVariableDataMap(string[] worksheets)
        {
            var VariableFields = new List<string>() { "NAME", "TYPE", "UNIT_2", "ADDR", "RAW_ZERO\nMANUAL", "RAW_FULL\nMANUAL", "ENG_ZERO\nMANUAL", "ENG_FULL\nMANUAL", "ENG_UNITS", "FORMAT\nMANUAL", "COMMENT", "EDITCODE", "LINKED", "OID", "REF1", "REF2", "DEADBAND", "CUSTOM", "TAGGENLINK", "CLUSTER" };

            var DataMap = new Dictionary<string, List<string>>();
            foreach (DataTable dt in DataMapDS.Tables)
            {
                try
                {

                    if (!worksheets.Contains(dt.TableName)) continue;

                    var rowLines = new List<string>();
                    foreach (DataRow row in dt.AsEnumerable()) //row 1 is title, row 2 are fieldName in data map sheet
                    {
                        if (row.IsNull("NAME") || int.TryParse(row["NAME"].ToString(), out int result)) continue;
                        if (row.IsNull("CLUSTER")) continue;

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

                        rowLines.Add(rowline);
                    }

                    DataMap.Add(dt.TableName, rowLines);
                    TagNoAddr.ForEach(s => log.DebugFormat("Tags Missing Address: {0}", s));
                }
                catch (Exception ex)
                {
                    log.Error("ReadVariableDataMap:", ex);
                }

            }

            OutputToCSV("VariableCsv", VariableFields, DataMap);
        }


        //gets all variable data and push to csv
        public void OutputToCSV(string dbfType, List<string> dbfFields, Dictionary<string, List<string>> DataMap)
        {
            var outputpath = System.Configuration.ConfigurationManager.AppSettings["OutputPath"];
            var path = outputpath + dbfType + ".csv";
            var csv = new LogCsv(path, true);

            var newFieldList = dbfFields.Select(s => Regex.Replace(s, "\n|\t|\r", "")).ToList();

            csv.WriteToFile("{0}", string.Join(",", newFieldList));
            var currsheet = "";
            foreach (var kvp in DataMap)
            {
                if (kvp.Key != currsheet)
                {
                    log.DebugFormat("writing {0} tags for {1} with {2} tags", dbfType, currsheet, kvp.Value.Count);
                    currsheet = kvp.Key;
                }
                kvp.Value.ForEach(s => csv.WriteToFile("{0}", s));
            }
            csv.CloseFile();
        }
    }
}
