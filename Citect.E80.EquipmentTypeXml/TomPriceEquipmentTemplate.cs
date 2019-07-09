using System.Collections.Generic;
using log4net;

namespace Citect.E80.EquipmentTypeXml
{
    /// <summary>
    /// defines the xml objects (input,output,param) required for Tom Price Equipment Types
    /// </summary>
    public class TomPriceEquipmentTemplate
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string editcode = "3939343";        

        /// <summary>
        /// Define Input section of Equipment Type xml template
        /// </summary>
        /// <param name="dbfname"></param>
        /// <returns></returns>
        public static templateInput GetEquipmentType_Input()
        {
            var inputFields = new List<string> { "NAME", "CLUSTER", "TYPE", "AREA", "LOCATION", "COMMENT", "IODEVICE", "TAGPREFIX", "PARAM" };
            var equipmentTypeInput = new templateInput { name = "equipment", file = "equip.dbf", desc = "Equipment Database" };
            var equipmentTypeInputFields = new List<templateInputField>();
            inputFields.ForEach(s => equipmentTypeInputFields.Add(new templateInputField { name = s }));
            equipmentTypeInput.field = equipmentTypeInputFields.ToArray();

            //add array specifics
            var inputArray = new List<templateInputArray>{
                new templateInputArray { name = "param_list", Value = "{ToProperty('{param}','=',';')}" },
                new templateInputArray {name = "BaseAddr", Value ="{ToProperty('{equipment.param_list[BaseAddr]}',':',',')}"}
            };

            equipmentTypeInput.array = inputArray.ToArray();

            return equipmentTypeInput;
        }

        /// <summary>
        /// define output section of equipment type for Calculated Variables
        /// 1. ensure the cicode unit is first defined in i/o devices
        /// 2. ensure cicode function (FuncName) is defined in cicode
        /// </summary>
        /// <param name="fieldParam"></param>
        /// <returns></returns>
        public static templateOutput GetEquipmentType_CalculatedCommandVariable(EquipmentTypeOutputParam fieldParam)
        {
            templateOutput templateOutput = new templateOutput { name = string.Format("Var.{0}Cmd", fieldParam.Suffix.TrimStart(new char[] { ',', '_', '.' })), file = "variable.dbf", filter = @"'{equipment.type}={type.name}'" };

            var funcArgs = string.Format("C_{{equipment.tagprefix}}{0}", fieldParam.Suffix);
            var tagaddressvalue = string.Format("{0}({1})", fieldParam.FuncName, funcArgs);

            var outputFields = new List<templateOutputField>
            {
                new templateOutputField { name = "name", key=true, Value = string.Format("{{equipment.tagprefix}}{0}Cmd",fieldParam.Suffix), keySpecified=true},
                new templateOutputField { name = "type", Value = fieldParam.DataType },
                new templateOutputField { name = "unit", Value = fieldParam.DeviceIO },                
                new templateOutputField { name = "comment", Value = "{equipment.tagprefix} " + fieldParam.Comment },   
                new templateOutputField { name = "equip", Value = fieldParam.Equipment },
                new templateOutputField { name= "addr", Value = tagaddressvalue},
                new templateOutputField { name = "item", Value = fieldParam.Suffix.TrimStart(new char[] {',','_','.'}) + "Cmd"},
                new templateOutputField { name = "taggenlink", load = true, Value = fieldParam.TagGenLink+"Cmd" , loadSpecified=true },
                new templateOutputField { name = "linked", Value = "1"},                
                new templateOutputField { name = "editcode", Value = editcode }
            };

            templateOutput.field = outputFields.ToArray();
            return templateOutput;
        }


