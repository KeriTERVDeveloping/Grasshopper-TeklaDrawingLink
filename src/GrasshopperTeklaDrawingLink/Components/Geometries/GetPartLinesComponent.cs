﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using GTDrawingLink.Extensions;
using GTDrawingLink.Tools;
using System.Drawing;
using System.Linq;
using Tekla.Structures.Model;

namespace GTDrawingLink.Components.Geometries
{
    public class GetPartLinesComponent : TeklaComponentBase
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override Bitmap Icon => Properties.Resources.GetPartLines;

        public GetPartLinesComponent() : base(ComponentInfos.GetPartLinesComponent)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            AddGenericParameter(pManager, ParamInfos.ModelObject, GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            AddCurveParameter(pManager, ParamInfos.PartReferenceLine, GH_ParamAccess.item);
            AddCurveParameter(pManager, ParamInfos.PartCenterLine, GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            dynamic inputObject = null;
            if (!DA.GetData(ParamInfos.ModelObject.Name, ref inputObject))
                return;

            Part part = GetPartFromInput(inputObject.Value);
            if (part == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Part not found");
                return;
            }

            part.Select();
            var refPoints = part.GetReferenceLine(false).ToTeklaPoints();
            var centerPoints = part.GetCenterLine(false).ToTeklaPoints();

            DA.SetData(
                ParamInfos.PartReferenceLine.Name,
                new GH_Curve(new Rhino.Geometry.PolylineCurve(refPoints.Select(p => p.ToRhino()))));

            DA.SetData(
                ParamInfos.PartCenterLine.Name,
                new GH_Curve(new Rhino.Geometry.PolylineCurve(centerPoints.Select(p => p.ToRhino()))));
        }

        private Part GetPartFromInput(ModelObject modelObject)
        {
            switch (modelObject)
            {
                case Part inputPart:
                    return inputPart;
                case Assembly assembly:
                    return assembly.GetMainPart() as Part;
                default:
                    return null;
            }
        }
    }
}
