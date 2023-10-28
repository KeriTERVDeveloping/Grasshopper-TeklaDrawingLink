﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using GTDrawingLink.Extensions;
using GTDrawingLink.Tools;
using GTDrawingLink.Types;
using Rhino.Geometry;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Tekla.Structures.Drawing;
using TSG = Tekla.Structures.Geometry3d;
using TSM = Tekla.Structures.Model;

namespace GTDrawingLink.Components
{
    public class GetBoltPropertiesComponent : TeklaComponentBaseNew<GetBoltPropertiesCommand>
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override Bitmap Icon => Properties.Resources.BoltProperties;

        public GetBoltPropertiesComponent() : base(ComponentInfos.GetBoltPropertiesComponent) { }

        protected override void InvokeCommand(IGH_DataAccess DA)
        {
            var bolt = _command.GetInputValues();
            if (bolt is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input object could not be casted to Bolt");
                return;
            }

            var boltPositions = GetBoltPositions(bolt.BoltPositions);

            _command.SetOutputValues(DA,
                                     bolt.BoltSize,
                                     bolt.BoltStandard,
                                     bolt.BoltType.ToString(),
                                     boltPositions);
        }

        private List<Point3d> GetBoltPositions(ArrayList boltPositions)
        {
            var points = new List<Point3d>();
            foreach (TSG.Point point in boltPositions)
            {
                points.Add(point.ToRhino());
            }

            return points;
        }
    }

    public class GetBoltPropertiesCommand : CommandBase
    {
        private readonly InputParam<object> _inTeklaObject = new InputParam<object>(ParamInfos.BoltObject);

        private readonly OutputParam<double> _outSize = new OutputParam<double>(ParamInfos.BoltSize);
        private readonly OutputParam<string> _outStandard = new OutputParam<string>(ParamInfos.BoltStandard);
        private readonly OutputParam<string> _outType = new OutputParam<string>(ParamInfos.BoltType);
        private readonly OutputListParam<Point3d> _outPositions = new OutputListParam<Point3d>(ParamInfos.BoltPositions);

        internal TSM.BoltGroup? GetInputValues()
        {
            return GetBoltFromInput(_inTeklaObject.Value);
        }

        internal Result SetOutputValues(IGH_DataAccess DA, double size, string standard, string type, List<Point3d> positions)
        {
            _outSize.Value = size;
            _outStandard.Value = standard;
            _outType.Value = type;
            _outPositions.Value = positions;

            return SetOutput(DA);
        }

        private TSM.BoltGroup? GetBoltFromInput(object inputObject)
        {
            if (inputObject is GH_Goo<TSM.ModelObject> modelGoo)
            {
                return (modelGoo.Value) as TSM.BoltGroup;
            }
            else if (inputObject is TeklaDatabaseObjectGoo drawingObject)
            {
                var drawingModelObject = drawingObject.Value as ModelObject;
                if (drawingModelObject is null)
                    return null;

                var modelObject = ModelInteractor.GetModelObject(drawingModelObject.ModelIdentifier);
                return modelObject as TSM.BoltGroup;
            }

            return null;
        }
    }
}