        /// <summary>
        /// Define Output section of Equipment Type xml template (digital variables)
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <param name="outputType"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static templateOutput GetEquipmentType_VarDiscreteOutputs(EquipmentTypeOutputParam fieldParam)
        {
            templateOutput templateOutput = new templateOutput { name = "Var." + fieldParam.Suffix.TrimStart(new char[] { ',', '_', '.' }), file = "variable.dbf", filter = @"'{equipment.type}={type.name}'" };
            var tagaddressvalue = "{equipment.BaseAddr[" + fieldParam.BaseAddressParam.ToString() + "]} + " + fieldParam.AddressOffset.ToString();
            var templateOutputCalculator = new templateOutputCalculator { name = "TagAddress", Value = tagaddressvalue };

            var outputFields = new List<templateOutputField>
            {

                new templateOutputField { name = "name", key=true, Value = string.Format("{0}_{{equipment.tagprefix}}{1}",fieldParam.Prefix,fieldParam.Suffix), keySpecified=true},
                new templateOutputField { name = "type", Value = fieldParam.DataType },
                new templateOutputField { name = "unit", Value = fieldParam.DeviceIO },
                new templateOutputField { name = "comment", Value = "{equipment.tagprefix} " + fieldParam.Comment },
                new templateOutputField { name = "cluster", key = true, Value = fieldParam.Cluster, keySpecified=true },
                new templateOutputField { name = "equip", Value = fieldParam.Equipment },
                new templateOutputField { name = "addr", Value = "%M{TagAddress}" },
                new templateOutputField { name = "item", Value = CapsFirstLetter(fieldParam.Suffix.TrimStart(new char[] {',','_','.'}).ToLower() )},
                new templateOutputField { name = "taggenlink", load = true, Value = fieldParam.TagGenLink , loadSpecified=true },
                new templateOutputField { name = "linked", Value = "1"},
                new templateOutputField { name = "editcode", Value = editcode },
            };
            templateOutput.calculator = templateOutputCalculator;
            templateOutput.field = outputFields.ToArray();
            return templateOutput;
        }

        /// <summary>
        /// Define Output section of Equipment Type xml template (analog variables)
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <param name="outputType"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static templateOutput GetEquipmentType_VarAnalogOutputs(EquipmentTypeOutputParam fieldParam)
        {
            templateOutput templateOutput = new templateOutput { name = "Var." + fieldParam.Suffix.TrimStart(new char[] { ',', '_', '.' }), file = "variable.dbf", filter = @"'{equipment.type}={type.name}'" };
            var tagaddressvalue = "{equipment.BaseAddr[" + fieldParam.BaseAddressParam.ToString() + "]} + " + fieldParam.AddressOffset.ToString();
            var templateOutputCalculator = new templateOutputCalculator { name = "TagAddress", Value = tagaddressvalue };
            var outputFields = new List<templateOutputField>
            {
                new templateOutputField { name = "name", key=true, Value =string.Format("{0}_{{equipment.tagprefix}}{1}",fieldParam.Prefix,fieldParam.Suffix) , keySpecified = true },
                new templateOutputField { name = "type", Value = fieldParam.DataType },
                new templateOutputField { name = "unit", Value = fieldParam.DeviceIO },
                new templateOutputField { name = "eng_zero", Value = fieldParam.EngZero },
                new templateOutputField { name = "eng_full", Value = fieldParam.EngFull},
                new templateOutputField { name = "comment", Value = "{equipment.tagprefix} " + fieldParam.Comment },
                new templateOutputField { name = "cluster", key = true, Value = fieldParam.Cluster , keySpecified = true},
                new templateOutputField { name = "equip", Value = fieldParam.Equipment },
                new templateOutputField { name = "addr", Value = "%MW{TagAddress}" },
                new templateOutputField { name = "raw_zero", Value = fieldParam.RawZero },
                new templateOutputField { name = "raw_full", Value = fieldParam.RawFull },
                new templateOutputField { name = "eng_units", Value = fieldParam.Units },
                new templateOutputField { name = "format", Value = fieldParam.Format },
                new templateOutputField { name = "item", Value = CapsFirstLetter( fieldParam.Suffix.TrimStart(new char[] {',','_','.'}).ToLower()) },
                new templateOutputField { name = "taggenlink", load = true, Value = fieldParam.TagGenLink, loadSpecified = true },
                new templateOutputField { name = "linked", Value = "1"},
                new templateOutputField { name = "editcode", Value = editcode },
            };
            templateOutput.calculator = templateOutputCalculator;
            templateOutput.field = outputFields.ToArray();
            return templateOutput;
        }



