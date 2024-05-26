﻿using Grasshopper.Kernel;
using GTDrawingLink.Extensions;
using GTDrawingLink.Properties;
using GTDrawingLink.Tools;
using System.Drawing;
using Tekla.Structures.Drawing;

namespace GTDrawingLink.Components.AttributesComponents
{
    public class TextAttributesComponent : TeklaComponentBaseNew<TextAttributesCommand>
    {
        public override GH_Exposure Exposure => GH_Exposure.quarternary;
        protected override Bitmap Icon => Resources.TextAttributes;

        public TextAttributesComponent() : base(ComponentInfos.TextAttributesComponent) { }

        protected override void InvokeCommand(IGH_DataAccess DA)
        {
            var (textAttributes, fileName, fontAttributes, frame, arrowAttributes, backgroundTransparency, angle, rulerWidth, alignment) = _command.GetInputValues();

            textAttributes = textAttributes.Copy();
            if (!string.IsNullOrEmpty(fileName))
                textAttributes.LoadAttributes(fileName);

            if (fontAttributes != null)
                textAttributes.Font = fontAttributes;

            if (frame != null)
                textAttributes.Frame = frame;

            if (arrowAttributes != null)
                textAttributes.ArrowHead = arrowAttributes;

            if (backgroundTransparency.HasValue)
                textAttributes.TransparentBackground = backgroundTransparency.Value;

            if (angle.HasValue)
                textAttributes.Angle = angle.Value;

            if (alignment.HasValue)
                textAttributes.Alignment = alignment.Value;

            if (rulerWidth.HasValue)
            {
                textAttributes.RulerWidth = rulerWidth.Value;
                textAttributes.UseWordWrapping = rulerWidth.Value > 1;
            }

            _command.SetOutputValues(DA, textAttributes);
        }
    }
    public class TextAttributesCommand : CommandBase
    {
        private readonly InputOptionalParam<Text.TextAttributes> _inTextAttributes = new InputOptionalParam<Text.TextAttributes>(ParamInfos.TextAttributes);
        private readonly InputOptionalParam<string> _inAttributesFileName = new InputOptionalParam<string>(ParamInfos.Attributes);
        private readonly InputOptionalParam<FontAttributes> _inFontAttributes = new InputOptionalParam<FontAttributes>(ParamInfos.FontAttributes);
        private readonly InputOptionalParam<Frame> _inFrameType = new InputOptionalParam<Frame>(ParamInfos.FrameAtributes);
        private readonly InputOptionalParam<ArrowheadAttributes> _inArrowAttributes = new InputOptionalParam<ArrowheadAttributes>(ParamInfos.ArrowAttributes);
        private readonly InputOptionalStructParam<bool> _inBackgroundTransparency = new InputOptionalStructParam<bool>(ParamInfos.BackgroundTransparency);
        private readonly InputOptionalStructParam<double> _inAngle = new InputOptionalStructParam<double>(ParamInfos.Angle);
        private readonly InputOptionalStructParam<double> _inTextRulerWidth = new InputOptionalStructParam<double>(ParamInfos.TextRulerWidth);
        private readonly InputOptionalStructParam<TextAlignment> _inAlignment = new InputOptionalStructParam<TextAlignment>(ParamInfos.TextAlignment);

        private readonly OutputParam<Text.TextAttributes> _outAttributes = new OutputParam<Text.TextAttributes>(ParamInfos.TextAttributes);

        internal (Text.TextAttributes textAttributes, string? attributesName, FontAttributes? fontAttributes, Frame? frame, ArrowheadAttributes? arrowAttributes, bool? backgroundTransparency, double? angle, double? rulerWidth, TextAlignment? alignment) GetInputValues()
        {
            return (
                _inTextAttributes.Value ?? new Text.TextAttributes(),
                _inAttributesFileName.GetValueFromUserOrNull(),
                _inFontAttributes.GetValueFromUserOrNull(),
                _inFrameType.GetValueFromUserOrNull(),
                _inArrowAttributes.GetValueFromUserOrNull(),
                _inBackgroundTransparency.GetValueFromUserOrNull(),
                _inAngle.GetValueFromUserOrNull(),
                _inTextRulerWidth.GetValueFromUserOrNull(),
                _inAlignment.GetValueFromUserOrNull());
        }

        internal Result SetOutputValues(IGH_DataAccess DA, Text.TextAttributes attributes)
        {
            _outAttributes.Value = attributes;

            return SetOutput(DA);
        }
    }
}
