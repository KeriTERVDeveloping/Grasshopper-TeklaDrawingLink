﻿using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using GTDrawingLink.Tools;
using GTDrawingLink.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Tekla.Structures.Model;

namespace GTDrawingLink.Components.Obsolete
{
    [Obsolete]
    public class GroupObjectsComponentOLD : TeklaComponentBase
    {
        private GroupingMode _mode = GroupingMode.ByAssemblyPosition;
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        protected override Bitmap Icon => Properties.Resources.GroupObjects;

        public GroupObjectsComponentOLD() : base(ComponentInfos.GroupObjectsComponent)
        {
            SetCustomMessage();
        }

        protected override void AppendAdditionalComponentMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, ParamInfos.GroupByPosition.Name, ByPositionMenuItem_Clicked, true, _mode == GroupingMode.ByAssemblyPosition).ToolTipText = ParamInfos.GroupByPosition.Description;
            Menu_AppendItem(menu, ParamInfos.GroupByName.Name, ByNameMenuItem_Clicked, true, _mode == GroupingMode.ByName).ToolTipText = ParamInfos.GroupByName.Description;
            Menu_AppendItem(menu, ParamInfos.GroupByClass.Name, ByClassMenuItem_Clicked, true, _mode == GroupingMode.ByClass).ToolTipText = ParamInfos.GroupByClass.Description;
            Menu_AppendItem(menu, ParamInfos.GroupByUDA.Name, ByUdaMenuItem_Clicked, true, _mode == GroupingMode.ByUDA).ToolTipText = ParamInfos.GroupByUDA.Description;
            Menu_AppendItem(menu, ParamInfos.GroupByReport.Name, ByReportMenuItem_Clicked, true, _mode == GroupingMode.ByReport).ToolTipText = ParamInfos.GroupByReport.Description;
            Menu_AppendItem(menu, ParamInfos.GroupByType.Name, ByTypeMenuItem_Clicked, true, _mode == GroupingMode.ByType).ToolTipText = ParamInfos.GroupByType.Description;
            Menu_AppendSeparator(menu);
        }

        private void ByPositionMenuItem_Clicked(object sender, EventArgs e)
        {
            _mode = GroupingMode.ByAssemblyPosition;
            SetCustomMessage();
            ExpireSolution(recompute: true);
        }

        private void ByNameMenuItem_Clicked(object sender, EventArgs e)
        {
            _mode = GroupingMode.ByName;
            SetCustomMessage();
            ExpireSolution(recompute: true);
        }

        private void ByClassMenuItem_Clicked(object sender, EventArgs e)
        {
            _mode = GroupingMode.ByClass;
            SetCustomMessage();
            ExpireSolution(recompute: true);
        }

        private void ByUdaMenuItem_Clicked(object sender, EventArgs e)
        {
            _mode = GroupingMode.ByUDA;
            SetCustomMessage();
            ExpireSolution(recompute: true);
        }

        private void ByReportMenuItem_Clicked(object sender, EventArgs e)
        {
            _mode = GroupingMode.ByReport;
            SetCustomMessage();
            ExpireSolution(recompute: true);
        }

        private void ByTypeMenuItem_Clicked(object sender, EventArgs e)
        {
            _mode = GroupingMode.ByType;
            SetCustomMessage();
            ExpireSolution(recompute: true);
        }

        private void SetCustomMessage()
        {
            switch (_mode)
            {
                case GroupingMode.ByAssemblyPosition:
                    Message = "Assembly Position";
                    break;
                case GroupingMode.ByName:
                    Message = "Name";
                    break;
                case GroupingMode.ByClass:
                    Message = "Class";
                    break;
                case GroupingMode.ByUDA:
                    Message = "UDA";
                    break;
                case GroupingMode.ByReport:
                    Message = "Report";
                    break;
                case GroupingMode.ByType:
                    Message = "Type";
                    break;
                default:
                    break;
            }
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32(ParamInfos.GroupByPosition.Name, (int)_mode);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            var serializedInt = 0;
            reader.TryGetInt32(ParamInfos.GroupByPosition.Name, ref serializedInt);
            _mode = (GroupingMode)serializedInt;
            SetCustomMessage();
            return base.Read(reader);
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            AddGenericParameter(pManager, ParamInfos.ModelObject, GH_ParamAccess.list);
            AddTextParameter(pManager, ParamInfos.PropertyName, GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            AddGenericParameter(pManager, ParamInfos.ModelObject, GH_ParamAccess.tree);
            AddTextParameter(pManager, ParamInfos.GroupingKeys, GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var inputObjects = new List<dynamic>();
            if (!DA.GetDataList(ParamInfos.ModelObject.Name, inputObjects))
                return;

            if (inputObjects.Any(i => i is null))
                return;

            var modelObjects = inputObjects.Select(i => i.Value as ModelObject).ToList();

            var propertyName = string.Empty;
            DA.GetData(ParamInfos.PropertyName.Name, ref propertyName);

            if (string.IsNullOrEmpty(propertyName) &&
                (_mode == GroupingMode.ByUDA || _mode == GroupingMode.ByReport))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Property name not specified");
                return;
            }

            var dictionary = new Dictionary<string, List<ModelObject>>();
            foreach (var modelObject in modelObjects)
            {
                if (modelObject is null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Provided object is not Tekla Model one");
                    continue;
                }

                string propertyValue = GetPropertyValue(modelObject, propertyName);
                if (dictionary.ContainsKey(propertyValue))
                    dictionary[propertyValue].Add(modelObject);
                else
                    dictionary.Add(propertyValue, new List<ModelObject> { modelObject });
            }

            var ordered = dictionary.OrderBy(kvp => kvp.Key).ToArray();

            DA.SetDataTree(0, GetOutputTree(DA.Iteration, ordered));
            DA.SetDataList(1, ordered.Select(d => d.Key));
        }

        private string GetPropertyValue(ModelObject inputObject, string inputProperty)
        {
            string propertyName;
            switch (_mode)
            {
                case GroupingMode.ByAssemblyPosition:
                    propertyName = "ASSEMBLY_POS";
                    break;
                case GroupingMode.ByName:
                    propertyName = inputObject is Assembly ? "ASSEMBLY_NAME" : "NAME";
                    break;
                case GroupingMode.ByClass:
                    propertyName = "CLASS_ATTR";
                    break;
                case GroupingMode.ByUDA:
                    propertyName = $"USERDEFINED.{inputProperty}";
                    break;
                case GroupingMode.ByReport:
                    propertyName = inputProperty;
                    break;
                case GroupingMode.ByType:
                    return inputObject.GetType().Name;
                default:
                    return "";
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                return "";
            }

            var strValue = "";
            if (inputObject.GetReportProperty(propertyName, ref strValue))
            {
                return strValue;
            }

            var dblValue = 0.0;
            if (inputObject.GetReportProperty(propertyName, ref dblValue))
            {
                return dblValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            var intValue = 0;
            if (inputObject.GetReportProperty(propertyName, ref intValue))
            {
                return intValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            return "";
        }

        private IGH_Structure GetOutputTree(int iteration, KeyValuePair<string, List<ModelObject>>[] dictionary)
        {
            var output = new GH_Structure<ModelObjectGoo>();

            var index = 0;
            foreach (var currentObjects in dictionary)
            {
                var indicies = currentObjects.Value.Select(o => new ModelObjectGoo(o));
                output.AppendRange(indicies, new GH_Path(iteration, index));

                index++;
            }

            return output;
        }

        [Flags]
        enum GroupingMode
        {
            ByAssemblyPosition = 0,
            ByName = 1,
            ByClass = 2,
            ByUDA = 4,
            ByReport = 8,
            ByType = 16
        }
    }
}
