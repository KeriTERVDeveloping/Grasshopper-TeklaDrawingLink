﻿using Grasshopper.Kernel;
using GTDrawingLink.Extensions;
using GTDrawingLink.Tools;
using System.Drawing;
using System.Linq;
using Tekla.Structures.Drawing;

namespace GTDrawingLink.Components.Parts
{
    public class DeleteDrawingObjectsComponent : TeklaComponentBase
    {
        public override GH_Exposure Exposure => GH_Exposure.quarternary;
        protected override Bitmap Icon => Properties.Resources.DeleteObjects;

        public DeleteDrawingObjectsComponent() : base(ComponentInfos.DeleteDrawingObjectsComponent)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            AddTeklaDbObjectParameter(pManager, ParamInfos.TeklaDatabaseObject, GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            AddBooleanParameter(pManager, ParamInfos.DrawingObjectDeleteResult, GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var drawingObjects = DA.GetGooListValue<DatabaseObject>(ParamInfos.TeklaDatabaseObject);
            if (drawingObjects == null)
                return;

            var status = DrawingInteractor.DeleteObjects(drawingObjects.OfType<DrawingObject>());

            DrawingInteractor.CommitChanges();

            DA.SetData(ParamInfos.DrawingObjectDeleteResult.Name, status);
        }
    }
}
