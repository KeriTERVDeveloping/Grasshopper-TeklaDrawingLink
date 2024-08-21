﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using GTDrawingLink.Tools;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Tekla.Structures.Drawing;

namespace GTDrawingLink.Types
{
    public class TeklaDatabaseObjectParam : TeklaParamBase<DatabaseObject>
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override Bitmap Icon => Properties.Resources.DrawingPart;

        protected bool _isFloatingParam;

        public TeklaDatabaseObjectParam(GH_InstanceDescription tag)
            : base(tag)
        {
        }

        public TeklaDatabaseObjectParam(GH_InstanceDescription tag, GH_ParamAccess access)
            : this(tag)
        {
            Access = access;
        }

        public override void CreateAttributes()
        {
            m_attributes = new TeklaDatabaseObjectParamAttributes(this);
        }

        protected override GH_Goo<DatabaseObject> InstantiateT()
        {
            return new TeklaDatabaseObjectGoo();
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_Goo<DatabaseObject> value)
        {
            var drawingObject = DrawingInteractor.PickObject();

            if (drawingObject == null)
                return GH_GetterResult.cancel;
            
            value = new TeklaDatabaseObjectGoo(drawingObject);
            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GH_Goo<DatabaseObject>> values)
        {
            var drawingObjects = DrawingInteractor.PickObjects();

            if (drawingObjects == null)
                return GH_GetterResult.cancel;

            values = new List<GH_Goo<DatabaseObject>>();
            foreach (var drawingObject in drawingObjects)
                values.Add(new TeklaDatabaseObjectGoo(drawingObject));

            return GH_GetterResult.success;
        }
        public void HighlightObjects()
        {
            DrawingInteractor.Highlight(
                GetReferencedDatabaseObjects()
                .Where(d => d is DrawingObject)
                .Cast<DrawingObject>());
        }

        public void UnHighlightObjects()
        {
            DrawingInteractor.UnHighlight();
        }
        public virtual IEnumerable<DatabaseObject> GetReferencedDatabaseObjects()
        {
            return from d in base.VolatileData.AllData(skipNulls: true)
                   select ((GH_Goo<DatabaseObject>)d).Value;
        }
    }
}
