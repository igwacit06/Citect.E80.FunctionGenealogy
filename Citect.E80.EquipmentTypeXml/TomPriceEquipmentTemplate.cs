using System.Collections.Generic;

namespace Citect.E80.EquipmentTypeXml
{
    public class TomPriceEquipmentTemplate
    {
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
            var inputArray = new List<templateInputArray> {
            new templateInputArray { name = "param_list", Value = "{ToProperty('{param}','=',';')}" },
            new templateInputArray {name = "BaseAddr", Value ="{ToProperty('{equipment.param_list[BaseAddr]}',':',',')}"}
            };

            equipmentTypeInput.array = inputArray.ToArray();

            return equipmentTypeInput;
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
            templateOutput templateOutput = new templateOutput { name = "Var." + fieldParam.Suffix, file = "variable.dbf", filter = @"'{equipment.type}={type.name}'" };
            var tagaddressvalue = "{equipment.BaseAddr[" + fieldParam.BaseAddressParam.ToString() + "]} + " + fieldParam.AddressOffset.ToString();
            var templateOutputCalculator = new templateOutputCalculator { name = "TagAddress", Value = tagaddressvalue };

            var outputFields = new List<templateOutputField>
            {

                new templateOutputField { name = "name", key=true, Value = fieldParam.Prefix +"_{equipment.tagprefix}_" + fieldParam.Suffix , keySpecified=true},
                new templateOutputField { name = "type", Value = fieldParam.DataType },
                new templateOutputField { name = "unit", Value = "{equipment.IODEVICE}" },
                new templateOutputField { name = "comment", Value = "{equipment.tagprefix} " + fieldParam.Comment },
                new templateOutputField { name = "cluster", key = true, Value = "{equipment.CLUSTER}", keySpecified=true },
                new templateOutputField { name = "equip", Value = "{equipment.name}" },
                new templateOutputField { name = "addr", Value = "%M{TagAddress}" },
                new templateOutputField { name = "item", Value = fieldParam.Suffix.ToLower() },
                new templateOutputField { name = "taggenlink", load = true, Value = fieldParam.TagGenLink , loadSpecified=true },
                new templateOutputField { name = "linked", Value = "1"},
                new templateOutputField { name = "editcode", Value = "11" },
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
            templateOutput templateOutput = new templateOutput { name = "Var." + fieldParam.Suffix, file = "variable.dbf", filter = @"'{equipment.type}={type.name}'" };
            var tagaddressvalue = "{equipment.BaseAddr[" + fieldParam.BaseAddressParam.ToString() + "]} + " + fieldParam.AddressOffset.ToString();
            var templateOutputCalculator = new templateOutputCalculator { name = "TagAddress", Value = tagaddressvalue };
            var outputFields = new List<templateOutputField>
            {
                new templateOutputField { name = "name", key=true, Value = fieldParam.Prefix +"_{equipment.tagprefix}_" + fieldParam.Suffix, keySpecified = true },
                new templateOutputField { name = "type", Value = fieldParam.DataType },
                new templateOutputField { name = "unit", Value = "{equipment.IODEVICE}" },
                new templateOutputField { name = "eng_zero", Value = fieldParam.EngZero },
                new templateOutputField { name = "eng_full", Value = fieldParam.EngFull},
                new templateOutputField { name = "comment", Value = "{equipment.tagprefix} " + fieldParam.Comment },
                new templateOutputField { name = "cluster", key = true, Value = "{equipment.CLUSTER}" , keySpecified = true},
                new templateOutputField { name = "equip", Value = "{equipment.name}" },
                new templateOutputField { name = "addr", Value = "%MW{TagAddress}" },
                new templateOutputField { name = "raw_zero", Value = fieldParam.RawZero },
                new templateOutputField { name = "raw_full", Value = fieldParam.RawFull },
                new templateOutputField { name = "eng_units", Value = fieldParam.Units },
                new templateOutputField { name = "format", Value = fieldParam.Format },
                new templateOutputField { name = "item", Value = fieldParam.Suffix.ToLower() },
                new templateOutputField { name = "taggenlink", load = true, Value = fieldParam.TagGenLink, loadSpecified = true },
                new templateOutputField { name = "linked", Value = "1"},
                new templateOutputField { name = "editcode", Value = "11" },
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
            templateOutput templateOutput = new templateOutput { name = "Alm." + fieldParam.Suffix, file = "digalm.dbf", filter = @"'{equipment.type}={type.name}'" };
            var outputFields = new List<templateOutputField>
            {

                new templateOutputField { name = "tag", key=true,Value = "{equipment.tagprefix}_" + fieldParam.Suffix  + "_Alm", keySpecified = true},
                new templateOutputField { name = "name", Value = "{equipment.tagprefix}"},
                new templateOutputField { name = "desc", Value = fieldParam.Comment},
                new templateOutputField { name = "VAR_A", Value = "{equipment.tagprefix}_" + fieldParam.Suffix},
                new templateOutputField{ name= "comment", Value=""},
                new templateOutputField { name = "cluster", key= true, Value = "{equipment.CLUSTER}", keySpecified = true },
                new templateOutputField{ name= "category", Value=""},
                new templateOutputField { name = "equip", Value = "{equipment.name}" },
                new templateOutputField { name = "item", Value = fieldParam.Suffix.ToLower() },
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
            templateOutput templateOutput = new templateOutput { name = "TRN." + fieldParam.Suffix, file = "trend.dbf", filter = @"'{equipment.type}={type.name}'" };
            var outputFields = new List<templateOutputField>
            {                
                new templateOutputField { name = "name", key=true, Value = fieldParam.Prefix +"_{equipment.tagprefix}_" + fieldParam.Suffix + "_TRN", keySpecified= true},
                new templateOutputField { name = "expr", Value = fieldParam.Prefix +"_{equipment.tagprefix}_" + fieldParam.Suffix},
                new templateOutputField {name="sampleper",Value=""},
                new templateOutputField{ name= "files", Value=""},
                new templateOutputField{ name= "time", Value=""},
                new templateOutputField{ name= "period", Value=""},
                new templateOutputField{ name= "comment", Value=fieldParam.Comment + " trend" },
                new templateOutputField { name="type", Value = "TRN_PERIODIC"},
                new templateOutputField {name = "stormethod", Value = "Floating Point (8-byte samples)"},
                new templateOutputField { name = "cluster", key= true, Value = "{equipment.CLUSTER}", keySpecified = true },                
                new templateOutputField { name = "equip", Value = "{equipment.name}" },
                new templateOutputField { name = "item", Value = fieldParam.Suffix.ToLower() },
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
                new templateParamString{name="parameter-definitions", Value = "BaseAddr.Status=;BaseAddr.Alarm=;BaseAddr.Analog="}
            };
            equipmentTypeParam.@string = paramstring.ToArray();
            return equipmentTypeParam;
        }
    }
}
