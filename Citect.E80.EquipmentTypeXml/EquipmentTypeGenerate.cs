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

        private DataSet CitectTags;
        private readonly List<template> EquipmentTypeTemplates;        
        //read from excel
        //convert to xml equipment type?
        //special cases?
        public EquipmentTypeGenerate()
        {
            CitectTags = new DataSet();
            EquipmentTypeTemplates = new List<template>();            
            GetExcelTemplateFromDirectory();
        }

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
                        CitectTags = reader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = (tableReader) => configuredatatable });
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetCitectTagsFromExcel", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ConvertTagsInExcelToTemplate()
        {
            foreach (DataTable dtTable in CitectTags.Tables)
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
                foreach (DataRow row in dtTable.AsEnumerable().Skip(3))
                {
                    //inner exception
                    if (row.IsNull("Tag Name")) continue;
                    string tagName = row["Tag Name"].ToString();
                    var nameSplit = tagName.Split(new char[] { '_', '.' });                    
                    try
                    {
                        int plcNumericAddress = int.Parse(Regex.Match(row["Address"].ToString(), @"\d+").Value);

                        outputparams.Add(new EquipmentTypeOutputParam
                        {
                            EquipName = dtTable.TableName.Replace(" ", string.Empty),
                            Comment = row["Comment"].ToString(),
                            DataType = row["Data Type"].ToString(),
                            Suffix = nameSplit.Length > 3 ? string.Join("_", nameSplit, 3, nameSplit.Length - 3) : nameSplit[nameSplit.Length - 1],
                            Prefix = nameSplit[0],
                            BaseAddrPairs = baseAddressList,
                            BaseAddressParam = GetBaseAddrParam(nameSplit[0]),
                            TagAddress = plcNumericAddress,
                            RawZero = row.IsNull("Raw Zero Scale") ? "0" : row["Raw Zero Scale"].ToString(),
                            RawFull = row.IsNull("Raw Full Scale") ? "0" : row["Raw Full Scale"].ToString(),
                            EngZero = row.IsNull("Eng Zero Scale") ? "0" : row["Eng Zero Scale"].ToString(),
                            EngFull = row.IsNull("Eng Full Scale") ? "0" : row["Eng Full Scale"].ToString(),
                            SetTrends = row.IsNull("Set Trend") ? false : bool.Parse(row["Set Trend"].ToString()),
                            Units = row.IsNull("Eng Units") ? string.Empty : row["Eng Units"].ToString(),
                            Format = row.IsNull("Format") ? string.Empty : row["Format"].ToString()
                        });
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("ConvertTagToTemplate:{0},{1}", tagName, ex);
                    }
                }

                outputparams.ForEach(s => templateOutputs.AddRange(s.GetTemplateOutput()));

                if (templateOutputs.Count > 0)
                    equipmentTypeTemplate.output = templateOutputs.ToArray();

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
            using (var txtwriter = new StreamWriter(@"C:\CodeTest\EquipmentTypes\Output\" + equipmenttype + ".xml" , false, Encoding.UTF8))
            {
                xs.Serialize(txtwriter, template, ns);
            }
            return true;
        }
    }
}
