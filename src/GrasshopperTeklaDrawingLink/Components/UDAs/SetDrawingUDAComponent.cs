﻿using Grasshopper.Kernel;
using GTDrawingLink.Extensions;
using GTDrawingLink.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Tekla.Structures.Drawing;

namespace GTDrawingLink.Components.UDAs
{
    public class SetDrawingUDAComponent : TeklaComponentBase
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override Bitmap Icon => Properties.Resources.SetUDA;

        public SetDrawingUDAComponent() : base(ComponentInfos.SetDrawingUDAComponent)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            AddTeklaDbObjectParameter(pManager, ParamInfos.TeklaDatabaseObject, GH_ParamAccess.list);
            AddTextParameter(pManager, ParamInfos.UDAInput, GH_ParamAccess.list, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            AddTeklaDbObjectParameter(pManager, ParamInfos.TeklaDatabaseObject, GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var databaseObjects = DA.GetGooListValue<DatabaseObject>(ParamInfos.TeklaDatabaseObject);
            if (databaseObjects == null || !databaseObjects.Any())
                return;

            var udas = new List<string>();
            DA.GetDataList(ParamInfos.UDAInput.Name, udas);
            if (udas == null || !udas.Any())
                return;

            var attributes = new List<Tools.Attributes>();
            foreach (string uda in udas)
            {
                Tools.Attributes item = null;
                if (!string.IsNullOrWhiteSpace(uda))
                    item = Tools.Attributes.Parse(uda);

                attributes.Add(item);
            }

            for (int i = 0; i < databaseObjects.Count; i++)
            {
                var databaseObject = databaseObjects[i];
                var uDAs = attributes[Math.Min(i, attributes.Count - 1)];
                AttributesIO.SetUDAs(databaseObject, uDAs);
            }

            DA.SetDataList(ParamInfos.TeklaDatabaseObject.Name, databaseObjects);
        }
    }
}
