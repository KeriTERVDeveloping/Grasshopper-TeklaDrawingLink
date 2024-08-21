﻿using Grasshopper.Kernel;
using GTDrawingLink.Extensions;
using GTDrawingLink.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using Tekla.Structures.Drawing;

namespace GTDrawingLink.Components.Obsolete
{
    [Obsolete]
    public class ModifyRebarComponentOLD : TeklaComponentBaseNew<ModifyRebarCommandOLD>
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        protected override Bitmap Icon => Properties.Resources.ModifyRebar;
        public ModifyRebarComponentOLD() : base(ComponentInfos.ModifyRebarComponent) { }

        protected override void InvokeCommand(IGH_DataAccess DA)
        {
            (List<ReinforcementBase> reinforcements, List<ReinforcementBase.ReinforcementSingleAttributes> attributes) = _command.GetInputValues();

            for (int i = 0; i < reinforcements.Count; i++)
            {
                ApplyAttributes(reinforcements[i], attributes.ElementAtOrLast(i));
            }

            DrawingInteractor.CommitChanges();

            _command.SetOutputValues(DA, reinforcements);
        }

        private void ApplyAttributes(ReinforcementBase reinforcementBase, ReinforcementBase.ReinforcementSingleAttributes attributes)
        {
            if (reinforcementBase is ReinforcementSingle single)
            {
                single.Attributes = attributes;
            }
            else if (reinforcementBase is ReinforcementGroup group)
            {
                group.Attributes = CastToGroupAttributes(attributes);
            }
            else if (reinforcementBase is ReinforcementSetGroup set)
            {
                set.Attributes = CastToSetAttributes(attributes);
            }
            else if (reinforcementBase is ReinforcementStrand strand)
            {
                strand.Attributes = CastToStrandAttributes(attributes);
            }

            reinforcementBase.Modify();
        }

        private ReinforcementBase.ReinforcementGroupAttributes CastToGroupAttributes(ReinforcementBase.ReinforcementSingleAttributes attributes)
        {
            return new ReinforcementBase.ReinforcementGroupAttributes()
            {
                CustomPresentation = attributes.CustomPresentation,
                HiddenLines = attributes.HiddenLines,
                HideLinesHiddenByPart = attributes.HideLinesHiddenByPart,
                HideLinesHiddenByReinforcement = attributes.HideLinesHiddenByReinforcement,
                HookedEndSymbolType = attributes.HookedEndSymbolType,
                ReinforcementRepresentation = attributes.ReinforcementRepresentation,
                ReinforcementVisibility = attributes.ReinforcementVisibility,
                StraightEndSymbolType = attributes.StraightEndSymbolType,
                VisibleLines = attributes.VisibleLines
            };
        }

        private ReinforcementBase.ReinforcementSetGroupAttributes CastToSetAttributes(ReinforcementBase.ReinforcementSingleAttributes attributes)
        {
            return new ReinforcementBase.ReinforcementSetGroupAttributes()
            {
                CustomPresentation = attributes.CustomPresentation,
                HiddenLines = attributes.HiddenLines,
                HideLinesHiddenByPart = attributes.HideLinesHiddenByPart,
                HideLinesHiddenByReinforcement = attributes.HideLinesHiddenByReinforcement,
                HookedEndSymbolType = attributes.HookedEndSymbolType,
                ReinforcementRepresentation = attributes.ReinforcementRepresentation,
                ReinforcementVisibility = attributes.ReinforcementVisibility,
                StraightEndSymbolType = attributes.StraightEndSymbolType,
                VisibleLines = attributes.VisibleLines
            };
        }

        private ReinforcementBase.ReinforcementStrandAttributes CastToStrandAttributes(ReinforcementBase.ReinforcementSingleAttributes attributes)
        {
            return new ReinforcementBase.ReinforcementStrandAttributes()
            {
                CustomPresentation = attributes.CustomPresentation,
                HiddenLines = attributes.HiddenLines,
                HideLinesHiddenByPart = attributes.HideLinesHiddenByPart,
                HideLinesHiddenByReinforcement = attributes.HideLinesHiddenByReinforcement,
                HookedEndSymbolType = attributes.HookedEndSymbolType,
                ReinforcementRepresentation = attributes.ReinforcementRepresentation,
                ReinforcementVisibility = attributes.ReinforcementVisibility,
                StraightEndSymbolType = attributes.StraightEndSymbolType,
                VisibleLines = attributes.VisibleLines
            };
        }
    }

    public class ModifyRebarCommandOLD : CommandBase
    {
        private readonly InputListParam<ReinforcementBase> _inReinforcements = new InputListParam<ReinforcementBase>(ParamInfos.Reinforcement);
        private readonly InputListParam<ReinforcementBase.ReinforcementSingleAttributes> _inAttributes = new InputListParam<ReinforcementBase.ReinforcementSingleAttributes>(ParamInfos.RebarAtributes);


        private readonly OutputListParam<ReinforcementBase> _outReinforcements = new OutputListParam<ReinforcementBase>(ParamInfos.Reinforcement);

        internal (List<ReinforcementBase> reinforcements, List<ReinforcementBase.ReinforcementSingleAttributes> attributes) GetInputValues()
        {
            return (_inReinforcements.Value, _inAttributes.Value);
        }

        internal Result SetOutputValues(IGH_DataAccess DA, List<ReinforcementBase> reinforcements)
        {
            _outReinforcements.Value = reinforcements;

            return SetOutput(DA);
        }
    }
}
