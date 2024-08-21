﻿using Grasshopper.Kernel;
using GTDrawingLink.Properties;
using GTDrawingLink.Tools;
using System.Drawing;
using Tekla.Structures.Drawing;

namespace GTDrawingLink.Components.Obsolete
{
    public class ArrowAttributesComponentOLD : TeklaComponentBaseNew<ArrowAttributesCommandOLD>
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        protected override Bitmap Icon => Resources.ArrowAttributes;

        public ArrowAttributesComponentOLD() : base(ComponentInfos.ArrowAttributesComponent) { }

        protected override void InvokeCommand(IGH_DataAccess DA)
        {
            var (head, width, height) = _command.GetInputValues();

            var arrowheadAttributes = new ArrowheadAttributes()
            {
                Head = head,
                Width = width,
                Height = height
            };

            _command.SetOutputValues(DA, arrowheadAttributes);
        }
    }

    public class ArrowAttributesCommandOLD : CommandBase
    {
        private readonly InputOptionalStructParam<ArrowheadTypes> _inArrowheadType = new InputOptionalStructParam<ArrowheadTypes>(ParamInfos.ArrowType, ArrowheadTypes.FilledArrow);
        private readonly InputOptionalStructParam<double> _inWidth = new InputOptionalStructParam<double>(ParamInfos.ArrowWidth, 1.0);
        private readonly InputOptionalStructParam<double> _inHeight = new InputOptionalStructParam<double>(ParamInfos.ArrowHeight, 2.0);

        private readonly OutputParam<ArrowheadAttributes> _outAttributes = new OutputParam<ArrowheadAttributes>(ParamInfos.ArrowAttributes);

        internal (ArrowheadTypes type, double width, double height) GetInputValues()
        {
            return (
                _inArrowheadType.Value,
                _inWidth.Value,
                _inHeight.Value);
        }

        internal Result SetOutputValues(IGH_DataAccess DA, ArrowheadAttributes attributes)
        {
            _outAttributes.Value = attributes;

            return SetOutput(DA);
        }
    }
}
