﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using GTDrawingLink.Extensions;
using GTDrawingLink.Tools;
using GTDrawingLink.Types;
using System.Drawing;

namespace GTDrawingLink.Components.Obsolete
{
    public class TransformPointToLocalCSComponent : TeklaComponentBase
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        protected override Bitmap Icon => Properties.Resources.TransformPointToLocal;

        public TransformPointToLocalCSComponent() : base(ComponentInfos.TransformPointToLocalCS)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "P", "Point to transform", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "P", "Point after transformation", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Rhino.Geometry.Point3d point = new Rhino.Geometry.Point3d();
            var parameterSet = DA.GetData("Point", ref point);
            if (!parameterSet)
                return;

            var teklaPoint = point.ToTekla();

            var matrix = ModelInteractor.Model
                .GetWorkPlaneHandler()
                .GetCurrentTransformationPlane()
                .TransformationMatrixToLocal;

            var resultPoint = matrix.Transform(teklaPoint);

            DA.SetData("Point", new GH_Point(resultPoint.ToRhino()));
        }
    }
}
