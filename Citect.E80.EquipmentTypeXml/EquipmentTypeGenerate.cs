using ExcelDataReader;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

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
        private readonly string FilePath;
        //read from excel
        //convert to xml equipment type?
        //special cases?
        public EquipmentTypeGenerate(string path)
        {
            CitectTags = new DataSet();
            EquipmentTypeTemplates = new List<template>();
            FilePath = path;
            GetCitectTagsFromExcel();
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetCitectTagsFromExcel()
        {
            var configuredatatable = new ExcelDataTableConfiguration { UseHeaderRow = true };
            try
            {
                using (var stream = File.Open(FilePath, FileMode.Open, FileAccess.Read))
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
        private void ConvertTagToTemplate()
        {            
            foreach (DataTable dtTable in CitectTags.Tables)
            {
                try
                {
                    var equipmentTypeTemplate = new template { desc = dtTable.TableName }; //name of worksheet - equipment type name
                    equipmentTypeTemplate.param = TomPriceEquipmentTemplate.GetEquipmentType_Param(dtTable.TableName, "");
                    equipmentTypeTemplate.input = TomPriceEquipmentTemplate.GetEquipmentType_Input(dtTable.TableName);
                    var templateOutputs = new List<templateOutput>();
                    
                    foreach (DataRow row in dtTable.Rows)
                    {
                        string tagName;
                        string suffix;
                        string dataType;
                        string comment;
                        string prefix;
                        int baseaddress = 0;    //to calculate the base address?
                        if (row.IsNull("Tag Name")) continue;

                        tagName = row["Tag Name"].ToString();
                        comment = row["Comment"].ToString();
                        dataType = row["Data Type"].ToString();

                        suffix = tagName.Substring(tagName.LastIndexOf("_"), tagName.Length - (tagName.LastIndexOf("_") + 1));
                        prefix = tagName.Substring(0, tagName.IndexOf("_"));
                        templateOutputs.Add(TomPriceEquipmentTemplate.GetEquipmentType_VarOutputs("", "", "", "", ""));

                        //can expand to TRN,Analog alarms etc
                        if (prefix.Equals("A"))
                            templateOutputs.Add(TomPriceEquipmentTemplate.GetEquipmentType_DigAlmOutputs(dtTable.TableName, suffix));
                       
                    }
                    if (templateOutputs.Count > 0)
                        equipmentTypeTemplate.output = templateOutputs.ToArray();

                    EquipmentTypeTemplates.Add(equipmentTypeTemplate);
                }
                catch (Exception ex)
                {
                    log.Error("ConvertTagToTemplate:", ex);
                }
            }
            //serialise to XML
            EquipmentTypeTemplates.ForEach(s => SerializeEquipmentTypeTemplate(s, s.desc));
        }

        /// <summary>
        /// Serialise to XML.
        /// </summary>
        /// <returns></returns>
        public bool SerializeEquipmentTypeTemplate(template template, string equipmenttype)
        {
            //foreach output to xml .output template to xml.
            try
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(template));                
                var xml = "";

                using (var sww = new StreamWriter(@"C:\CodeTest\EquipmentTypes\" + equipmenttype + ".xml"))
                {
                    using (var writer = XmlWriter.Create(sww))
                    {
                        xsSubmit.Serialize(writer, template);
                        xml = sww.ToString(); // Your XML
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("SerializeEquipmentTypeTemplate:", ex);
            }
            return false;            
        }
    }
}