        /// <summary>
        /// Define Output section of Equipment Type xml template (digalm)
        /// </summary>
        /// <param name="equipmenttype"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static templateOutput GetEquipmentType_DigAlmOutputs(EquipmentTypeOutputParam fieldParam)
        {
            templateOutput templateOutput = new templateOutput { name = "Alm." + fieldParam.Suffix.TrimStart(new char[] { ',', '_', '.' }), file = "digalm.dbf", filter = @"'{equipment.type}={type.name}'" };
            var outputFields = new List<templateOutputField>
            {

                new templateOutputField { name = "tag", key=true,Value = string.Format("{0}_{{equipment.tagprefix}}{1}_Alm",fieldParam.Prefix,fieldParam.Suffix), keySpecified = true},
                new templateOutputField { name = "name", Value = fieldParam.Equipment},
                new templateOutputField { name = "desc", Value = fieldParam.Comment},
                new templateOutputField { name = "VAR_A", Value = string.Format("{0}_{{equipment.tagprefix}}{1}",fieldParam.Prefix,fieldParam.Suffix) },
                new templateOutputField{ name= "comment", Value=""},
                new templateOutputField { name = "cluster", key= true, Value = fieldParam.Cluster, keySpecified = true },
                new templateOutputField{ name= "category", Value=""},
                new templateOutputField { name = "equip", Value = fieldParam.Equipment },
                new templateOutputField { name = "item", Value = CapsFirstLetter( fieldParam.Suffix.TrimStart(new char[] {',','_','.'}).ToLower()) },
                new templateOutputField { name = "taggenlink", load = true, Value= fieldParam.TagGenLink, loadSpecified = true },
                new templateOutputField { name = "linked", Value = "1"},
                new templateOutputField { name = "editcode", Value = "9" },
            };
            templateOutput.field = outputFields.ToArray();
            return templateOutput;
        }

        /// <summary>
        /// Define output section of equipmet type template (trends)
        /// </summary>
        /// <param name="fieldParam"></param>
        /// <returns></returns>
        public static templateOutput GetEquipmentType_TrnOutputs(EquipmentTypeOutputParam fieldParam)
        {
            templateOutput templateOutput = new templateOutput { name = "TRN." + fieldParam.Suffix.TrimStart(new char[] { ',', '_', '.' }), file = "trend.dbf", filter = @"'{equipment.type}={type.name}'" };
            var outputFields = new List<templateOutputField>
            {
                new templateOutputField { name = "name", key=true, Value = string.Format("{0}_{{equipment.tagprefix}}{1}_Trn",fieldParam.Prefix,fieldParam.Suffix), keySpecified= true},
                new templateOutputField { name = "expr", Value = string.Format("{0}_{{equipment.tagprefix}}{1}",fieldParam.Prefix,fieldParam.Suffix)},
                new templateOutputField {name="sampleper",Value=""},
                new templateOutputField{ name= "files", Value=""},
                new templateOutputField{ name= "time", Value=""},
                new templateOutputField{ name= "period", Value=""},
                new templateOutputField{ name= "comment", Value=fieldParam.Comment + " trend" },
                new templateOutputField { name="type", Value = "TRN_PERIODIC"},
                new templateOutputField {name = "stormethod", Value = "Floating Point (8-byte samples)"},
                new templateOutputField { name = "cluster", key= true, Value = fieldParam.Cluster, keySpecified = true },
                new templateOutputField { name = "equip", Value = fieldParam.Equipment },
                new templateOutputField { name = "item", Value = CapsFirstLetter( fieldParam.Suffix.TrimStart(new char[] {',','_','.'}).ToLower() )},
                new templateOutputField { name = "taggenlink", load = true, Value= fieldParam.TagGenLink, loadSpecified = true },
                new templateOutputField { name = "linked", Value = "1"},
                new templateOutputField { name = "editcode", Value = "9" },
            };
            templateOutput.field = outputFields.ToArray();
            return templateOutput;
        }

        /// <summary>
        /// Define Parameters section of Equipment Type XML.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="refName"></param>
        /// <returns></returns>
        public static templateParam GetEquipmentType_Param(string name, string refName)
        {
            templateParam equipmentTypeParam = new templateParam { name = "type" };
            var paramstring = new List<templateParamString>
            {
                new templateParamString{ name = "name",  Value = name},
                new templateParamString{ name = "ref",  Value = ""},
                new templateParamString{name="parameter-definitions", Value = "BaseAddr.Status=;BaseAddr.Alarm=;BaseAddr.Analog=;Command.AUTO=;"}
            };
            equipmentTypeParam.@string = paramstring.ToArray();
            return equipmentTypeParam;
        }


        private static string CapsFirstLetter(string value)
        {
            var result = "";
            try
            {
                result = char.ToUpper(value[0]) + value.Substring(1);
                return result;
            }
            catch (System.Exception ex)
            {
                log.Error("CapsFirstLetter()" + value, ex);
                result = value;
            }
            return result;
        }
    }
}
