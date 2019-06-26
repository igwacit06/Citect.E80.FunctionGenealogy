using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citect.E80.EquipmentTypeXml
{
    public class TomPriceEquipmentTemplate
    {
        /// <summary>
        /// Define Input section of Equipment Type xml template
        /// </summary>
        /// <param name="dbfname"></param>
        /// <returns></returns>
        public static templateInput GetEquipmentType_Input(string dbfname)
        {
            var inputFields = new List<string> { "NAME", "CLUSTER", "TYPE", "AREA", "LOCATION", "COMMENT", "IODEVICE" };
            var equipmentTypeInput = new templateInput { name = dbfname, file = "equip.dbf", desc = "Equipment Database" };
            var equipmentTypeInputFields = new List<templateInputField>();
            inputFields.ForEach(s => equipmentTypeInputFields.Add(new templateInputField { name = s }));
            equipmentTypeInput.field = equipmentTypeInputFields.ToArray();

            //add array specifics
            var inputArray = new List<templateInputArray> {
            new templateInputArray { name = "", Value = "" },
            new templateInputArray {name = "", Value =""}
            };

            equipmentTypeInput.array = inputArray.ToArray();

            return equipmentTypeInput;
        }

        /// <summary>
        /// Define Output section of Equipment Type xml template (variables)
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <param name="outputType"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static templateOutput GetEquipmentType_VarOutputs(string name, string prefix, string suffix, string outputType, string comment)
        {
            templateOutput templateOutput = new templateOutput { name = "Var." + suffix, file = "variable.dbf", filter = @"'{equipment.type}={type.name}'" };

            var templateOutputCalculator = new templateOutputCalculator { name = "TagAddress", Value = "{equipment.BaseAddr[Status]}" };

            var outputFields = new List<templateOutputField>
            {
                new templateOutputField { name = "taggenlink", load = true, Value = string.Format("{0}.{1}", name, suffix) },
                new templateOutputField { name = "name", key=true, Value = prefix +"_{equipment.tagprefix}_" + suffix },
                new templateOutputField { name = "item", Value = suffix },
                new templateOutputField { name = "type", Value = outputType },
                new templateOutputField { name = "cluster", key = true, Value = "{equipment.CLUSTER}" },
                new templateOutputField { name = "unit", Value = "{equipment.IODEVICE}" },
                new templateOutputField { name = "equip", Value = "{equipment.name}" },
                new templateOutputField { name = "comment", Value = "{equipment.tagprefix} " + comment },
                new templateOutputField {name = "addr", Value = "TagAddress"},
                new templateOutputField { name = "linked", Value = "1"},
                new templateOutputField { name = "editcode", Value = "11" }

            };
            templateOutput.field = outputFields.ToArray();
            templateOutput.calculator = templateOutputCalculator;
            return templateOutput;
        }

        /// <summary>
        /// Define Output section of Equipment Type xml template (variables)
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <param name="outputType"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static templateOutput GetEquipmentType_VarOutputs(EquipmentTypeOutputParam fieldParam)
        {
            templateOutput templateOutput = new templateOutput { name = "Var." + fieldParam.Prefix, file = fieldParam.FileName, filter = @"'{equipment.type}={type.name}'" };
            var outputFields = new List<templateOutputField>
            {
                new templateOutputField { name = "taggenlink", load = true, Value = fieldParam.TagGenLink },
                new templateOutputField { name = "name", key=true, Value = fieldParam.Prefix +"_{equipment.tagprefix}_" + fieldParam.Suffix },
                new templateOutputField { name = "item", Value = fieldParam.Suffix },
                new templateOutputField { name = "type", Value = fieldParam.DataType },
                new templateOutputField { name = "cluster", key = true, Value = "{equipment.CLUSTER}" },
                new templateOutputField { name = "unit", Value = "{equipment.IODEVICE}" },
                new templateOutputField { name = "equip", Value = "{equipment.name}" },
                new templateOutputField { name = "comment", Value = "{equipment.tagprefix} " + fieldParam.Comment },
                new templateOutputField { name = "linked", Value = "1"},
                new templateOutputField { name = "editcode", Value = "11" },

            };
            templateOutput.field = outputFields.ToArray();
            return templateOutput;
        }


        /// <summary>
        /// Define Output section of Equipment Type xml template (digalm)
        /// </summary>
        /// <param name="equipmenttype"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static templateOutput GetEquipmentType_DigAlmOutputs(string equipmenttype, string suffix)
        {
            templateOutput templateOutput = new templateOutput { name = "Alm." + suffix, file = "digalm.dbf", filter = @"'{equipment.type}={type.name}'" };
            var outputFields = new List<templateOutputField>
            {
                new templateOutputField { name = "taggenlink", load = true, Value= equipmenttype + "." + suffix },
                new templateOutputField { name = "tag", key=true,Value = "{equipment.tagprefix}_" + suffix  + "_Alm"},
                new templateOutputField { name = "name", Value = "{equipment.tagprefix}"},
                new templateOutputField { name = "item", Value = suffix },
                new templateOutputField { name = "VAR_A", Value = "{equipment.tagprefix}_" + suffix},
                new templateOutputField { name = "cluster", key= true, Value = "{equipment.CLUSTER}" },
                new templateOutputField{ name= "category", Value=""},
                new templateOutputField { name = "equip", Value = "{equipment.name}" },
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
                new templateParamString{ name = "ref",  Value = refName},
                new templateParamString{name="paramter-definitions", Value = "BaseAddr.Status=;BaseAddr.Alarm=;BaseAddr.Analog="}
            };
            equipmentTypeParam.@string = paramstring.ToArray();
            return equipmentTypeParam;
        }
    }
}
