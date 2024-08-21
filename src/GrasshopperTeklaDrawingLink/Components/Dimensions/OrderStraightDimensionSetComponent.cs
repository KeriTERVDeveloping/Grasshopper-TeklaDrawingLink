﻿using Grasshopper.Kernel;
using GTDrawingLink.Extensions;
using GTDrawingLink.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;

namespace GTDrawingLink.Components.Dimensions
{
    public class OrderStraightDimensionSetComponent : TeklaComponentBaseNew<OrderStraightDimensionSetCommand>
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.OrderStraightDimensionSets;
        public OrderStraightDimensionSetComponent() : base(ComponentInfos.OrderStraightDimensionSetComponent) { }

        protected override void InvokeCommand(IGH_DataAccess DA)
        {
            var (dimensionLines, distance) = _command.GetInputValues();
            if (dimensionLines.Count < 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Ordering is possible only for two or more dimension lines");
            }
            else
            {
                dimensionLines.ForEach(d => d.Select());
                OrderDimensions(dimensionLines, distance);
            }

            _command.SetOutputValues(DA, dimensionLines);
        }

        private void OrderDimensions(List<StraightDimensionSet> dimensionLines, int offset)
        {
            var dimLinesWithDistances = CalculateDimensionLinePositions(dimensionLines);
            var orderVector = GetOrderVector(dimensionLines.First());            

            var orderedDimLines = dimLinesWithDistances.OrderBy(d => d.distance).ToList();

            var locationDirection = orderedDimLines[0].location.GetDirectionVector();
            var expectedLocationStartPt = orderedDimLines[0].location.Point1;

            for (int i = 1; i < orderedDimLines.Count; i++)
            {
                expectedLocationStartPt += orderVector * offset;

                var (dimension, points, location, _) = orderedDimLines[i];

                var flattenedPt = new Point(points.First().X, points.First().Y);
                var newDistance = Distance.PointToLine(flattenedPt, new Tekla.Structures.Geometry3d.Line(expectedLocationStartPt, locationDirection));
                if (new Vector(expectedLocationStartPt - flattenedPt).Dot(dimension.GetUpDirection()) < 0)
                    newDistance *= -1;
                dimension.Distance = newDistance;
                dimension.Attributes.Placing.Placing = DimensionSetBaseAttributes.Placings.Fixed;
                dimension.Modify();
            }

            DrawingInteractor.CommitChanges();
        }

        private List<(StraightDimensionSet dimension, List<Point> points, LineSegment location, double distance)> CalculateDimensionLinePositions(List<StraightDimensionSet> dimensionLines)
        {
            var initialDim = dimensionLines.First();
            var initialPoints = initialDim.GetPoints().ToList();
            var initialSegment = initialDim.GetDimensionLocation(initialPoints);
            var initialLine = new Tekla.Structures.Geometry3d.Line(initialSegment);

            var dimLinesWithDistances = new List<(StraightDimensionSet dimension, List<Point> points, LineSegment location, double distance)>
            {
                (initialDim, initialPoints, initialSegment, 0)
            };

            var orderVector = GetOrderVector(initialDim);
            for (int i = 1; i < dimensionLines.Count; i++)
            {
                var dimLine = dimensionLines[i];
                var dimPoints = dimLine.GetPoints().ToList();
                var dimLocation = dimLine.GetDimensionLocation();
                var distance = Distance.PointToLine(dimLocation.Point1, initialLine);
                var orientation = new Vector(dimLocation.Point1 - Projection.PointToLine(dimLocation.Point1, initialLine));
                if (orderVector.Dot(orientation) < 0)
                    distance *= -1;

                dimLinesWithDistances.Add((dimLine, dimPoints, dimLocation, distance));
            }

            return dimLinesWithDistances;
        }

        private Vector GetOrderVector(StraightDimensionSet sds)
        {
            var signOfDistnace = Math.Sign(sds.Distance);
            return signOfDistnace*sds.GetUpDirection().GetNormal();
        }
    }

    public class OrderStraightDimensionSetCommand : CommandBase
    {
        private readonly InputListParam<StraightDimensionSet> _inDimensionLines = new InputListParam<StraightDimensionSet>(ParamInfos.StraightDimensionSets);
        private readonly InputStructParam<int> _inDistance = new InputStructParam<int>(ParamInfos.DistanceBetweenDimensions);

        private readonly OutputListParam<StraightDimensionSet> _outDimensionLine = new OutputListParam<StraightDimensionSet>(ParamInfos.StraightDimensionSets);

        internal (List<StraightDimensionSet> dimensionLines, int distance) GetInputValues()
        {
            return (
                _inDimensionLines.Value,
                _inDistance.Value);
        }

        internal Result SetOutputValues(IGH_DataAccess DA, List<StraightDimensionSet> dimensionLines)
        {
            _outDimensionLine.Value = dimensionLines;

            return SetOutput(DA);
        }
    }
}
