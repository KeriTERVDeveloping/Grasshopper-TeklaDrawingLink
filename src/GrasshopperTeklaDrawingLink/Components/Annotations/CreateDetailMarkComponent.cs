﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using GTDrawingLink.Extensions;
using GTDrawingLink.Tools;
using GTDrawingLink.Types;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;
using Tekla.Structures.Drawing;

namespace GTDrawingLink.Components.Annotations
{
    public class CreateDetailMarkComponent : CreateDatabaseObjectComponentBaseNew<CreateDetailMarkCommand>
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override Bitmap Icon => Properties.Resources.DetailMark;

        public CreateDetailMarkComponent() : base(ComponentInfos.CreateDetailMarkComponent) { }

        protected override IEnumerable<DatabaseObject> InsertObjects(IGH_DataAccess DA)
        {
            (var views, var centerPoints, var radiuses, var labelPoints, var attributes, var names) = _command.GetInputValues();
            if (!DrawingInteractor.IsInTheActiveDrawing(views.First()))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, Messages.Error_ViewFromDifferentDrawing);
                return null;
            }

            var strategy = GetSolverStrategy(false, centerPoints, radiuses, labelPoints, attributes, names);
            var inputMode = strategy.Mode;

            var outputTree = new GH_Structure<TeklaDatabaseObjectGoo>();
            var outputObjects = new List<DatabaseObject>();

            for (int i = 0; i < strategy.Iterations; i++)
            {
                var path = strategy.GetPath(i);

                var mark = InsertDetailMark(views.Get(path),
                                            centerPoints.Get(i, inputMode),
                                            radiuses.Get(i, inputMode),
                                            labelPoints.Get(i, inputMode),
                                            attributes.Get(i, inputMode),
                                            names.Get(i, inputMode));

                outputObjects.Add(mark);
                outputTree.Append(new TeklaDatabaseObjectGoo(mark), path);
            }

            _command.SetOutputValues(DA, outputTree);

            DrawingInteractor.CommitChanges();
            return outputObjects;
        }

        private static DetailMark InsertDetailMark(View view, Point3d centerPoint, double radius, Point3d labelPoint, string attribute, string name)
        {
            var boundaryPoint = centerPoint + new Point3d(radius, 0, 0);
            var detailAttributes = new DetailMark.DetailMarkAttributes(attribute)
            {
                MarkName = name
            };

            var mark = new DetailMark(view, centerPoint.ToTekla(), boundaryPoint.ToTekla(), labelPoint.ToTekla(), detailAttributes);
            mark.Insert();
            return mark;
        }
    }

    public class CreateDetailMarkCommand : CommandBase
    {
        private const double _defaultRadius = 500;
        private const string _defaultAttributes = "standard";

        private readonly InputListParam<View> _inView = new InputListParam<View>(ParamInfos.View);

        private readonly InputTreePoint _inCenterPoint = new InputTreePoint(ParamInfos.DetailCenterPoint);
        private readonly InputTreeNumber _inRadius = new InputTreeNumber(ParamInfos.DetailRadius, isOptional: true);
        private readonly InputTreePoint _inLabelPoint = new InputTreePoint(ParamInfos.DetailLabelPoint);
        private readonly InputTreeString _inAttributes = new InputTreeString(ParamInfos.DetailMarkAttributes, isOptional: true);
        private readonly InputTreeString _inName = new InputTreeString(ParamInfos.Name);

        private readonly OutputTreeParam<DatabaseObject> _outMark = new OutputTreeParam<DatabaseObject>(ParamInfos.Mark, 0);

        internal (ViewCollection<View> views, TreeData<Point3d> centerPoint, TreeData<double> radius, TreeData<Point3d> labelPoint, TreeData<string> attributes, TreeData<string> name) GetInputValues()
        {
            return (new ViewCollection<View>(_inView.Value),
                    _inCenterPoint.AsTreeData(),
                    _inRadius.IsEmpty() ? _inRadius.GetDefault(_defaultRadius) : _inRadius.AsTreeData(),
                    _inLabelPoint.AsTreeData(),
                    _inAttributes.IsEmpty() ? _inAttributes.GetDefault(_defaultAttributes) : _inAttributes.AsTreeData(),
                    _inName.AsTreeData());
        }

        internal Result SetOutputValues(IGH_DataAccess DA, IGH_Structure marks)
        {
            _outMark.Value = marks;

            return SetOutput(DA);
        }
    }
}
