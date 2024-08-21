﻿using Grasshopper.Kernel;
using GTDrawingLink.Tools;
using GTDrawingLink.Types;
using System.Drawing;
using Tekla.Structures.Drawing;

namespace GTDrawingLink.Components.Parts
{
    public class ConvertDrawingToModelObjectComponent : TeklaComponentBase
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override Bitmap Icon => Properties.Resources.ConvertDrawingToModelObject;

        public ConvertDrawingToModelObjectComponent() : base(ComponentInfos.ConvertDrawingToModelObjectComponent)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            AddTeklaDbObjectParameter(pManager, ComponentInfos.DrawingPartParam, GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model Object", "MO", "Tekla model object", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object input = null;
            DA.GetData(ComponentInfos.DrawingPartParam.Name, ref input);

            if (input is TeklaDatabaseObjectGoo)
            {
                var databaseObject = (input as TeklaDatabaseObjectGoo).Value;
                if (databaseObject is ModelObject drawingObject)
                {
                    var modelObject = ModelInteractor.GetModelObject(drawingObject.ModelIdentifier);

                    DA.SetData(0, new ModelObjectGoo(modelObject));
                }
            }
        }
    }
}
