﻿using Grasshopper.Kernel;
using GTDrawingLink.Extensions;
using GTDrawingLink.Properties;
using GTDrawingLink.Tools;
using GTDrawingLink.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Tekla.Structures.Drawing;

namespace GTDrawingLink.Components.Obsolete
{
    [Obsolete]
    public class ModifyPartComponentOLD : TeklaComponentBase
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        protected override Bitmap Icon => Resources.ModifyPart;

        public ModifyPartComponentOLD()
            : base(ComponentInfos.ModifyPartComponent)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            AddTeklaDbObjectParameter(pManager, ParamInfos.TeklaDrawingPart, GH_ParamAccess.list);
            AddOptionalParameter(pManager, new LineTypeAttributesParam(ParamInfos.VisibleLineTypeAttributes, GH_ParamAccess.list));
            AddOptionalParameter(pManager, new LineTypeAttributesParam(ParamInfos.HiddenLineTypeAttributes, GH_ParamAccess.list));
            AddOptionalParameter(pManager, new LineTypeAttributesParam(ParamInfos.ReferenceLineTypeAttributes, GH_ParamAccess.list));
            AddOptionalParameter(pManager, new ModelObjectHatchAttributesParam(ParamInfos.PartFacesHatchAttributes, GH_ParamAccess.list));
            AddOptionalParameter(pManager, new ModelObjectHatchAttributesParam(ParamInfos.SectionHatchAttributes, GH_ParamAccess.list));
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            AddTeklaDbObjectParameter(pManager, ParamInfos.TeklaDrawingPart, GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var drawingObjects = DA.GetGooListValue<DatabaseObject>(ParamInfos.TeklaDrawingPart).Cast<DrawingObject>().ToList();
            if (drawingObjects == null || drawingObjects.Count == 0)
                return;

            var visibileLines = DA.GetGooListValue<LineTypeAttributes>(ParamInfos.VisibleLineTypeAttributes);
            var hiddenLines = DA.GetGooListValue<LineTypeAttributes>(ParamInfos.HiddenLineTypeAttributes);
            var referenceLines = DA.GetGooListValue<LineTypeAttributes>(ParamInfos.ReferenceLineTypeAttributes);
            var faceHatches = DA.GetGooListValue<ModelObjectHatchAttributes>(ParamInfos.PartFacesHatchAttributes);
            var sectionHatches = DA.GetGooListValue<ModelObjectHatchAttributes>(ParamInfos.SectionHatchAttributes);

            if (!visibileLines.HasItems() && !hiddenLines.HasItems() && !referenceLines.HasItems()
                 && !faceHatches.HasItems() && !sectionHatches.HasItems())
                return;

            for (int i = 0; i < drawingObjects.Count; i++)
            {
                var drawingObject = drawingObjects[i];
                if (!(drawingObject is Part))
                    continue;

                var part = (Part)drawingObject;
                part.Select();

                var visibileLine = GetNthLine(visibileLines, i);
                if (visibileLine != null)
                    part.Attributes.VisibleLines = visibileLine;

                var hiddenLine = GetNthLine(hiddenLines, i);
                if (hiddenLine != null)
                    part.Attributes.HiddenLines = hiddenLine;

                var referenceLine = GetNthLine(referenceLines, i);
                if (referenceLine != null)
                    part.Attributes.ReferenceLine = referenceLine;

                var faceHatch = GetNthLine(faceHatches, i);
                if (faceHatch != null)
                    part.Attributes.FaceHatch = faceHatch;

                var sectionHatch = GetNthLine(sectionHatches, i);
                if (sectionHatch != null)
                    part.Attributes.SectionFaceHatch = sectionHatch;

                part.Modify();
            }

            DrawingInteractor.CommitChanges();

            DA.SetDataList(ParamInfos.TeklaDrawingPart.Name, drawingObjects.Select(d => new TeklaDatabaseObjectGoo(d)));
        }

        private T GetNthLine<T>(List<T> lineTypeAttributes, int index)
        {
            if (!lineTypeAttributes.HasItems())
                return default;

            if (lineTypeAttributes.Count - 1 < index)
                return lineTypeAttributes.Last();

            return lineTypeAttributes[index];
        }
    }
}
