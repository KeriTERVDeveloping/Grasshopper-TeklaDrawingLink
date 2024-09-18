﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using GTDrawingLink.Tools;
using System;
using Tekla.Structures.Drawing;

namespace GTDrawingLink.Types
{
    public class CircleAttributesParam : GH_Param<GH_Goo<Circle.CircleAttributes>>
    {
        public override Guid ComponentGuid => VersionSpecificConstants.GetGuid(GetType());

        public CircleAttributesParam(IGH_InstanceDescription tag)
            : base(tag)
        {
        }

        public CircleAttributesParam(IGH_InstanceDescription tag, GH_ParamAccess access)
            : base(tag, access)
        {
        }

        protected override GH_Goo<Circle.CircleAttributes> InstantiateT()
        {
            return new CircleAttributesGoo();
        }
    }
}
