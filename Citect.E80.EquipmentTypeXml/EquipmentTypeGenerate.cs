using ExcelDataReader;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Citect.E80.EquipmentTypeXml
{
    /// <summary>
    /// Generate EquipmentType xml template from Excel File 
    /// XML should have the following sections as per 
    /// file:///C:/Program%20Files%20(x86)/Schneider%20Electric/Citect%20SCADA%202016/Bin/Help/Citect%20SCADA/Default.htm#Equipment_XML_Template.html
    /// header
    /// parameters
    /// input
    /// outputs
    /// footer
    /// </summary>
    public class EquipmentTypeGenerate
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private List<DataSet> CitectTagsDataSet;
        private readonly List<template> EquipmentTypeTemplates;
        //read from excel
        //convert to xml equipment type?
        //special cases?
        public EquipmentTypeGenerate()
        {
            CitectTagsDataSet = new List<DataSet>();
            EquipmentTypeTemplates = new List<template>();
            GetExcelTemplateFromDirectory();
        }

        /// <summary>
        /// get excel template from specified directory
        /// </summary>
        private void GetExcelTemplateFromDirectory()
        {
            var path = System.Configuration.ConfigurationManager.AppSettings["EqTypePath"];
            var directoryInfo = new DirectoryInfo(path);
            foreach (var fileinfo in directoryInfo.GetFiles())
                GetCitectTagsFromExcel(fileinfo.FullName);
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetCitectTagsFromExcel(string filePath)
        {
            var configuredatatable = new ExcelDataTableConfiguration { UseHeaderRow = true };
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {

                        CitectTagsDataSet.Add(reader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = (tableReader) => configuredatatable }));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetCitectTagsFromExcel", ex);
            }
        }

        public bool Execute_SetupTags()
        {
            if (!CitectTagsDataSet.Any())
                return false;


            CitectTagsDataSet.ForEach(s => ConvertTagsInExcelToTemplate(s));
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ConvertTagsInExcelToTemplate(DataSet ds)
        {
            foreach (DataTable dtTable in ds.Tables)
            {
                var equipmentTypeTemplate = new template { desc = dtTable.TableName }; //name of worksheet - equipment type name
                equipmentTypeTemplate.param = TomPriceEquipmentTemplate.GetEquipmentType_Param(dtTable.TableName, "");
                equipmentTypeTemplate.input = TomPriceEquipmentTemplate.GetEquipmentType_Input();
                var templateOutputs = new List<templateOutput>();
                //get baseaddress?
                var baseAddressList = new Dictionary<string, int>();
                foreach (DataRow row in dtTable.AsEnumerable().Take(3))
                    baseAddressList.Add(row["Tag Name"].ToString(), int.Parse(row["Address"].ToString()));

                //put the information into class
                var outputparams = new List<EquipmentTypeOutputParam>();
                string equipment = "";
                foreach (DataRow row in dtTable.AsEnumerable().Skip(3))
                {
                    //inner exception
                    if (row.IsNull("Tag Name")) continue;
                    string tagName = row["Tag Name"].ToString();
                    equipment = row.IsNull("Equipment") ? equipment : row["Equipment"].ToString();
                    var nameSplit = tagName.Split(new char[] { '_', '.' });

                    try
                    {
                        int.TryParse(Regex.Match(row["Address"].ToString(), @"\d+").Value, out int plcNumericAddress);
                        var outputparam = new EquipmentTypeOutputParam
                        {
                            EquipName = dtTable.TableName.Replace(" ", string.Empty),
                            Comment = row["Comment"].ToString(),
                            DataType = row["Data Type"].ToString(),
                            Suffix = tagName.Substring(tagName.IndexOf(equipment) + equipment.Length).TrimStart(new char[] { '_', '.', ',', '|' }),  //suffix is anything after equipment name string
                            Prefix = nameSplit[0],
                            BaseAddrPairs = baseAddressList,
                            BaseAddressParam = GetBaseAddrParam(nameSplit[0]),
                            TagAddress = plcNumericAddress,
                            AlmCategory = row.IsNull("Category") ? "" : row["Category"].ToString(),
                            RawZero = row.IsNull("Raw Zero Scale") ? "0" : row["Raw Zero Scale"].ToString(),
                            RawFull = row.IsNull("Raw Full Scale") ? "0" : row["Raw Full Scale"].ToString(),
                            EngZero = row.IsNull("Eng Zero Scale") ? "0" : row["Eng Zero Scale"].ToString(),
                            EngFull = row.IsNull("Eng Full Scale") ? "0" : row["Eng Full Scale"].ToString(),
                            SetTrends = row.IsNull("Set Trend") ? false : bool.Parse(row["Set Trend"].ToString()),
                            Units = row.IsNull("Eng Units") ? string.Empty : row["Eng Units"].ToString(),
                            Format = row.IsNull("Format") ? string.Empty : row["Format"].ToString()
                        };

                        if (!row.IsNull(@"I/O Device"))
                        {
                            if (row["I/O Device"].ToString().Equals("CICODESVR"))
                            {
                                outputparam.DeviceIO = row["I/O Device"].ToString();
                                outputparam.FuncName = row["Address"].ToString();
                                outputparam.IsCalCulated = true;
                            }
                            else
                            {
                                outputparam.DeviceIO = row["I/O Device"].ToString();
                            }
                        }                        

                        outputparams.Add(outputparam);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("ConvertTagToTemplate:{0},{1}", tagName, ex);
                    }
                }

                //find duplicates in list?
                var result = outputparams.GroupBy(x => x.Suffix.TrimStart(new char[] { '_', '.', ',' })).Select(y => new { suffix = y.Key, count = y.Count() });
                foreach (var rec in result.Where(a => a.count > 1))
                    log.WarnFormat("Please correct tag list: possible duplication of tags, suffux: {0} for {1}", rec.suffix, dtTable.TableName);

                //get xml formatted string from outputparams
                outputparams.ForEach(s => templateOutputs.AddRange(s.GetXmlTemplateOutput()));
                if (templateOutputs.Count > 0)
                    equipmentTypeTemplate.output = templateOutputs.ToArray();

                //add to list
                EquipmentTypeTemplates.Add(equipmentTypeTemplate);

            }
            //serialise to XML
            EquipmentTypeTemplates.ForEach(s => SerializeEquipmentTypeTemplate(s, s.desc));

            return EquipmentTypeTemplates.Count > 0;
        }

        private BaseAddr GetBaseAddrParam(string prefix)
        {
            BaseAddr baseAddr;
            switch (prefix)
            {
                case "S":
                    baseAddr = BaseAddr.Status;
                    break;
                case "A":
                    baseAddr = BaseAddr.Alarm;
                    break;
                case "N":
                    baseAddr = BaseAddr.Analog;
                    break;
                default:
                    baseAddr = BaseAddr.Invalid;
                    break;
            }
            return baseAddr;
        }

        /// <summary>
        /// Serialise to XML.
        /// </summary>
        /// <returns></returns>
        private bool SerializeEquipmentTypeTemplate(template template, string equipmenttype)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            //foreach output to xml .output template to xml.
            if (template == null) return false;

            XmlSerializer xs = new XmlSerializer(typeof(template));
            using (var txtwriter = new StreamWriter(@"C:\CodeTest\EquipmentTypes\Output\" + equipmenttype + ".xml", false, Encoding.UTF8))
            {
                xs.Serialize(txtwriter, template, ns);
            }
            return true;
        }
    }
}
